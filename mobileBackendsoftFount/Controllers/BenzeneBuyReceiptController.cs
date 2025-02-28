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
            if (request == null) return BadRequest("Invalid data");

            // Convert DateTime fields to UTC
            request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
            request.MobilReceiptDate = DateTime.SpecifyKind(request.MobilReceiptDate, DateTimeKind.Utc);

            int year = request.Date.Year;
            int month = request.Date.Month;

            // Find the last receipt created in the same year and month
            var lastReceipt = await _context.BenzeneBuyReceipts
                .Where(r => r.Date.Year == year && r.Date.Month == month)
                .OrderByDescending(r => r.IncrementalId)
                .FirstOrDefaultAsync();

            // Incremental ID logic
            int newIncrementalId = (lastReceipt != null) ? lastReceipt.IncrementalId + 1 : 1;

            var newReceipt = new BenzeneBuyReceipt
            {
                MobilId = request.MobilId,
                MobilReceiptDate = request.MobilReceiptDate,
                Date = request.Date,
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



    [HttpPost("add-products")]
    public async Task<IActionResult> AddProductsToReceipt([FromBody] AddProductsRequest request)
    {
        if (request == null || request.Products == null || request.Products.Count == 0)
            return BadRequest("Invalid data. Products list cannot be empty.");

        // Ensure the date is UTC
        request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);

        // Find the receipt based on Date and IncrementalId
        var receipt = await _context.BenzeneBuyReceipts
            .Include(r => r.Products) // Ensure products are loaded
            .FirstOrDefaultAsync(r => r.Date.Year == request.Date.Year &&
                                    r.Date.Month == request.Date.Month &&
                                    r.IncrementalId == request.IncrementalId);

        if (receipt == null)
            return NotFound("Receipt not found.");

        // Add new products to the receipt and calculate total value
        var products = request.Products.Select(p => 
        {
            // Calculate product total value
            float valueBeforeEvaporation = p.Amount * p.PricePerLiter;
            float valueOfEvaporation = valueBeforeEvaporation * (p.EvaporationPercentage / 100);
            float totalProductValue = valueBeforeEvaporation - valueOfEvaporation + p.Taxes;

            return new BenzeneRecipeProduct
            {
                ProductName = p.ProductName,  // ✅ Added product name
                Amount = p.Amount,
                PricePerLiter = p.PricePerLiter,
                EvaporationPercentage = p.EvaporationPercentage,
                Taxes = p.Taxes,
                ValueOfEvaporation = valueOfEvaporation,  // Save calculated evaporation
                ValueOfTaxes = p.Taxes,  // Assuming taxes are pre-determined
                TotalValue = totalProductValue,  // ✅ Save calculated total value
                BenzeneBuyReceiptId = receipt.Id
            };
        }).ToList();

        _context.BenzeneRecipeProducts.AddRange(products);
        await _context.SaveChangesAsync();

        // ✅ **Recalculate the total value of the receipt**
        receipt.TotalValue = products.Sum(p => p.TotalValue);

        _context.BenzeneBuyReceipts.Update(receipt);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Products added successfully", receipt });
    }


    [HttpPut("edit-products")]
    public async Task<IActionResult> EditProductsInReceipt([FromBody] EditProductsRequest request)
    {
        if (request == null || request.Products == null || request.Products.Count == 0)
            return BadRequest("Invalid data.");

        // Convert the date to UTC
        request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);

        // Find the receipt based on Date and IncrementalId
        var receipt = await _context.BenzeneBuyReceipts
            .Include(r => r.Products)
            .FirstOrDefaultAsync(r => r.Date.Year == request.Date.Year &&
                                    r.Date.Month == request.Date.Month &&
                                    r.IncrementalId == request.IncrementalId);

        if (receipt == null)
            return NotFound("Receipt not found.");

        // Update products in the receipt
        foreach (var productRequest in request.Products)
        {
            var product = receipt.Products.FirstOrDefault(p => p.Id == productRequest.Id);
            if (product != null)
            {
                product.ProductName = productRequest.ProductName;
                product.Amount = productRequest.Amount;
                product.PricePerLiter = productRequest.PricePerLiter;
                product.EvaporationPercentage = productRequest.EvaporationPercentage;
                product.Taxes = productRequest.Taxes;

                // Recalculate values
                float valueBeforeEvaporation = product.Amount * product.PricePerLiter;
                product.ValueOfEvaporation = valueBeforeEvaporation * (product.EvaporationPercentage / 100);
                product.TotalValue = valueBeforeEvaporation - product.ValueOfEvaporation + product.Taxes;
            }
        }

        // Recalculate total receipt value
        receipt.TotalValue = receipt.Products.Sum(p => p.TotalValue);

        _context.BenzeneBuyReceipts.Update(receipt);
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
    public string ProductName { get; set; }  // ✅ Added Product Name
    public float Amount { get; set; }
    public float PricePerLiter { get; set; }
    public float EvaporationPercentage { get; set; }
    public float Taxes { get; set; }
}


    // public class EditProductRequest
    // {
    //     public float Amount { get; set; }
    //     public float PricePerLiter { get; set; }
    //     public float EvaporationPercentage { get; set; }
    //     public float Taxes { get; set; }
    // }


    public class BenzeneBuyReceiptRequest
    {
        public int MobilId { get; set; }
        public DateTime MobilReceiptDate { get; set; }
        public DateTime Date { get; set; } // Should be in yy/MM format
    }



public class EditProductsRequest
{
    public DateTime Date { get; set; } // Year and Month
    public int IncrementalId { get; set; } // Receipt ID within that month
    public List<EditProductDto> Products { get; set; }
}


public class EditProductDto
{
    public int Id { get; set; }  // ID of the product being edited
    public string ProductName { get; set; }  
    public float Amount { get; set; }
    public float PricePerLiter { get; set; }
    public float EvaporationPercentage { get; set; }
    public float Taxes { get; set; }
}



}
