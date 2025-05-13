using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Globalization;  // ✅ Add this
using System.ComponentModel.DataAnnotations;

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/expenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {
            return await _context.Expenses.OrderByDescending(e => e.Date).ToListAsync();
        }


        // GET: api/expenses/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound(); // If no expense is found with the given ID, return 404.
            
            return expense; // Return the found expense
        }


        // POST: api/expenses
        [HttpPost]
        public async Task<ActionResult<Expense>> CreateExpense([FromBody] ExpenseCreateRequest request)
        {
            // Manual validation
            if (request.Date == default(DateTime))
            {
                return BadRequest("Date is required and must be valid.");
            }

            if (string.IsNullOrWhiteSpace(request.BankName))
            {
                return BadRequest("BankName cannot be empty.");
            }

            if (request.BankName.Length > 100)
            {
                return BadRequest("BankName cannot exceed 100 characters.");
            }

            if (request.Value <=0)
            {
                return BadRequest("Value cannot be zero or less.");
            }


            if (request.Comment.Length > 500)
            {
                return BadRequest("Comment cannot exceed 500 characters.");
            }

            if (request.Round <= 0)
            {
                return BadRequest("Round must be a positive integer.");
            }

                var expenseCategory = await _context.ExpenseCategories.FindAsync(request.ExpenseCategoryId);
                if (expenseCategory == null)
                {
                    return BadRequest("Invalid ExpenseCategoryId.");
                }

            var expense = new Expense
            {
                Date = request.Date.ToUniversalTime(),  // Ensure the date is UTC
                BankName = request.BankName,
                Round = request.Round,
                Comment = request.Comment,
                ExpenseCategoryId = request.ExpenseCategoryId,  // Linking to ExpenseCategory
                ExpenseCategory = expenseCategory,  // Navigation property
                Value=request.Value
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExpense", new { id = expense.Id }, expense);

        }

        // PUT: api/expenses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Returns detailed validation errors if any.
            }

            var existing = await _context.Expenses.FindAsync(id);
            if (existing == null)
            {
                return NotFound();  // Expense not found
            }

            // Apply updates only for the provided values (nullable values)
            if (request.Date.HasValue)
            {
                existing.Date = request.Date.Value.ToUniversalTime();
            }

            // Ensure BankName is not empty before updating
            if (!string.IsNullOrWhiteSpace(request.BankName))
            {
                existing.BankName = request.BankName;
            }
            else
            {
                return BadRequest("BankName cannot be empty.");
            }

            if (request.Round.HasValue)
            {
                existing.Round = request.Round.Value;
            }

            if (request.Value.HasValue)
            {
                existing.Value = request.Value.Value;
            }


            if (!string.IsNullOrWhiteSpace(request.Comment))
            {
                existing.Comment = request.Comment;
            }

                // Update ExpenseCategoryId if provided
                if (request.ExpenseCategoryId.HasValue)
                {
                    var expenseCategory = await _context.ExpenseCategories.FindAsync(request.ExpenseCategoryId.Value);
                    if (expenseCategory == null)
                    {
                        return BadRequest("Invalid ExpenseCategoryId.");
                    }
                    existing.ExpenseCategoryId = request.ExpenseCategoryId.Value;
                    existing.ExpenseCategory = expenseCategory; // Set the navigation property
                }

            await _context.SaveChangesAsync();
            return Ok(existing);  // Return 204 No Content on success
        }


        // DELETE: api/expenses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();  // Return 204 No Content on success
        }

        // GET: api/expenses/by-month?yearMonth=yyyy-MM
        [HttpGet("by-month")]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpensesByMonth([FromQuery] string yearMonth)
        {
            if (!DateTime.TryParseExact(yearMonth, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format. Use 'yyyy-MM'.");
            }

            var startDate = new DateTime(parsedDate.Year, parsedDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var expenses = await _context.Expenses
                .Where(e => e.Date >= startDate && e.Date < endDate)
                .ToListAsync();

            return Ok(expenses);
        }
    }

    public class ExpenseCreateRequest
    {
        public DateTime Date { get; set; }

        public string BankName { get; set; } = "";

        public int Round { get; set; } = 0;

        public string Comment { get; set; } = "";

        public decimal Value { get; set; } = 0.0m;
        

        // New field for ExpenseCategoryId
        [Required(ErrorMessage = "ExpenseCategoryId is required.")]
        public int ExpenseCategoryId { get; set; }
    }

    public class ExpenseUpdateRequest
    {
        public DateTime? Date { get; set; }


        public string BankName { get; set; } = "";

        public int? Round { get; set; }

        public string Comment { get; set; } = "";
        
        public decimal? Value { get; set; } // ✅ Fix: make it nullable

        // Optional field for ExpenseCategoryId
        public int? ExpenseCategoryId { get; set; }
    }
}
