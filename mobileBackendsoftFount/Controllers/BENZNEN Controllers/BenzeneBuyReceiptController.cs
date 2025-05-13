using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using Microsoft.AspNetCore.Authorization;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/buy-receipts")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class BenzeneBuyReceiptController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BenzeneBuyReceiptController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 1️⃣ Create a new Benzene Buy Receipt
        [HttpPost]
        public async Task<IActionResult> CreateBenzeneBuyReceipt([FromBody] BenzeneBuyReceiptRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid data" });

            // Validate required fields
            if (!request.MobilId.HasValue || request.MobilId <= 0)
                return BadRequest(new { message = "MobilId must be a positive number." });

            if (!request.MobilReceiptDate.HasValue)
                return BadRequest(new { message = "MobilReceiptDate is required." });

            if (!request.Date.HasValue)
                return BadRequest(new { message = "Date is required." });

            // Extract non-nullable values
            var mobilReceiptDate = request.MobilReceiptDate.Value;
            var date = request.Date.Value;

            // Convert DateTime fields to UTC
            mobilReceiptDate = DateTime.SpecifyKind(mobilReceiptDate, DateTimeKind.Utc);
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);


            // 🔹 Uniqueness Check
            bool receiptExists = await _context.BenzeneBuyReceipts
                .AnyAsync(r => r.MobilReceiptDate == mobilReceiptDate);

            if (receiptExists)
            {
                return BadRequest(new { message = "A receipt with this MobilReceiptDate already exists." });
            }

            int year = date.Year;
            int month = date.Month;

            // Find the last receipt created in the same year and month
            var lastReceipt = await _context.BenzeneBuyReceipts
                .Where(r => r.Date.Year == year && r.Date.Month == month)
                .OrderByDescending(r => r.IncrementalId)
                .FirstOrDefaultAsync();

            // Incremental ID logic
            int newIncrementalId = (lastReceipt != null) ? lastReceipt.IncrementalId + 1 : 1;

            var newReceipt = new BenzeneBuyReceipt
            {
                MobilId = request.MobilId.Value,
                MobilReceiptDate = mobilReceiptDate,
                Date = date,
                IncrementalId = newIncrementalId
            };

            _context.BenzeneBuyReceipts.Add(newReceipt);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReceiptById), new { id = newReceipt.Id }, newReceipt);
        }



        // 🔹 2️⃣ Get all Benzene Buy Receipts
        [HttpGet]
        public async Task<IActionResult> GetAllReceipts()
        {
            var receipts = await _context.BenzeneBuyReceipts
                .Include(r => r.Products)  // ✅ Ensure products are loaded
                .ToListAsync();

            return Ok(receipts);
        }


        // 🔹 3️⃣ Get a single Benzene Buy Receipt by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReceiptById(int id)
        {
            var receipt = await _context.BenzeneBuyReceipts.FindAsync(id);
            if (receipt == null) return NotFound();
            return Ok(receipt);
        }
            // // 🔹 Delete All Benzene Buy Receipts
            // [HttpDelete("delete-all")]
            // public async Task<IActionResult> DeleteAllReceipts()
            // {
            //     var allReceipts = _context.BenzeneBuyReceipts.ToList();

            //     if (!allReceipts.Any())
            //         return NotFound("No receipts found to delete");

            //     _context.BenzeneBuyReceipts.RemoveRange(allReceipts);
            //     await _context.SaveChangesAsync();

            //     return Ok(new { message = "All receipts deleted successfully", deletedCount = allReceipts.Count });
            // }




        [HttpGet("{yearMonth}/{incrementalId}")]
        public async Task<IActionResult> GetReceiptByIncrementalIdAndDate(string yearMonth, int incrementalId)
        {
            if (!DateTime.TryParseExact(yearMonth, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format. Use 'yyyy-MM'.");
            }

            var receipt = await _context.BenzeneBuyReceipts
                .Include(r => r.Products) // Ensure products are loaded
                .FirstOrDefaultAsync(r => r.IncrementalId == incrementalId && 
                                        r.Date.Year == parsedDate.Year &&
                                        r.Date.Month == parsedDate.Month);

            if (receipt == null)
                return NotFound("No receipt found with the given IncrementalId and Date.");

            return Ok(receipt);
        }

        // 🔹 4️⃣ Delete a Benzene Buy Receipt by ID
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteReceipt(int id)
        {
            var receipt = await _context.BenzeneBuyReceipts.FindAsync(id);
            if (receipt == null) return NotFound("Receipt not found");

            _context.BenzeneBuyReceipts.Remove(receipt);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Receipt deleted successfully" });
        }




        [HttpGet("GetIncrementalIdsByDate")]
        public async Task<IActionResult> GetIncrementalIdsByDate([FromQuery] DateTime date)
        {
            // Ensure the date is UTC for proper comparison
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            // Get all Incremental IDs for the given date
            var incrementalIds = await _context.BenzeneBuyReceipts
                .Where(r => r.Date.Date == date.Date)  // Compare only the date part
                .OrderBy(r => r.IncrementalId) // Sort by Incremental ID
                .Select(r => r.IncrementalId)  // Select only Incremental ID values
                .ToListAsync();

            // Return the list (empty list if no records found)
            return Ok(incrementalIds);
        }



        // [HttpPost("add-products")]
        // public async Task<IActionResult> AddProductsToReceipt([FromBody] AddProductsRequest request)
        // {
        //     if (request == null || request.Products == null || request.Products.Count == 0)
        //         return BadRequest("Invalid data. Products list cannot be empty.");

        //     // Ensure the date is UTC
        //     request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);

        //     // Find the receipt based on Date and IncrementalId
        //     var receipt = await _context.BenzeneBuyReceipts
        //         .Include(r => r.Products) // Ensure products are loaded
        //         .FirstOrDefaultAsync(r => r.Date.Year == request.Date.Year &&
        //                                 r.Date.Month == request.Date.Month &&
        //                                 r.IncrementalId == request.IncrementalId);

        //     if (receipt == null)
        //         return NotFound("Receipt not found.");

        //     // Add new products to the receipt and calculate total value
        //     var products = request.Products.Select(p => 
        //     {
        //         // Calculate product total value
        //         float valueBeforeEvaporation = p.Amount * p.PricePerLiter;
        //         float valueOfEvaporation = valueBeforeEvaporation * (p.EvaporationPercentage / 100);
        //         float totalProductValue = valueBeforeEvaporation - valueOfEvaporation + p.Taxes;

        //         return new BenzeneRecipeProduct
        //         {
        //             ProductName = p.ProductName,  // ✅ Added product name
        //             Amount = p.Amount,
        //             PricePerLiter = p.PricePerLiter,
        //             EvaporationPercentage = p.EvaporationPercentage,
        //             Taxes = p.Taxes,
        //             ValueOfEvaporation = valueOfEvaporation,  // Save calculated evaporation
        //             ValueOfTaxes = p.Taxes,  // Assuming taxes are pre-determined
        //             TotalValue = totalProductValue,  // ✅ Save calculated total value
        //             BenzeneBuyReceiptId = receipt.Id
        //         };
        //     }).ToList();

        //     _context.BenzeneRecipeProducts.AddRange(products);
        //     await _context.SaveChangesAsync();

        //     // ✅ **Recalculate the total value of the receipt**
        //     receipt.TotalValue = products.Sum(p => p.TotalValue);

        //     _context.BenzeneBuyReceipts.Update(receipt);
        //     await _context.SaveChangesAsync();

        //     return Ok(new { message = "Products added successfully", receipt });
        // }

        [HttpPost("add-products")]
        public async Task<IActionResult> AddProductsToReceipt([FromBody] AddProductsRequest request)
        {
            if (request == null || request.Products == null || request.Products.Count == 0)
                return BadRequest(new { message = "Invalid data. Products list cannot be empty." });

            // Ensure the date is UTC
            request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);

            // Find the receipt based on Date and IncrementalId
            var receipt = await _context.BenzeneBuyReceipts
                .Include(r => r.Products) // Ensure products are loaded
                .FirstOrDefaultAsync(r => r.Date.Year == request.Date.Year &&
                                        r.Date.Month == request.Date.Month &&
                                        r.IncrementalId == request.IncrementalId);

            if (receipt == null)
                return NotFound(new { message = "Receipt not found." });

            var products = new List<BenzeneRecipeProduct>();
            
            foreach (var product in request.Products)
            {
                if (product.Amount <= 0)
                    return BadRequest(new { message = "Product amount must be greater than zero." });

                // Retrieve the corresponding Benzene entry from the database
                var benzene = await _context.Benzenes.FirstOrDefaultAsync(b => b.Name == product.ProductName);
                if (benzene == null)
                    return NotFound(new { message = $"Benzene product '{product.ProductName}' not found." });
                
                // Use values from the Benzene table
                float pricePerLiter = benzene.PriceOfLitre;
                float evaporationPercentage = benzene.RateOfEvaporation;
                float taxes = benzene.RateOfTaxes;
                float vats = benzene.RateOfVats;

                // Calculate product total value
                float valueBeforeEvaporation = product.Amount * pricePerLiter;
                float valueOfTaxes = taxes * product.Amount * vats;
                float valueOfEvaporation = evaporationPercentage * product.Amount * pricePerLiter;
                float totalProductValue = valueBeforeEvaporation - valueOfEvaporation + valueOfTaxes;

                products.Add(new BenzeneRecipeProduct
                {
                    ProductName = product.ProductName,
                    Amount = product.Amount,
                    PricePerLiter = pricePerLiter,
                    EvaporationPercentage = evaporationPercentage,
                    Taxes = taxes,
                    ValueOfEvaporation = valueOfEvaporation,
                    ValueOfTaxes = valueOfTaxes,
                    TotalValue = totalProductValue,
                    BenzeneBuyReceiptId = receipt.Id
                });
            }

            _context.BenzeneRecipeProducts.AddRange(products);
            await _context.SaveChangesAsync();

            // Reload the receipt's products from the database to include the new ones
            await _context.Entry(receipt).Collection(r => r.Products).LoadAsync();

            // Recalculate the total value of the receipt including old and new products
            receipt.TotalValue = receipt.Products.Sum(p => p.TotalValue);

            _context.BenzeneBuyReceipts.Update(receipt);
            await _context.SaveChangesAsync();

            //changing the balanceee     ------->>>>>>>>>>>>>>
            // Now, find the latest balance that is before or equal to the receipt's date
            var latestBalance = await _context.Balances
                .Where(b => b.DateTime <= receipt.MobilReceiptDate) // Find balances on or before the given date
                .OrderByDescending(b => b.DateTime) // Order by DateTime descending (most recent date first)
                .ThenByDescending(b => b.Id) // If dates are the same, get the one with the largest Id (most recent by Id)
                .FirstOrDefaultAsync(); // Get the first (most recent) balance


            decimal currentBalance = latestBalance != null ? latestBalance.BalanceAmount : 0;

            // Subtract the total value of the updated receipt from the current balance (because it's a buy receipt)
            currentBalance -= (decimal)receipt.TotalValue;

            // Convert MobilReceiptDate to the correct time zone (e.g., if the receipt is in UTC)
            DateTime adjustedDateTime = TimeZoneInfo.ConvertTimeFromUtc(receipt.MobilReceiptDate, TimeZoneInfo.Utc);  // Adjust based on your time zone, e.g. EST, UTC, etc.

            // Create a new Balance entry after editing the products
            var newBalance = new Balance
            {
                BalanceAmount = currentBalance,
                DateTime = adjustedDateTime // Record the adjusted time
            };

            // Save the new balance
            _context.Balances.Add(newBalance);
            await _context.SaveChangesAsync();












            return Ok(new { message = "Products added successfully", receipt });

        }

        // [HttpPut("edit-products")]
        // public async Task<IActionResult> EditProductsInReceipt([FromBody] EditProductsRequest request)
        // {
        //     if (request == null || request.Products == null || request.Products.Count == 0)
        //         return BadRequest("Invalid data.");

        //     // Convert the date to UTC
        //     request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);

        //     // Find the receipt based on Date and IncrementalId
        //     var receipt = await _context.BenzeneBuyReceipts
        //         .Include(r => r.Products)
        //         .FirstOrDefaultAsync(r => r.Date.Year == request.Date.Year &&
        //                                 r.Date.Month == request.Date.Month &&
        //                                 r.IncrementalId == request.IncrementalId);

        //     if (receipt == null)
        //         return NotFound("Receipt not found.");

        //     // Update products in the receipt
        //     foreach (var productRequest in request.Products)
        //     {
        //         var product = receipt.Products.FirstOrDefault(p => p.Id == productRequest.Id);
        //         if (product != null)
        //         {
        //             product.ProductName = productRequest.ProductName;
        //             product.Amount = productRequest.Amount;
        //             product.PricePerLiter = productRequest.PricePerLiter;
        //             product.EvaporationPercentage = productRequest.EvaporationPercentage;
        //             product.Taxes = productRequest.Taxes;

        //             // Recalculate values
        //             float valueBeforeEvaporation = product.Amount * product.PricePerLiter;
        //             product.ValueOfEvaporation = valueBeforeEvaporation * (product.EvaporationPercentage / 100);
        //             product.TotalValue = valueBeforeEvaporation - product.ValueOfEvaporation + product.Taxes;
        //         }
        //     }

        //     // Recalculate total receipt value
        //     receipt.TotalValue = receipt.Products.Sum(p => p.TotalValue);

        //     _context.BenzeneBuyReceipts.Update(receipt);
        //     await _context.SaveChangesAsync();

        //     return Ok(new { message = "Products updated successfully", receipt });
        // }



        [HttpPut("edit-products")]
        public async Task<IActionResult> EditProductsInReceipt([FromBody] EditProductsRequest request)
        {
            if (request == null || request.Products == null || request.Products.Count == 0)
                return BadRequest(new { message = "Invalid data." });

            // Convert the date to UTC
            request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);

            // Find the receipt based on Date and IncrementalId
            var receipt = await _context.BenzeneBuyReceipts
                .Include(r => r.Products)
                .FirstOrDefaultAsync(r => r.Date.Year == request.Date.Year &&
                                        r.Date.Month == request.Date.Month &&
                                        r.IncrementalId == request.IncrementalId);

            if (receipt == null)
                return NotFound(new { message = "Receipt not found." });



            //---------------------------->>>>>>>>>>>>>
            // Find the last balance created **before** the receipt was created (just before the edit)
            var previousBalance = await _context.Balances
                .Where(b => b.DateTime < receipt.MobilReceiptDate) // Ensure we find balances before the receipt date
                .OrderByDescending(b => b.DateTime) // Order by DateTime descending
                .ThenByDescending(b => b.Id) // In case there are multiple balances with the same DateTime, get the one with the largest Id
                .FirstOrDefaultAsync(); // Get the first (most recent) balance


            // If no previous balance is found, set the previous balance to 0
            decimal adjustedBalance = previousBalance != null ? previousBalance.BalanceAmount : 0;




            // Update products in the receipt
            foreach (var productRequest in request.Products)
            {
                if (productRequest.Amount <= 0)
                    return BadRequest(new { message = "Product amount must be greater than zero." });

                var product = receipt.Products.FirstOrDefault(p => p.Id == productRequest.Id);
                if (product != null)
                {
                    var benzene = await _context.Benzenes.FirstOrDefaultAsync(b => b.Name == productRequest.ProductName);
                    if (benzene == null)
                        return NotFound(new { message = $"Benzene product '{productRequest.ProductName}' not found." });
                    
                    // Use values from the Benzene table
                    float pricePerLiter = benzene.PriceOfLitre;
                    float evaporationPercentage = benzene.RateOfEvaporation;
                    float taxes = benzene.RateOfTaxes;
                    float vats = benzene.RateOfVats;

                    // Update product fields
                    product.ProductName = productRequest.ProductName;
                    product.Amount = productRequest.Amount;
                    product.PricePerLiter = pricePerLiter;
                    product.EvaporationPercentage = evaporationPercentage;
                    product.Taxes = taxes;

                    // Recalculate values
                    float valueBeforeEvaporation = product.Amount * product.PricePerLiter;
                    product.ValueOfEvaporation = evaporationPercentage * product.Amount * pricePerLiter;
                    product.ValueOfTaxes = taxes * product.Amount * vats;
                    product.TotalValue = valueBeforeEvaporation - product.ValueOfEvaporation + product.ValueOfTaxes;
                }
            }

            // Recalculate total receipt value
            receipt.TotalValue = receipt.Products.Sum(p => p.TotalValue);

            _context.BenzeneBuyReceipts.Update(receipt);
            await _context.SaveChangesAsync();




            //changing the balanceee     ------->>>>>>>>>>>>>>

            // Now, subtract the current receipt total value from the balance
                adjustedBalance -= (decimal)receipt.TotalValue;

                // Create a new Balance entry with the updated balance
                var newBalance = new Balance
                {
                    BalanceAmount = adjustedBalance,
                    DateTime = DateTime.SpecifyKind(receipt.MobilReceiptDate, DateTimeKind.Utc) // Set the time of the receipt
                };

                // Save the new balance
                _context.Balances.Add(newBalance);
                await _context.SaveChangesAsync();



            return Ok(new { message = "Products updated successfully", receipt });
        }

    }







public class AddProductsRequest
{
    public DateTime Date { get; set; } // Year and Month
    public int IncrementalId { get; set; } // Receipt ID within that month
    public List<ProductRequest> Products { get; set; }
}

public class ProductRequest
{
    public string ProductName { get; set; }  // Only Product Name
    public float Amount { get; set; } // Only Amount
}


// public class ProductRequest
// {
//     public string ProductName { get; set; }  // ✅ Added Product Name
//     public float Amount { get; set; }
//     public float PricePerLiter { get; set; }
//     public float EvaporationPercentage { get; set; }
//     public float Taxes { get; set; }
// }

public class BenzeneBuyReceiptRequest
{
    public int? MobilId { get; set; }  // Nullable to handle missing or invalid inputs
    public DateTime? MobilReceiptDate { get; set; } // Nullable to allow validation
    public DateTime? Date { get; set; } // Nullable to allow validation
}




public class EditProductsRequest
{
    public DateTime Date { get; set; } // Year and Month
    public int IncrementalId { get; set; } // Receipt ID within that month
    public List<ProductEditRequest> Products { get; set; }
}


// public class EditProductDto
// {
//     public int Id { get; set; }  // ID of the product being edited
//     public string ProductName { get; set; }  
//     public float Amount { get; set; }
//     public float PricePerLiter { get; set; }
//     public float EvaporationPercentage { get; set; }
//     public float Taxes { get; set; }
// }

public class ProductEditRequest
{
    public int Id { get; set; } // Product ID in receipt
    public string ProductName { get; set; }
    public float Amount { get; set; }
}



}
