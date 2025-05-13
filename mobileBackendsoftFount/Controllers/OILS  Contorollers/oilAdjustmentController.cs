using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OilAdjustmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilAdjustmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdjustment([FromBody] oilAdjustmentRequest request)
        {
            if (request == null || request.amount == null || request.increase == null || request.date == null)
                return BadRequest(new { message = "Amount, Increase, and Date are required." });

            var date = request.date.Value.ToUniversalTime();

            // ❗ Check if adjustment already exists for this exact date
            bool exists = await _context.OilAdjustments
                .AnyAsync(a => a.date == date);

            if (exists)
                return BadRequest(new { message = "An adjustment already exists for this date." });

            // Count adjustments for the month to assign monthlyId
            var countForMonth = await _context.OilAdjustments
                .CountAsync(a => a.date.Month == date.Month && a.date.Year == date.Year);

            var adjustment = new oilAdjustment
            {
                amount = request.amount.Value,
                comment = request.comment ?? "",
                date = date,
                increase = request.increase.Value,
                monthlyId = countForMonth + 1
            };

            _context.OilAdjustments.Add(adjustment);
            await _context.SaveChangesAsync();

            // Create new balance entry
            var latestBalance = await _context.oilAccountBalances
                .Where(b => b.DateTime <= adjustment.date)
                .OrderByDescending(b => b.DateTime)
                .ThenByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            decimal baseAmount = latestBalance?.BalanceAmount ?? 0;
            decimal adjustmentAmount = (decimal)adjustment.amount;

            decimal newBalanceAmount = adjustment.increase
                ? baseAmount + adjustmentAmount
                : baseAmount - adjustmentAmount;

            var newBalance = new oilAccountBalance
            {
                BalanceAmount = newBalanceAmount,
                DateTime = adjustment.date
            };

            _context.oilAccountBalances.Add(newBalance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = adjustment.Id }, new
            {
                message = "Adjustment created.",
                adjustment
            });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var adjustment = await _context.OilAdjustments.FindAsync(id);
            if (adjustment == null)
                return NotFound(new { message = "Adjustment not found." });

            return Ok(adjustment);
        }


        [HttpGet("by-date")]
        public async Task<ActionResult<IEnumerable<oilAdjustment>>> GetAdjustmentsByMonthAndYear([FromQuery] DateTime date)
        {
            var results = await _context.OilAdjustments
                .Where(a => a.date.Month == date.Month && a.date.Year == date.Year)
                .ToListAsync();

            return Ok(results);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdjustment(int id, [FromBody] oilAdjustment request)
        {
            var adjustment = await _context.OilAdjustments.FindAsync(id);
            if (adjustment == null)
                return NotFound(new { message = "Adjustment not found." });

            // Save old values
            var oldAmount = adjustment.amount;
            var oldIncrease = adjustment.increase;
            var oldDate = adjustment.date;

            // Update fields
            adjustment.amount = request.amount;
            adjustment.comment = request.comment ?? adjustment.comment;
            adjustment.increase = request.increase;
            adjustment.date = request.date.ToUniversalTime();

            _context.OilAdjustments.Update(adjustment);
            await _context.SaveChangesAsync();

            // 🔥 Now update balance similarly to how deposits update

            // Find the balance created closest to this adjustment
            var balance = await _context.oilAccountBalances
                .Where(b => b.DateTime == oldDate)
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            if (balance != null)
            {
                // First, remove the old adjustment effect
                decimal oldEffect = oldIncrease ? (decimal)oldAmount : -(decimal)oldAmount;
                balance.BalanceAmount -= oldEffect;

                // Then, apply the new adjustment effect
                decimal newEffect = adjustment.increase ? (decimal)adjustment.amount : -(decimal)adjustment.amount;
                balance.BalanceAmount += newEffect;

                // If the adjustment date has changed, update balance date
                balance.DateTime = adjustment.date;

                _context.oilAccountBalances.Update(balance);
                await _context.SaveChangesAsync();
            }
            else
            {
                // If no balance found, create a new balance
                var latestBalance = await _context.oilAccountBalances
                    .Where(b => b.DateTime <= adjustment.date)
                    .OrderByDescending(b => b.DateTime)
                    .ThenByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                decimal baseAmount = latestBalance?.BalanceAmount ?? 0;
                decimal newBalanceAmount = adjustment.increase
                    ? baseAmount + (decimal)adjustment.amount
                    : baseAmount - (decimal)adjustment.amount;

                var newBalance = new oilAccountBalance
                {
                    BalanceAmount = newBalanceAmount,
                    DateTime = adjustment.date
                };

                _context.oilAccountBalances.Add(newBalance);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Adjustment updated.", adjustment });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdjustment(int id)
        {
            var adjustment = await _context.OilAdjustments.FindAsync(id);
            if (adjustment == null)
                return NotFound(new { message = "Adjustment not found." });

            // Optionally delete the related balance
            var balance = await _context.oilAccountBalances
                .Where(b => b.DateTime == adjustment.date)
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            if (balance != null)
                _context.oilAccountBalances.Remove(balance);

            _context.OilAdjustments.Remove(adjustment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Adjustment deleted." });
        }





    }


        public class oilAdjustmentRequest
    {
        public float? amount { get; set; }
        public bool? increase { get; set; }
        public string comment { get; set; }
        public DateTime? date { get; set; }
    }
}
