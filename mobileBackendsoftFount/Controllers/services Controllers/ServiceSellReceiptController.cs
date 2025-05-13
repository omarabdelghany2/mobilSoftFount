using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/servicesellreceipt")]


    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class ServiceSellReceiptController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceSellReceiptController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// 🔹 1. CREATE A NEW RECEIPT WITH UNIQUE DATE (WITHOUT PRODUCTS)
        [HttpPost("create/{date}")]
        public async Task<IActionResult> CreateReceipt(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");
            }

            // ✅ Convert date to UTC to match PostgreSQL requirements
            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

            var existingReceipt = await _context.ServiceSellReceipts.FirstOrDefaultAsync(r => r.Date == parsedDate);
            if (existingReceipt != null)
            {
                return BadRequest("A receipt for this date already exists.");
            }

            var newReceipt = new ServiceSellReceipt
            {
                Date = parsedDate, // ✅ Make sure this is in UTC
                ServiceSellProducts = new List<ServiceSellProduct>(),
                TotalPrice = 0
            };

            _context.ServiceSellReceipts.Add(newReceipt);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReceipt), new { date = newReceipt.Date.ToString("yyyy-MM-dd") }, newReceipt);
        }


        // [HttpDelete("delete/{date}")]
        // public async Task<IActionResult> DeleteReceipt(string date)
        // {
        //     if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
        //     {
        //         return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        //     }

        //     // ✅ Convert to UTC before searching in DB
        //     parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

        //     var receipt = await _context.ServiceSellReceipts.FirstOrDefaultAsync(r => r.Date == parsedDate);
        //     if (receipt == null)
        //     {
        //         return NotFound("Receipt not found for the given date.");
        //     }

        //     _context.ServiceSellReceipts.Remove(receipt);
        //     await _context.SaveChangesAsync();

        //     return Ok("Receipt deleted successfully.");
        // }

        [HttpDelete("delete/{date}")]
        public async Task<IActionResult> DeleteReceipt(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD." });
            }

            // ✅ Convert to UTC before searching in DB
            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

            // Find the receipt and include related products and their client services
            var receipt = await _context.ServiceSellReceipts
                .Include(r => r.ServiceSellProducts)
                    .ThenInclude(p => p.ClientServices)
                .FirstOrDefaultAsync(r => r.Date == parsedDate);

            if (receipt == null)
            {
                return BadRequest(new { message = "Receipt not found. It was not created to be deleted." });
            }

            // Delete all related ClientServices
            foreach (var product in receipt.ServiceSellProducts)
            {
                _context.ClientServices.RemoveRange(product.ClientServices);
            }

            // Delete all related ServiceSellProducts
            _context.ServiceSellProducts.RemoveRange(receipt.ServiceSellProducts);

            // Delete the receipt
            _context.ServiceSellReceipts.Remove(receipt);

            // Save changes
            await _context.SaveChangesAsync();

            return NoContent(); // ✅ Return NoContent with a success message
        }


        /// 🔹 3. GET A RECEIPT BY DATE
        [HttpGet("get/{date}")]
        public async Task<IActionResult> GetReceipt(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");
            }

            // ✅ Convert to UTC before searching in DB
            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

            var receipt = await _context.ServiceSellReceipts
                .Include(r => r.ServiceSellProducts)
                .ThenInclude(p => p.ClientServices)
                .FirstOrDefaultAsync(r => r.Date == parsedDate);

            if (receipt == null)
            {
                return NotFound("Receipt not found for the given date.");
            }

            return Ok(receipt);
        }


        // /// 🔹 4. ADD SERVICE SELL PRODUCTS TO AN EXISTING RECEIPT BY DATE
        // [HttpPost("add-products/{date}")]
        // public async Task<IActionResult> AddSellProducts(string date, [FromBody] List<ServiceSellProductRequest> products)
        // {
        //     // ✅ Parse Date & Ensure it's in UTC
        //     if (!DateTime.TryParse(date, out DateTime parsedDate))
        //     {
        //         return BadRequest("Invalid date format.");
        //     }
        //     parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

        //     // ✅ Find the existing receipt by date
        //     var receipt = await _context.ServiceSellReceipts
        //         .Include(r => r.ServiceSellProducts)
        //         .ThenInclude(p => p.ClientServices)
        //         .FirstOrDefaultAsync(r => r.Date.Date == parsedDate.Date);

        //     if (receipt == null)
        //         return NotFound("ServiceSellReceipt not found for the given date.");

        //     // ✅ Remove all existing products and their services
        //     _context.ServiceSellProducts.RemoveRange(receipt.ServiceSellProducts);

        //     // ✅ Prepare new products list
        //     List<ServiceSellProduct> newProducts = new();

        //     foreach (var product in products)
        //     {
        //         decimal totalValue = 0; // To calculate total value of this product

        //         // 🔹 Validate Worker Name
        //         var workerExists = await _context.OilWorkers.AnyAsync(w => w.Name == product.Worker);
        //         if (!workerExists)
        //             return BadRequest($"Worker '{product.Worker}' is not valid.");

        //         List<ClientService> clientServices = new();
        //         foreach (var service in product.ClientServices)
        //         {
        //             // 🔹 Validate Category Name
        //             var categoryExists = await _context.Categories.AnyAsync(c => c.Name == service.CategoryName);
        //             if (!categoryExists)
        //                 return BadRequest($"Category '{service.CategoryName}' is not valid.");

        //             // 🔹 Fetch & Validate Subcategory Price
        //             var subCategory = await _context.SubCategories
        //                 .FirstOrDefaultAsync(s => s.Name == service.SubCategoryName);

        //             if (subCategory == null)
        //                 return BadRequest($"Subcategory '{service.SubCategoryName}' is not valid.");

        //             // 🔹 Create ClientService with fetched price
        //             var clientService = new ClientService
        //             {
        //                 CategoryName = service.CategoryName,
        //                 SubCategoryName = service.SubCategoryName,
        //                 SubCategoryPrice = (decimal)subCategory.Price
        //             };

        //             // 🔹 Add to client services list & update total value
        //             clientServices.Add(clientService);
        //             totalValue += clientService.SubCategoryPrice;
        //         }

        //         // 🔹 Create new ServiceSellProduct
        //         var newProduct = new ServiceSellProduct
        //         {
        //             ClientName = product.ClientName,
        //             ClientCarModel = product.ClientCarModel,
        //             ClientCarNumber = product.ClientCarNumber,
        //             Worker = product.Worker,
        //             Value = totalValue, // 🔹 Sum of all subcategory prices
        //             ClientServices = clientServices
        //         };

        //         newProducts.Add(newProduct);
        //     }

        //     // ✅ Attach new products to the receipt
        //     receipt.ServiceSellProducts = newProducts;

        //     // ✅ Recalculate Total Price of the receipt
        //     receipt.TotalPrice = newProducts.Sum(p => p.Value);

        //     _context.Entry(receipt).State = EntityState.Modified;
        //     await _context.SaveChangesAsync();

        //     return Ok(receipt);
        // }


    // [HttpPost("add-products/{date}")]
    // public async Task<IActionResult> AddSellProducts(string date, [FromBody] List<ServiceSellProductRequest> products)
    // {
    //     // ✅ Parse Date & Ensure it's in UTC
    //     if (!DateTime.TryParse(date, out DateTime parsedDate))
    //     {
    //         return BadRequest("Invalid date format.");
    //     }
    //     parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

    //     // ✅ Find the existing receipt by date
    //     var receipt = await _context.ServiceSellReceipts
    //         .Include(r => r.ServiceSellProducts)
    //         .ThenInclude(p => p.ClientServices)
    //         .FirstOrDefaultAsync(r => r.Date.Date == parsedDate.Date);

    //     if (receipt == null)
    //         return NotFound("ServiceSellReceipt not found for the given date.");

    //     // ✅ Remove all existing products and their services
    //     _context.ServiceSellProducts.RemoveRange(receipt.ServiceSellProducts);

    //     // ✅ Prepare new products list
    //     List<ServiceSellProduct> newProducts = new();

    //     foreach (var product in products)
    //     {
    //         decimal totalValue = 0; // To calculate total value of this product

    //         // 🔹 Validate Worker Name
    //         var workerExists = await _context.OilWorkers.AnyAsync(w => w.Name == product.Worker);
    //         if (!workerExists)
    //             return BadRequest($"Worker '{product.Worker}' is not valid.");

    //         List<ClientService> clientServices = new();
    //         foreach (var service in product.ClientServices)
    //         {
    //             // 🔹 Validate Category Name
    //             var categoryExists = await _context.Categories.AnyAsync(c => c.Name == service.CategoryName);
    //             if (!categoryExists)
    //                 return BadRequest($"Category '{service.CategoryName}' is not valid.");

    //             // 🔹 Fetch & Validate Subcategory Price
    //             var subCategory = await _context.SubCategories
    //                 .FirstOrDefaultAsync(s => s.Name == service.SubCategoryName);

    //             if (subCategory == null)
    //                 return BadRequest($"Subcategory '{service.SubCategoryName}' is not valid.");

    //             // 🔹 Create ClientService with fetched price
    //             var clientService = new ClientService
    //             {
    //                 CategoryName = service.CategoryName,
    //                 SubCategoryName = service.SubCategoryName,
    //                 SubCategoryPrice = (decimal)subCategory.Price
    //             };

    //             // 🔹 Add to client services list & update total value
    //             clientServices.Add(clientService);
    //             totalValue += clientService.SubCategoryPrice;
    //         }

    //         // 🔹 Create new ServiceSellProduct
    //         var newProduct = new ServiceSellProduct
    //         {
    //             ClientName = product.ClientName,
    //             ClientCarModel = product.ClientCarModel,
    //             ClientCarNumber = product.ClientCarNumber,
    //             ClientPhone = product.ClientPhone, // Map ClientPhone from the request
    //             Worker = product.Worker,
    //             Value = totalValue, // 🔹 Sum of all subcategory prices
    //             ClientServices = clientServices
    //         };

    //         newProducts.Add(newProduct);
    //     }

    //     // ✅ Attach new products to the receipt
    //     receipt.ServiceSellProducts = newProducts;

    //     // ✅ Recalculate Total Price of the receipt
    //     receipt.TotalPrice = newProducts.Sum(p => p.Value);

    //     _context.Entry(receipt).State = EntityState.Modified;
    //     await _context.SaveChangesAsync();

    //     return Ok(receipt);
    // }


        [HttpPost("add-products/{date}")]
    public async Task<IActionResult> AddSellProducts(string date, [FromBody] List<ServiceSellProductRequest>? products)
    {
        if (products == null || products.Count == 0)
            return BadRequest(new { message = "Product list cannot be empty." });

        // ✅ Parse Date & Ensure it's in UTC
        if (!DateTime.TryParse(date, out DateTime parsedDate))
            return BadRequest(new { message = "Invalid date format." });

        parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

        // ✅ Find the existing receipt by date
        var receipt = await _context.ServiceSellReceipts
            .Include(r => r.ServiceSellProducts)
            .ThenInclude(p => p.ClientServices)
            .FirstOrDefaultAsync(r => r.Date.Date == parsedDate.Date);

        if (receipt == null)
            return NotFound(new { message = "ServiceSellReceipt not found for the given date." });

        // ✅ Remove all existing products and their services
        _context.ServiceSellProducts.RemoveRange(receipt.ServiceSellProducts);

        // ✅ Prepare new products list
        List<ServiceSellProduct> newProducts = new();

        foreach (var product in products)
        {
            // 🔹 Validate Required Fields
            if (string.IsNullOrWhiteSpace(product.ClientName))
                return BadRequest(new { message = "Client name is required." });

            if (string.IsNullOrWhiteSpace(product.ClientCarModel))
                return BadRequest(new { message = "Client car model is required." });

            if (string.IsNullOrWhiteSpace(product.ClientCarNumber))
                return BadRequest(new { message = "Client car number is required." });

            if (string.IsNullOrWhiteSpace(product.Worker))
                return BadRequest(new { message = "Worker name is required." });

            if (product.ClientServices == null || product.ClientServices.Count == 0)
                return BadRequest(new { message = "At least one service is required." });

            decimal totalValue = 0;

            // 🔹 Validate Worker Name
            var workerExists = await _context.OilWorkers.AnyAsync(w => w.Name == product.Worker);
            if (!workerExists)
                return BadRequest(new { message = $"Worker '{product.Worker}' is not valid." });

            List<ClientService> clientServices = new();
            foreach (var service in product.ClientServices)
            {
                if (string.IsNullOrWhiteSpace(service.CategoryName))
                    return BadRequest(new { message = "Category name is required." });

                if (string.IsNullOrWhiteSpace(service.SubCategoryName))
                    return BadRequest(new { message = "Subcategory name is required." });

                // 🔹 Validate Category Name
                var categoryExists = await _context.Categories.AnyAsync(c => c.Name == service.CategoryName);
                if (!categoryExists)
                    return BadRequest(new { message = $"Category '{service.CategoryName}' is not valid." });

                // 🔹 Fetch & Validate Subcategory Price
                var subCategory = await _context.SubCategories
                    .FirstOrDefaultAsync(s => s.Name == service.SubCategoryName);

                if (subCategory == null)
                    return BadRequest(new { message = $"Subcategory '{service.SubCategoryName}' is not valid." });

                // 🔹 Create ClientService with fetched price
                var clientService = new ClientService
                {
                    CategoryName = service.CategoryName,
                    SubCategoryName = service.SubCategoryName,
                    SubCategoryPrice = (decimal)subCategory.Price
                };

                // 🔹 Add to client services list & update total value
                clientServices.Add(clientService);
                totalValue += clientService.SubCategoryPrice;
            }

            // 🔹 Create new ServiceSellProduct
            var newProduct = new ServiceSellProduct
            {
                ClientName = product.ClientName,
                ClientCarModel = product.ClientCarModel,
                ClientCarNumber = product.ClientCarNumber,
                ClientPhone = product.ClientPhone, // Nullable now
                Worker = product.Worker,
                Value = totalValue, // 🔹 Sum of all subcategory prices
                ClientServices = clientServices
            };

            newProducts.Add(newProduct);
        }

        // ✅ Attach new products to the receipt
        receipt.ServiceSellProducts = newProducts;

        // ✅ Recalculate Total Price of the receipt
        receipt.TotalPrice = newProducts.Sum(p => p.Value);

        _context.Entry(receipt).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Products updated successfully", receipt });
    }


    }







public class ServiceSellProductRequest
{
    public string? ClientName { get; set; }
    public string? ClientCarModel { get; set; }
    public string? ClientCarNumber { get; set; }
    public string? ClientPhone { get; set; } // Nullable now
    public string? Worker { get; set; }
    public List<ClientServiceRequest>? ClientServices { get; set; } = new();
}

public class ClientServiceRequest
{
    public string? CategoryName { get; set; }
    public string? SubCategoryName { get; set; }
}



}
