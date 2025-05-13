using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mobileBackendsoftFount.Data;  // Ensure the correct namespace for ApplicationDbContext
using mobileBackendsoftFount.Models;  // Or the correct namespace where OilBuyReceipt is defined

using Microsoft.AspNetCore.Authorization;



namespace mobileBackendsoftFount.Controllers{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class OilBuyReceiptController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilBuyReceiptController(ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpPost]
        public async Task<IActionResult> CreateOilBuyReceipt([FromBody] OilBuyReceiptRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid data. Request body is required." });

            if (request.Date == null || request.Round == null || request.SupplierId == null)
                return BadRequest(new { message = "Invalid data. 'date', 'round', and 'supplierId' are required." });

            // Ensure Date is UTC
            request.Date = DateTime.SpecifyKind(request.Date.Value, DateTimeKind.Utc);
            request.MonthlyDate = new DateTime(request.Date.Value.Year, request.Date.Value.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // 🔹 Find last Monthly Buy Index in the same month
            var lastReceiptInMonth = await _context.OilBuyReceipts
                .Where(r => r.MonthlyDate.Year == request.MonthlyDate.Value.Year && r.MonthlyDate.Month == request.MonthlyDate.Value.Month)
                .OrderByDescending(r => r.MonthlyBuyIndex)
                .FirstOrDefaultAsync();

            // 🔹 Generate Monthly Buy Index
            int newMonthlyBuyIndex = (lastReceiptInMonth != null) ? lastReceiptInMonth.MonthlyBuyIndex + 1 : 1;

            // 🔹 Create empty receipt
            var newReceipt = new OilBuyReceipt
            {
                Date = request.Date.Value,  // Convert from nullable to non-nullable
                Round = request.Round.Value,
                SupplierId = request.SupplierId.Value,
                MonthlyDate = request.MonthlyDate.Value,  // Convert from nullable to non-nullable
                MonthlyBuyIndex = newMonthlyBuyIndex,
                OilBuyProducts = new List<OilBuyProduct>(),  // Empty initially
                TotalValue = 0  // Will be calculated when adding products
            };

            _context.OilBuyReceipts.Add(newReceipt);
            await _context.SaveChangesAsync();

            return Ok(new { monthlyBuyIndex = newReceipt.MonthlyBuyIndex });
        }


        [HttpGet("monthly-indexes/{monthlyDate}")]
        public async Task<IActionResult> GetMonthlyBuyIndexes(string monthlyDate)
        {
            if (!DateTime.TryParseExact(monthlyDate, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedMonthlyDate))
            {
                return BadRequest("Invalid Monthly Date format. Use YYYY-MM.");
            }

            // 🔹 Convert to first day of the month
            parsedMonthlyDate = new DateTime(parsedMonthlyDate.Year, parsedMonthlyDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // 🔹 Get all MonthlyBuyIndex values for the given month
            var indexes = await _context.OilBuyReceipts
                .Where(r => r.MonthlyDate == parsedMonthlyDate)
                .Select(r => r.MonthlyBuyIndex)
                .ToListAsync();

            if (indexes == null || indexes.Count == 0)
                return NotFound("No Monthly Buy Indexes found for the given date.");

            return Ok(indexes);
        }

        [HttpDelete("delete/{monthlyDate}/{monthlyBuyIndex}")]
        public async Task<IActionResult> DeleteOilBuyReceiptByMonthlyDate(string monthlyDate, int monthlyBuyIndex)
        {
            if (!DateTime.TryParseExact(monthlyDate, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedMonthlyDate))
            {
                return BadRequest("Invalid Monthly Date format. Use YYYY-MM.");
            }

            // 🔹 Convert to first day of the month
            parsedMonthlyDate = new DateTime(parsedMonthlyDate.Year, parsedMonthlyDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // 🔹 Find the receipt using Monthly Date & Monthly Buy Index
            var receipt = await _context.OilBuyReceipts
                                        .Include(r => r.OilBuyProducts)
                                        .FirstOrDefaultAsync(r => r.MonthlyDate == parsedMonthlyDate && r.MonthlyBuyIndex == monthlyBuyIndex);

            if (receipt == null)
                return NotFound("OilBuyReceipt not found for the given Monthly Date and Monthly Buy Index.");
            // 🔥 Delete linked oilAccountBalance if exists
            if (receipt.OilAccountBalanceId.HasValue)
            {
                var balance = await _context.oilAccountBalances.FindAsync(receipt.OilAccountBalanceId.Value);
                if (balance != null)
                {
                    _context.oilAccountBalances.Remove(balance);
                }
            }
            // 🔹 Remove the receipt
            _context.OilBuyReceipts.Remove(receipt);
            await _context.SaveChangesAsync();

            return NoContent(); // ✅ Successfully deleted
        }

        [HttpPost("add-products/{monthlyDate}/{monthlyBuyIndex}")]
        public async Task<IActionResult> ReplaceBuyOilProducts(string monthlyDate, int monthlyBuyIndex, [FromBody] List<OilBuyProductRequest> products)
        {
            if (!DateTime.TryParseExact(monthlyDate, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedMonthlyDate))
            {
                return BadRequest("Invalid Monthly Date format. Use YYYY-MM.");
            }

            if (products == null || products.Count == 0) 
                return BadRequest("No products provided.");

            parsedMonthlyDate = new DateTime(parsedMonthlyDate.Year, parsedMonthlyDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var receipt = await _context.OilBuyReceipts
                .Include(r => r.OilBuyProducts)
                .FirstOrDefaultAsync(r => r.MonthlyDate == parsedMonthlyDate && r.MonthlyBuyIndex == monthlyBuyIndex);

            if (receipt == null) 
                return NotFound("OilBuyReceipt not found for the given Monthly Date and Monthly Buy Index.");

            DateTime receiptDate = receipt.Date;

            var sellRecipe = await _context.OilSellRecipes
                .Include(sr => sr.OilSellProducts)
                .FirstOrDefaultAsync(sr => sr.Date.Date == receiptDate.Date);

            _context.OilBuyProducts.RemoveRange(receipt.OilBuyProducts);

            List<OilBuyProduct> newProducts = new List<OilBuyProduct>();
            foreach (var product in products)
            {
                var oil = await _context.Oils
                    .FirstOrDefaultAsync(o => o.Name == product.Name);

                if (oil == null)
                {
                    return BadRequest($"Oil '{product.Name}' not found.");
                }

                var newProduct = new OilBuyProduct
                {
                    Name = product.Name,
                    Amount = product.Amount,
                    PriceOfBuy = (decimal)oil.Price,  
                    Weight = oil.Weight,         
                    Value = product.Amount * (decimal)oil.Price,  
                    SupplierId = oil.SupplierId
                };

                newProducts.Add(newProduct);

                if (sellRecipe != null)
                {
                    var matchingSellProduct = sellRecipe.OilSellProducts.FirstOrDefault(sp => sp.Name == newProduct.Name);
                    if (matchingSellProduct != null)
                    {
                        matchingSellProduct.BoughtAmount = newProduct.Amount;
                        matchingSellProduct.BoughtRound = receipt.Round;
                        _context.Entry(matchingSellProduct).State = EntityState.Modified;

                        if (matchingSellProduct.BoughtAmount != 0)
                        {
                            if (matchingSellProduct.BoughtRound == 1)
                            {
                                matchingSellProduct.SoldAmount = matchingSellProduct.ReceiveAmount + matchingSellProduct.BoughtAmount - matchingSellProduct.RoundOneAmount +
                                                                (matchingSellProduct.RoundOneAmount - matchingSellProduct.RoundTwoAmount) +
                                                                (matchingSellProduct.RoundTwoAmount - matchingSellProduct.RoundThreeAmount);
                            }
                            else if (matchingSellProduct.BoughtRound == 2)
                            {
                                matchingSellProduct.SoldAmount = matchingSellProduct.ReceiveAmount - matchingSellProduct.RoundOneAmount +
                                                                (matchingSellProduct.RoundOneAmount + matchingSellProduct.BoughtAmount - matchingSellProduct.RoundTwoAmount) +
                                                                (matchingSellProduct.RoundTwoAmount - matchingSellProduct.RoundThreeAmount);
                            }
                            else if (matchingSellProduct.BoughtRound == 3)
                            {
                                matchingSellProduct.SoldAmount = (matchingSellProduct.ReceiveAmount - matchingSellProduct.RoundOneAmount) +
                                                                (matchingSellProduct.RoundOneAmount - matchingSellProduct.RoundTwoAmount) +
                                                                (matchingSellProduct.RoundTwoAmount + matchingSellProduct.BoughtAmount - matchingSellProduct.RoundThreeAmount);
                            }
                        }
                        else
                        {
                            matchingSellProduct.SoldAmount = (matchingSellProduct.ReceiveAmount - matchingSellProduct.RoundOneAmount) +
                                                            (matchingSellProduct.RoundOneAmount - matchingSellProduct.RoundTwoAmount) +
                                                            (matchingSellProduct.RoundTwoAmount - matchingSellProduct.RoundThreeAmount);
                        }

                        sellRecipe.TotalPrice = sellRecipe.OilSellProducts.Sum(p => p.SoldAmount * p.Price);
                        _context.Entry(sellRecipe).State = EntityState.Modified;
                    }
                }
            }

            receipt.OilBuyProducts = newProducts;
            // creating the balance
            var oldTotalValue = receipt.TotalValue;

            receipt.TotalValue = newProducts.Sum(p => p.Value);

            // 🔥 Update or create the linked oilAccountBalance
            if (receipt.OilAccountBalanceId.HasValue)
            {
                var balance = await _context.oilAccountBalances.FindAsync(receipt.OilAccountBalanceId.Value);
                if (balance != null)
                {
                    // Recalculate balance amount (adjust for the change in TotalValue)
                    balance.BalanceAmount = (balance.BalanceAmount + (oldTotalValue - receipt.TotalValue)); // Adjust based on value difference
                    _context.Entry(balance).State = EntityState.Modified;
                }
            }
            else
            {
                // 🔥 If no balance exists, create a new one
                var latestBalance = await _context.oilAccountBalances
                    .Where(b => b.DateTime <= receipt.Date)
                    .OrderByDescending(b => b.DateTime)
                    .ThenByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                decimal baseAmount = latestBalance?.BalanceAmount ?? 0;
                decimal depositAmount = (decimal)receipt.TotalValue;

                var newBalance = new oilAccountBalance
                {
                    BalanceAmount = baseAmount + depositAmount,
                    DateTime = receipt.Date
                };

                _context.oilAccountBalances.Add(newBalance);
                await _context.SaveChangesAsync();

                // Link the new balance to the receipt
                receipt.OilAccountBalanceId = newBalance.Id;
            }


            if (sellRecipe != null)
            {
                foreach (var product in sellRecipe.OilSellProducts)
                {
                    var oil = await _context.Oils.FirstOrDefaultAsync(o => o.Name == product.Name);
                    if (oil != null)
                    {
                        oil.Amount = (int)product.RoundThreeAmount;
                    }
                }
            }

            _context.Entry(receipt).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // 🔹 **Transform response to include SupplierName**
            var response = new
            {
                receipt.Id,
                receipt.Date,
                receipt.Round,
                receipt.SupplierId,
                receipt.MonthlyDate,
                receipt.MonthlyBuyIndex,
                OilBuyProducts = newProducts.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Amount,
                    p.PriceOfBuy,
                    p.Weight,
                    p.Value,
                    p.SupplierId,
                    SupplierName = _context.OilSuppliers.Where(s => s.Id == p.SupplierId).Select(s => s.Name).FirstOrDefault() ?? "Unknown"
                }).ToList(),
                receipt.TotalValue
            };

            return Ok(response);
        }

        // 🔹 Get Oil Buy Receipt by Monthly Date & Monthly Buy Index
        [HttpGet("monthly/{monthlyDate}/{monthlyBuyIndex}")]
        public async Task<ActionResult> GetOilBuyReceiptByMonthlyDate(string monthlyDate, int monthlyBuyIndex)
        {
            if (!DateTime.TryParseExact(monthlyDate, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedMonthlyDate))
            {
                return BadRequest("Invalid Monthly Date format. Use YYYY-MM.");
            }

            var receipt = await _context.OilBuyReceipts
                                        .Include(r => r.OilBuyProducts)
                                        .FirstOrDefaultAsync(r => r.MonthlyDate.Year == parsedMonthlyDate.Year &&
                                                                    r.MonthlyDate.Month == parsedMonthlyDate.Month &&
                                                                    r.MonthlyBuyIndex == monthlyBuyIndex);

            if (receipt == null) 
                return NotFound("OilBuyReceipt not found for the given Monthly Date and Monthly Buy Index.");

            // Fetch the supplier name for the receipt (if available)
            var supplierName = await _context.OilSuppliers
                                            .Where(s => s.Id == receipt.SupplierId)
                                            .Select(s => s.Name)
                                            .FirstOrDefaultAsync();

            // Fetch all supplier IDs from products
            var supplierIds = receipt.OilBuyProducts.Select(p => p.SupplierId).Distinct().ToList();

            // Fetch suppliers from the database
            var suppliers = await _context.OilSuppliers
                                        .Where(s => supplierIds.Contains(s.Id))
                                        .ToDictionaryAsync(s => s.Id, s => s.Name);

            // Modify products to include supplier name
            var productsWithSupplier = receipt.OilBuyProducts.Select(p => new
            {
                p.Id,
                p.Name,
                p.Amount,
                p.PriceOfBuy,
                p.Weight,
                p.Value,
                p.SupplierId,
                SupplierName = suppliers.ContainsKey(p.SupplierId) ? suppliers[p.SupplierId] : "Unknown Supplier"
            }).ToList();

            // Return a custom object with updated product data
            return Ok(new
            {
                receipt.Id,
                receipt.Date,
                receipt.Round,
                receipt.SupplierId,
                SupplierName = supplierName ?? "Unknown Supplier", // Add supplier name at the receipt level
                receipt.MonthlyDate,
                receipt.MonthlyBuyIndex,
                OilBuyProducts = productsWithSupplier, // Updated products with supplier names
                receipt.TotalValue
            });
        }


        // 🔹 Get all Oil Buy Receipts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OilBuyReceipt>>> GetAllOilBuyReceipts()
        {
            var receipts = await _context.OilBuyReceipts
                                        .Include(r => r.OilBuyProducts)
                                        .ToListAsync();

            if (receipts == null || receipts.Count == 0)
                return NotFound("No Oil Buy Receipts found.");

            return Ok(receipts);
        }

        // 🔹 Delete Oil Buy Receipt by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOilBuyReceipt(int id)
        {
            var receipt = await _context.OilBuyReceipts
                                        .Include(r => r.OilBuyProducts)
                                        .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null) 
                return NotFound("OilBuyReceipt not found.");

            _context.OilBuyReceipts.Remove(receipt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OilBuyReceipt>> GetOilBuyReceiptById(int id)
        {
            var receipt = await _context.OilBuyReceipts
                                        .Include(r => r.OilBuyProducts)
                                        .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null) return NotFound("OilBuyReceipt not found.");
            return receipt;
        }


    }

    // 🔹 DTO for creating a new Oil Buy Receipt
        public class OilBuyReceiptRequest
        {
            public DateTime? Date { get; set; }  // Nullable DateTime
            public int? Round { get; set; }      // Nullable int
            public int? SupplierId { get; set; } // Nullable int
            public DateTime? MonthlyDate { get; set; }  // Nullable DateTime
        }


    public class OilBuyProductRequest
    {
        public string Name { get; set; }  
        public decimal Amount { get; set; }  
        // public decimal PriceOfBuy { get; set; }  
        // public decimal Weight { get; set; }  
        public string SupplierName { get; set; } // ✅ Now using Supplier Name instead of ID
    }
}