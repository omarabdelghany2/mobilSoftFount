using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mobileBackendsoftFount.Data;  // Ensure the correct namespace for ApplicationDbContext
using mobileBackendsoftFount.Models;  // Or the correct namespace where OilBuyReceipt is defined





namespace mobileBackendsoftFount.Controllers{
    [Route("api/[controller]")]
    [ApiController]
    public class OilBuyReceiptController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilBuyReceiptController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Create a new Oil Buy Receipt
        [HttpPost]
        public async Task<IActionResult> CreateOilBuyReceipt([FromBody] OilBuyReceiptRequest request)
        {
            if (request == null) return BadRequest("Invalid data");

            // Ensure Date is UTC
            request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
            request.MonthlyDate = new DateTime(request.Date.Year, request.Date.Month, 1, 0, 0, 0, DateTimeKind.Utc); // First day of month

            // 🔹 Find last Monthly Buy Index in the same month
            var lastReceiptInMonth = await _context.OilBuyReceipts
                .Where(r => r.MonthlyDate.Year == request.MonthlyDate.Year && r.MonthlyDate.Month == request.MonthlyDate.Month)
                .OrderByDescending(r => r.MonthlyBuyIndex)
                .FirstOrDefaultAsync();

            // 🔹 Generate Monthly Buy Index
            int newMonthlyBuyIndex = (lastReceiptInMonth != null) ? lastReceiptInMonth.MonthlyBuyIndex + 1 : 1;

            // 🔹 Create empty receipt
            var newReceipt = new OilBuyReceipt
            {
                Date = request.Date,  // Full date (unique)
                Round = request.Round,
                SupplierId = request.SupplierId,
                MonthlyDate = request.MonthlyDate,  // First day of the month
                MonthlyBuyIndex = newMonthlyBuyIndex,
                OilBuyProducts = new List<OilBuyProduct>(),  // Empty initially
                TotalValue = 0  // Will be calculated when adding products
            };

            _context.OilBuyReceipts.Add(newReceipt);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOilBuyReceiptById), new { id = newReceipt.Id }, newReceipt);
        }

        [HttpPost("replace-products/{monthlyDate}/{monthlyBuyIndex}")]
        public async Task<IActionResult> ReplaceBuyOilProducts(string monthlyDate, int monthlyBuyIndex, [FromBody] List<OilBuyProductRequest> products)
        {
            if (!DateTime.TryParseExact(monthlyDate, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedMonthlyDate))
            {
                return BadRequest("Invalid Monthly Date format. Use YYYY-MM.");
            }

            if (products == null || products.Count == 0) 
                return BadRequest("No products provided.");

            // 🔹 Convert Monthly Date to First Day of the Month
            parsedMonthlyDate = new DateTime(parsedMonthlyDate.Year, parsedMonthlyDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // 🔹 Find the OilBuyReceipt using Monthly Date & Buy Index
            var receipt = await _context.OilBuyReceipts
                .Include(r => r.OilBuyProducts)
                .FirstOrDefaultAsync(r => r.MonthlyDate == parsedMonthlyDate && r.MonthlyBuyIndex == monthlyBuyIndex);

            if (receipt == null) 
                return NotFound("OilBuyReceipt not found for the given Monthly Date and Monthly Buy Index.");

            // 🔹 Remove all old products
            _context.OilBuyProducts.RemoveRange(receipt.OilBuyProducts);

            // 🔹 Add new products
            List<OilBuyProduct> newProducts = new List<OilBuyProduct>();
            foreach (var product in products)
            {
                // 🔹 Find Supplier by Name
                var supplier = await _context.OilSuppliers.FirstOrDefaultAsync(s => s.Name == product.SupplierName);
                if (supplier == null)
                {
                    return BadRequest($"Supplier '{product.SupplierName}' not found.");
                }

                newProducts.Add(new OilBuyProduct
                {
                    Name = product.Name,
                    Amount = product.Amount,
                    PriceOfBuy = product.PriceOfBuy,
                    Weight = product.Weight,
                    Value = product.Amount * product.PriceOfBuy,  // 🔹 Calculate Value
                    SupplierId = supplier.Id  // ✅ Now using SupplierId from the database
                });
            }

            // 🔹 Attach new products to receipt
            receipt.OilBuyProducts = newProducts;

            // 🔹 Recalculate Total Value
            receipt.TotalValue = newProducts.Sum(p => p.Value);

            _context.Entry(receipt).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(receipt);
        }



        // 🔹 Get Oil Buy Receipt by Monthly Date & Monthly Buy Index
        [HttpGet("monthly/{monthlyDate}/{monthlyBuyIndex}")]
        public async Task<ActionResult<OilBuyReceipt>> GetOilBuyReceiptByMonthlyDate(
            string monthlyDate, int monthlyBuyIndex)
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

            if (receipt == null) return NotFound("OilBuyReceipt not found for the given Monthly Date and Monthly Buy Index.");

            return receipt;
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
        public DateTime Date { get; set; }
        public int Round { get; set; }
        public int SupplierId { get; set; }
        public DateTime MonthlyDate { get; set; }  // Should be Year-Month (first day of month)
    }

    public class OilBuyProductRequest
    {
        public string Name { get; set; }  
        public decimal Amount { get; set; }  
        public decimal PriceOfBuy { get; set; }  
        public decimal Weight { get; set; }  
        public string SupplierName { get; set; } // ✅ Now using Supplier Name instead of ID
    }
}