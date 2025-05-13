using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.ComponentModel.DataAnnotations;


namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RevenueCategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RevenueCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RevenueCategory>>> GetRevenueCategories()
        {
            return await _context.RevenueCategories.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RevenueCategory>> GetRevenueCategory(int id)
        {
            var category = await _context.RevenueCategories.FindAsync(id);
            if (category == null) return NotFound();

            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<RevenueCategory>> CreateRevenueCategory([FromBody] RevenueCategoryCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name is required and cannot be empty.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.RevenueCategories
                .FirstOrDefaultAsync(c => c.Name == request.Name);

            if (existing != null)
                return BadRequest("Category name must be unique.");

            var category = new RevenueCategory
            {
                Name = request.Name
            };

            _context.RevenueCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRevenueCategory), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRevenueCategory(int id, [FromBody] RevenueCategoryUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name is required and cannot be empty.");

            var category = await _context.RevenueCategories.FindAsync(id);
            if (category == null) return NotFound();

            var existing = await _context.RevenueCategories
                .FirstOrDefaultAsync(c => c.Name == request.Name && c.Id != id);

            if (existing != null)
                return BadRequest("Category name must be unique.");

            category.Name = request.Name;
            await _context.SaveChangesAsync();

            return Ok(category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRevenueCategory(int id)
        {
            var category = await _context.RevenueCategories.FindAsync(id);
            if (category == null) return NotFound();

            _context.RevenueCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class RevenueCategoryCreateRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class RevenueCategoryUpdateRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
