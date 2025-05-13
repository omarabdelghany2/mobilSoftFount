using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/subcategories")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class SubCategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/subcategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubCategory>>> GetSubCategories()
        {
            return await _context.SubCategories
                .Include(s => s.Categories) // ✅ Fix: Load multiple categories
                .ToListAsync();
        }

        // GET: api/subcategories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SubCategory>> GetSubCategory(int id)
        {
            var subCategory = await _context.SubCategories
                .Include(s => s.Categories) // ✅ Fix: Load multiple categories
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (subCategory == null) return NotFound();
            return subCategory;
        }

        // [HttpPost]
        // public async Task<ActionResult<SubCategory>> CreateSubCategory(SubCategory subCategory)
        // {
        //     var categoryExists = await _context.Categories.AnyAsync(c => c.Id == subCategory.CategoryId);
        //     if (!categoryExists) return BadRequest("Category does not exist.");

        //     _context.SubCategories.Add(subCategory);
        //     await _context.SaveChangesAsync();
        //     return CreatedAtAction(nameof(GetSubCategory), new { id = subCategory.Id }, subCategory);
        // }



        // [HttpPost]
        // public async Task<ActionResult<SubCategory>> CreateSubCategory(SubCategory subCategory)
        // {
        //     // Extract the base category name (remove numbers at the end, e.g., "oil2" → "oil")
        //     string baseName = System.Text.RegularExpressions.Regex.Replace(
        //         _context.Categories.Where(c => c.Id == subCategory.CategoryId).Select(c => c.Name).FirstOrDefault() ?? "",
        //         @"\d+$", "");  

        //     if (string.IsNullOrEmpty(baseName)) 
        //         return BadRequest("Category does not exist.");

        //     // Find all related categories with the same base name (oil1, oil2, oil3)
        //     var relatedCategories = await _context.Categories
        //         .Where(c => c.Name.StartsWith(baseName) && (c.Name == $"{baseName}1" || c.Name == $"{baseName}2" || c.Name == $"{baseName}3"))
        //         .ToListAsync();

        //     if (!relatedCategories.Any()) 
        //         return BadRequest("No valid category group found.");

        //     // Attach subcategory to multiple categories
        //     subCategory.Categories = relatedCategories;

        //     _context.SubCategories.Add(subCategory);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetSubCategory), new { id = subCategory.Id }, subCategory);
        // }


        // [HttpPost]
        // public async Task<ActionResult<SubCategory>> CreateSubCategory([FromBody] dynamic requestData)
        // {
        //     // Extract categoryId
        //     int categoryId = requestData.categoryId;

        //     if (categoryId == 0)
        //         return BadRequest("Category ID must be provided.");

        //     // Find the main category
        //     var category = await _context.Categories.FindAsync(categoryId);
        //     if (category == null)
        //         return BadRequest("Category does not exist.");

        //     // 🔹 Extract base category name (e.g., 'oil' from 'oil1')
        //     string baseName = System.Text.RegularExpressions.Regex.Replace(category.Name, @"\d+$", "");  

        //     // 🔹 Find all categories that match baseName (e.g., oil1, oil2, oil3)
        //     var relatedCategories = await _context.Categories
        //         .Where(c => c.Name.StartsWith(baseName) &&
        //                     (c.Name == $"{baseName}1" || c.Name == $"{baseName}2" || c.Name == $"{baseName}3"))
        //         .ToListAsync();

        //     if (!relatedCategories.Any()) 
        //         return BadRequest("No valid category group found.");

        //     // 🔹 Create the SubCategory and associate it with all found categories
        //     var subCategory = new SubCategory
        //     {
        //         Name = requestData.name,
        //         PriceOfBuy = requestData.priceOfBuy,
        //         Price = requestData.price,
        //         Categories = relatedCategories  // ✅ Assign to all categories (oil1, oil2, oil3)
        //     };

        //     _context.SubCategories.Add(subCategory);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction(nameof(GetSubCategory), new { id = subCategory.Id }, subCategory);
        // }



[HttpPost]
public async Task<ActionResult<SubCategory>> CreateSubCategory([FromBody] SubCategoryRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // ✅ Ensure CategoryId is valid
    if (!request.CategoryId.HasValue || request.CategoryId.Value <= 0)
        return BadRequest(new { message = "Category ID must be provided and greater than zero." });

    var category = await _context.Categories.FindAsync(request.CategoryId.Value);
    if (category == null)
        return BadRequest(new { message = "Category does not exist." });

    // ✅ Validate Name
    if (string.IsNullOrWhiteSpace(request.Name))
        return BadRequest(new { message = "SubCategory name is required." });

    // ✅ Validate PriceOfBuy and Price
    if (!request.PriceOfBuy.HasValue || request.PriceOfBuy.Value <= 0)
        return BadRequest(new { message = "PriceOfBuy must be provided and greater than zero." });

    if (!request.Price.HasValue || request.Price.Value <= 0)
        return BadRequest(new { message = "Price must be provided and greater than zero." });

    // 🔹 Extract base category name (e.g., 'oil' from 'oil1')
    string baseName = System.Text.RegularExpressions.Regex.Replace(category.Name, @"\d+$", "");  

    // 🔹 Find all categories that match baseName (e.g., oil1, oil2, oil3)
    var relatedCategories = await _context.Categories
        .Where(c => c.Name.StartsWith(baseName) &&
                    (c.Name == $"{baseName}1" || c.Name == $"{baseName}2" || c.Name == $"{baseName}3"))
        .ToListAsync();

    if (!relatedCategories.Any()) 
        return BadRequest(new { message = "No valid category group found." });

    // ✅ Create the SubCategory and associate it with all matching categories
    var subCategory = new SubCategory
    {
        Name = request.Name,
        PriceOfBuy = request.PriceOfBuy.Value, // Now safely accessing Value
        Price = request.Price.Value,           // Now safely accessing Value
        Categories = relatedCategories
    };

    _context.SubCategories.Add(subCategory);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetSubCategory), new { id = subCategory.Id }, subCategory);
}





        // DELETE: api/subcategories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null) return NotFound();

            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }


    public class SubCategoryRequest
{

    public int? CategoryId { get; set; } // Nullable to validate properly

    public string? Name { get; set; }



    public double? PriceOfBuy { get; set; }



    public double? Price { get; set; }
}

}
