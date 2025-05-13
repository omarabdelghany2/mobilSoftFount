using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/categories")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }
        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Category>>> CreateCategory([FromBody] CategoryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Category name is required." });

            // ✅ Generate potential category names
            List<string> categoryNames = new();
            for (int i = 1; i <= 3; i++)
            {
                categoryNames.Add($"{request.Name}{i}");
            }

            // ✅ Check if any of the generated category names already exist
            bool anyCategoryExists = await _context.Categories.AnyAsync(c => categoryNames.Contains(c.Name));
            if (anyCategoryExists)
                return BadRequest(new { message = $"A category with the name '{request.Name}' already exists." });

            // ✅ Create new categories
            List<Category> newCategories = categoryNames.Select(name => new Category { Name = name }).ToList();

            _context.Categories.AddRange(newCategories);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), newCategories);
        }






        // // DELETE: api/categories/{name} (Deletes name1, name2, name3)
        // [HttpDelete("{name}")]
        // public async Task<IActionResult> DeleteCategoriesByName(string name)
        // {
        //     // Extract the base name (remove trailing numbers)
        //     string baseName = Regex.Replace(name, @"\d+$", ""); // Removes numbers from the end

        //     var categoriesToDelete = await _context.Categories
        //         .Where(c => c.Name == $"{baseName}1" || c.Name == $"{baseName}2" || c.Name == $"{baseName}3")
        //         .ToListAsync();

        //     if (!categoriesToDelete.Any()) return NotFound();

        //     _context.Categories.RemoveRange(categoriesToDelete);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }



// DELETE: api/categories/{name} (Deletes name1, name2, name3 + related subcategories)
[HttpDelete("{name}")]
public async Task<IActionResult> DeleteCategoriesByName(string name)
{
    // Extract the base name (remove trailing numbers)
    string baseName = Regex.Replace(name, @"\d+$", "");

    // Find all matching categories (e.g., oil1, oil2, oil3)
    var categoriesToDelete = await _context.Categories
        .Where(c => c.Name.StartsWith(baseName) && 
                    (c.Name == $"{baseName}1" || c.Name == $"{baseName}2" || c.Name == $"{baseName}3"))
        .Include(c => c.SubCategories)  // 🔥 Include subcategories before deletion
        .ToListAsync();

    if (!categoriesToDelete.Any()) return NotFound();

    // 🔹 Collect all related subcategories
    var subCategoriesToDelete = categoriesToDelete
        .SelectMany(c => c.SubCategories)
        .Distinct()
        .ToList();

    // 🔥 Delete subcategories first, then categories
    _context.SubCategories.RemoveRange(subCategoriesToDelete);
    _context.Categories.RemoveRange(categoriesToDelete);

    await _context.SaveChangesAsync();
    return NoContent();
}




        // DELETE: api/categories/id/{id} (Deletes a single category by ID)
        [HttpDelete("id/{id}")]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }





        // GET: api/categories/id/{id}/subcategories (Gets all subcategories of a category)
        [HttpGet("id/{id}/subcategories")]
        public async Task<ActionResult<IEnumerable<object>>> GetSubCategoriesByCategoryId(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories) // 🔥 Include subcategories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            // 🔹 Return only necessary properties (exclude categories)
            var subCategoriesWithoutCategories = category.SubCategories
                .Select(sc => new 
                {
                    sc.Id,
                    sc.Name,
                    sc.PriceOfBuy,
                    sc.Price
                })
                .ToList();

            return Ok(subCategoriesWithoutCategories);
        }







        // GET: api/categories/with-subcategories (Returns all categories with their subcategories)
        [HttpGet("with-subcategories")]
        public async Task<ActionResult<Dictionary<string, List<object>>>> GetCategoriesWithSubcategories()
        {
            var categories = await _context.Categories
                .Include(c => c.SubCategories) // 🔥 Include subcategories
                .ToListAsync();

            var result = categories.ToDictionary(
                category => category.Name, // Key: Category Name
                category => category.SubCategories
                    .Select(sc => new 
                    {
                        sc.Id,
                        sc.Name,
                    })
                    .ToList<object>() // Convert to List<object> for serialization
            );

            return Ok(result);
        }



    }


    public class CategoryRequest
{
    public string? Name { get; set; }
}






    
}
