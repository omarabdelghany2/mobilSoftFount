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
    [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
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
            .FirstOrDefaultAsync(r => r.Date.Year == request.Date.Year &&
                                    r.Date.Month == request.Date.Month &&
                                    r.IncrementalId == request.IncrementalId);

        if (receipt == null)
            return NotFound("Receipt not found.");

        // Add new products to the receipt
        var products = request.Products.Select(p => new BenzeneRecipeProduct
        {
            Amount = p.Amount,
            PricePerLiter = p.PricePerLiter,
            EvaporationPercentage = p.EvaporationPercentage,
            Taxes = p.Taxes,
            BenzeneBuyReceiptId = receipt.Id  // Link to the receipt
        }).ToList();

        _context.BenzeneRecipeProducts.AddRange(products);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Products added successfully", products });
    }

    [HttpPut("edit-product/{productId}")]
    public async Task<IActionResult> EditProductInReceipt(int productId, [FromBody] EditProductRequest request)
    {
        if (request == null)
            return BadRequest("Invalid data.");

        var product = await _context.BenzeneRecipeProducts.FindAsync(productId);
        if (product == null)
            return NotFound("Product not found.");

        // Update product details
        product.Amount = request.Amount;
        product.PricePerLiter = request.PricePerLiter;
        product.EvaporationPercentage = request.EvaporationPercentage;
        product.Taxes = request.Taxes;

        _context.BenzeneRecipeProducts.Update(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Product updated successfully", product });
    }


    [HttpDelete("delete-product/{productId}")]
    public async Task<IActionResult> DeleteProductFromReceipt(int productId)
    {
        var product = await _context.BenzeneRecipeProducts.FindAsync(productId);
        if (product == null)
            return NotFound("Product not found.");

        _context.BenzeneRecipeProducts.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Product deleted successfully" });
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
    public float Amount { get; set; }
    public float PricePerLiter { get; set; }
    public float EvaporationPercentage { get; set; }
    public float Taxes { get; set; }
}

public class EditProductRequest
{
    public float Amount { get; set; }
    public float PricePerLiter { get; set; }
    public float EvaporationPercentage { get; set; }
    public float Taxes { get; set; }
}


    public class BenzeneBuyReceiptRequest
    {
        public int MobilId { get; set; }
        public DateTime MobilReceiptDate { get; set; }
        public DateTime Date { get; set; } // Should be in yy/MM format
    }
}
