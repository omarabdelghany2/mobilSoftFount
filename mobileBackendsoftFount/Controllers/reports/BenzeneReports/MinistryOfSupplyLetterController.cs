using Microsoft.AspNetCore.Mvc;
using mobileBackendsoftFount.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MinistryOfSupplyLetterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MinistryOfSupplyLetterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("generate-ministry-of-supply-letter")]
        public async Task<ActionResult<MinistryOfSupplyLetter>> GenerateLetter([FromBody] MinistryOfSupplyLetterInputModel input)
        {
            try
            {
                input.Date = DateTime.SpecifyKind(input.Date, DateTimeKind.Utc);

                if (!DateTime.TryParseExact(input.MonthlyDate, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedMonthlyDate))
                {
                    return BadRequest("MonthlyDate must be in format YYYY-MM.");
                }

                parsedMonthlyDate = DateTime.SpecifyKind(new DateTime(parsedMonthlyDate.Year, parsedMonthlyDate.Month, 1), DateTimeKind.Utc);

                var letter = new MinistryOfSupplyLetter
                {
                    Introduction = input.Introduction,
                    Date = input.Date,
                    MonthlyDate = parsedMonthlyDate
                };

                var monthStart = parsedMonthlyDate;
                var monthEnd = monthStart.AddMonths(1);

                var lastLetter = await _context.MinistryOfSupplyLetters
                    .Include(l => l.Members)
                    .Where(l => l.MonthlyDate < parsedMonthlyDate)
                    .OrderByDescending(l => l.MonthlyDate)
                    .FirstOrDefaultAsync();

                bool exists = await _context.MinistryOfSupplyLetters
                    .AnyAsync(l => l.MonthlyDate.Year == parsedMonthlyDate.Year && l.MonthlyDate.Month == parsedMonthlyDate.Month);

                if (exists)
                {
                    return BadRequest("A letter already exists for this month.");
                }

                var previousStartBalances = lastLetter?.Members
                    .ToDictionary(m => m.Type.ToLower(), m => m.CurrentBalance)
                    ?? new Dictionary<string, decimal> {
                        { "92", 0m }, { "95", 0m }, { "oils", 0m }
                    };

                var benzeneReceipts = await _context.BenzeneBuyReceipts
                    .Include(r => r.Products)
                    .Where(r => r.Date >= monthStart && r.Date < monthEnd)
                    .ToListAsync();

                decimal income92 = benzeneReceipts
                    .SelectMany(r => r.Products)
                    .Where(p => p.ProductName.Contains("92"))
                    .Sum(p => (decimal)p.Amount);

                decimal income95 = benzeneReceipts
                    .SelectMany(r => r.Products)
                    .Where(p => p.ProductName.Contains("95"))
                    .Sum(p => (decimal)p.Amount);

                var oilReceipts = await _context.OilBuyReceipts
                    .Include(r => r.OilBuyProducts)
                    .Where(r => r.Date >= monthStart && r.Date < monthEnd)
                    .ToListAsync();

                decimal oilIncome = oilReceipts
                    .SelectMany(r => r.OilBuyProducts)
                    .Sum(p => p.Amount * p.Weight);

                var member92 = new MinistryOfSupplyLetterMember
                {
                    Type = "92",
                    StartBalance = previousStartBalances.GetValueOrDefault("92", 0),
                    CurrentBalance = input.CurrentBalance92,
                    IncomeAmount = income92,
                    SoldAmount = previousStartBalances.GetValueOrDefault("92", 0) + income92 - input.CurrentBalance92,
                    Total = previousStartBalances.GetValueOrDefault("92", 0) + income92
                };

                var member95 = new MinistryOfSupplyLetterMember
                {
                    Type = "95",
                    StartBalance = previousStartBalances.GetValueOrDefault("95", 0),
                    CurrentBalance = input.CurrentBalance95,
                    IncomeAmount = income95,
                    SoldAmount = previousStartBalances.GetValueOrDefault("95", 0) + income95 - input.CurrentBalance95,
                    Total = previousStartBalances.GetValueOrDefault("95", 0) + income95
                };

                decimal currentOilBalance = await _context.Oils
                    .Where(o => o.Enable)
                    .SumAsync(o => (decimal)(o.Amount * o.Weight));

                var memberOils = new MinistryOfSupplyLetterMember
                {
                    Type = "oils",
                    StartBalance = previousStartBalances.GetValueOrDefault("oils", 0),
                    CurrentBalance = currentOilBalance,
                    IncomeAmount = oilIncome,
                    SoldAmount = previousStartBalances.GetValueOrDefault("oils", 0) + oilIncome - currentOilBalance,
                    Total = previousStartBalances.GetValueOrDefault("oils", 0) + oilIncome
                };

                letter.Members = new List<MinistryOfSupplyLetterMember> { member92, member95, memberOils };

                _context.MinistryOfSupplyLetters.Add(letter);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetLetter), new { id = letter.Id }, letter);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (FormatException fmtEx)
            {
                return BadRequest(new { message = $"Date format error: {fmtEx.Message}" });
            }
            catch (InvalidOperationException invEx)
            {
                return BadRequest(new { message = $"Invalid operation: {invEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MinistryOfSupplyLetter>> GetLetter(int id)
        {
            var letter = await _context.MinistryOfSupplyLetters
                .Include(l => l.Members)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (letter == null)
            {
                return NotFound();
            }

            return letter;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLetter(int id)
        {
            var letter = await _context.MinistryOfSupplyLetters
                .Include(l => l.Members) // Include related members to delete them as well
                .FirstOrDefaultAsync(l => l.Id == id);

            if (letter == null)
            {
                return NotFound();
            }

            // Remove related members first if cascade delete is not configured
            _context.MinistryOfSupplyLetterMembers.RemoveRange(letter.Members);
            _context.MinistryOfSupplyLetters.Remove(letter);

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("dates")]
        public async Task<ActionResult<IEnumerable<LetterSummaryDto>>> GetLetterDates()
        {
            var letters = await _context.MinistryOfSupplyLetters
                .OrderByDescending(l => l.MonthlyDate)
                .Select(l => new LetterSummaryDto
                {
                    Id = l.Id,
                    MonthlyDate = l.MonthlyDate
                })
                .ToListAsync();

            return Ok(letters);
        }
    }

    public class MinistryOfSupplyLetterInputModel
    {
        public string Introduction { get; set; }
        public DateTime Date { get; set; }
        public string MonthlyDate { get; set; } // "yyyy-MM" from client
        public decimal CurrentBalance92 { get; set; }
        public decimal CurrentBalance95 { get; set; }
        // Remove CurrentBalanceOils
    }


    public class LetterSummaryDto
    {
        public int Id { get; set; }
        public DateTime MonthlyDate { get; set; }
    }
}
