using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseCategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpenseCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/expensecategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseCategory>>> GetExpenseCategories()
        {
            var categories = await _context.ExpenseCategories.ToListAsync();
            return Ok(categories);
        }

        // GET: api/expensecategories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseCategory>> GetExpenseCategory(int id)
        {
            var category = await _context.ExpenseCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        // POST: api/expensecategories
        [HttpPost]
        public async Task<ActionResult<ExpenseCategory>> CreateExpenseCategory([FromBody] ExpenseCategoryCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name is required and cannot be empty.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Returns detailed validation errors if any.
            }

            var existingCategory = await _context.ExpenseCategories
                .Where(c => c.Name == request.Name)
                .FirstOrDefaultAsync();

            if (existingCategory != null)
            {
                return BadRequest("Category name must be unique.");
            }

            var expenseCategory = new ExpenseCategory
            {
                Name = request.Name,
                DeductionFromProfit = request.DeductionFromProfit
            };

            _context.ExpenseCategories.Add(expenseCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExpenseCategory), new { id = expenseCategory.Id }, expenseCategory);
        }

        // PUT: api/expensecategories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpenseCategory(int id, [FromBody] ExpenseCategoryUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name is required and cannot be empty.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.ExpenseCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var existingCategory = await _context.ExpenseCategories
                .Where(c => c.Name == request.Name && c.Id != id)
                .FirstOrDefaultAsync();

            if (existingCategory != null)
            {
                return BadRequest("Category name must be unique.");
            }

            category.Name = request.Name;
            category.DeductionFromProfit = request.DeductionFromProfit;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // DELETE: api/expensecategories/{id}
        // DELETE: api/expensecategories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpenseCategory(int id)
        {
            var category = await _context.ExpenseCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Cascade delete is automatically handled by EF Core when the category is deleted
            _context.ExpenseCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();  // Return 204 No Content on success
        }

    }








    public class ExpenseCategoryCreateRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        public bool DeductionFromProfit { get; set; }
    }

        public class ExpenseCategoryUpdateRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        public bool DeductionFromProfit { get; set; }
    }


}
