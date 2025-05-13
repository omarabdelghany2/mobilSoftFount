using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Globalization;
using System.ComponentModel.DataAnnotations;


namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RevenuesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RevenuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Revenue>>> GetRevenues()
        {
            return await _context.Revenues.OrderByDescending(r => r.Date).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Revenue>> GetRevenue(int id)
        {
            var revenue = await _context.Revenues.FindAsync(id);
            if (revenue == null) return NotFound();
            return revenue;
        }

        [HttpPost]
        public async Task<ActionResult<Revenue>> CreateRevenue([FromBody] RevenueCreateRequest request)
        {
            if (request.Date == default(DateTime))
                return BadRequest("Date is required and must be valid.");

            if (string.IsNullOrWhiteSpace(request.BankName))
                return BadRequest("BankName cannot be empty.");

            if (request.BankName.Length > 100)
                return BadRequest("BankName cannot exceed 100 characters.");

            if (request.Value <= 0)
                return BadRequest("Value must be greater than zero.");

            if (request.Comment.Length > 500)
                return BadRequest("Comment cannot exceed 500 characters.");

            if (request.Round <= 0)
                return BadRequest("Round must be a positive integer.");

            var revenueCategory = await _context.RevenueCategories.FindAsync(request.RevenueCategoryId);
            if (revenueCategory == null)
                return BadRequest("Invalid RevenueCategoryId.");

            var revenue = new Revenue
            {
                Date = request.Date.ToUniversalTime(),
                BankName = request.BankName,
                Round = request.Round,
                Comment = request.Comment,
                Value = request.Value,
                RevenueCategoryId = request.RevenueCategoryId,
                RevenueCategory = revenueCategory
            };

            _context.Revenues.Add(revenue);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRevenue), new { id = revenue.Id }, revenue);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRevenue(int id, [FromBody] RevenueUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Revenues.FindAsync(id);
            if (existing == null)
                return NotFound();

            if (request.Date.HasValue)
                existing.Date = request.Date.Value.ToUniversalTime();

            if (!string.IsNullOrWhiteSpace(request.BankName))
                existing.BankName = request.BankName;
            else
                return BadRequest("BankName cannot be empty.");

            if (request.Round.HasValue)
                existing.Round = request.Round.Value;

            if (request.Value.HasValue)
                existing.Value = request.Value.Value;

            if (!string.IsNullOrWhiteSpace(request.Comment))
                existing.Comment = request.Comment;

            if (request.RevenueCategoryId.HasValue)
            {
                var category = await _context.RevenueCategories.FindAsync(request.RevenueCategoryId.Value);
                if (category == null)
                    return BadRequest("Invalid RevenueCategoryId.");

                existing.RevenueCategoryId = request.RevenueCategoryId.Value;
                existing.RevenueCategory = category;
            }

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRevenue(int id)
        {
            var revenue = await _context.Revenues.FindAsync(id);
            if (revenue == null) return NotFound();

            _context.Revenues.Remove(revenue);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("by-month")]
        public async Task<ActionResult<IEnumerable<Revenue>>> GetRevenuesByMonth([FromQuery] string yearMonth)
        {
            if (!DateTime.TryParseExact(yearMonth, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                return BadRequest("Invalid date format. Use 'yyyy-MM'.");

            var startDate = new DateTime(parsedDate.Year, parsedDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var revenues = await _context.Revenues
                .Where(r => r.Date >= startDate && r.Date < endDate)
                .ToListAsync();

            return Ok(revenues);
        }
    }

    public class RevenueCreateRequest
    {
        public DateTime Date { get; set; }
        public string BankName { get; set; } = "";
        public int Round { get; set; } = 0;
        public string Comment { get; set; } = "";
        public decimal Value { get; set; } = 0.0m;

        [Required]
        public int RevenueCategoryId { get; set; }
    }

    public class RevenueUpdateRequest
    {
        public DateTime? Date { get; set; }
        public string BankName { get; set; } = "";
        public int? Round { get; set; }
        public string Comment { get; set; } = "";
        public decimal? Value { get; set; }
        public int? RevenueCategoryId { get; set; }
    }
}
