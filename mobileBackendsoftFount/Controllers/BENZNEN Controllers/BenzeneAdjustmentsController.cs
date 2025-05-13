using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/benzene-adjustments")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class BenzeneAdjustmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BenzeneAdjustmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<benzeneAdjustment>>> GetAll()
            {
                return await _context.BenzeneAdjustments.ToListAsync();
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<benzeneAdjustment>> GetById(int id)
            {
                var adjustment = await _context.BenzeneAdjustments.FindAsync(id);
                if (adjustment == null) return NotFound();

                return adjustment;
            }

            [HttpGet("by-date")]
            public async Task<ActionResult<IEnumerable<benzeneAdjustment>>> GetByMonthAndYear([FromQuery] DateTime date)
            {
                var results = await _context.BenzeneAdjustments
                    .Where(a => a.date.Month == date.Month && a.date.Year == date.Year)
                    .ToListAsync();

                return Ok(results);
            }

            // [HttpPost]
            // public async Task<IActionResult> CreateAdjustment([FromBody] BenzeneAdjustmentRequest request)
            // {
            //         if (request == null)
            //             return BadRequest(new { message = "Invalid request body." });

            //         var date = request.date ?? DateTime.UtcNow;

            //         // Count adjustments for same month & year
            //         var countForMonth = await _context.BenzeneAdjustments
            //             .CountAsync(a => a.date.Month == date.Month && a.date.Year == date.Year);

            //         var adjustment = new benzeneAdjustment
            //         {
            //             amount = request.amount ?? 0.0f,
            //             increase = request.increase ?? true,
            //             comment = request.comment ?? string.Empty,
            //             date = date,
            //             monthlyId = countForMonth + 1
            //         };

            //         _context.BenzeneAdjustments.Add(adjustment);
            //         await _context.SaveChangesAsync();

            //     return CreatedAtAction(nameof(GetById), new { id = adjustment.Id }, new
            //     {
            //         message = "Adjustment created.",
            //         adjustment
            //     });

            // }



            [HttpPost]
            public async Task<IActionResult> CreateAdjustment([FromBody] BenzeneAdjustmentRequest request)
            {
                if (request == null)
                    return BadRequest(new { message = "Invalid request body." });

                var date = request.date?.Date.ToUniversalTime() ?? DateTime.UtcNow;

                var countForMonth = await _context.BenzeneAdjustments
                    .CountAsync(a => a.date.Month == date.Month && a.date.Year == date.Year);

                var adjustment = new benzeneAdjustment
                {
                    amount = request.amount ?? 0.0f,
                    increase = request.increase ?? true,
                    comment = request.comment ?? string.Empty,
                    date = date,
                    monthlyId = countForMonth + 1
                };

                _context.BenzeneAdjustments.Add(adjustment);







                 // --- Create Balance Entry ---
                var latestBalance = await _context.Balances
                    .OrderByDescending(b => b.DateTime)
                    .ThenByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                var adjustedBalanceAmount = latestBalance?.BalanceAmount ?? 0;
                adjustedBalanceAmount += (adjustment.increase ? (decimal)adjustment.amount : -(decimal)adjustment.amount);


                var newBalance = new Balance
                {
                    BalanceAmount = adjustedBalanceAmount,
                    DateTime = DateTime.UtcNow
                };

                _context.Balances.Add(newBalance);
                ////////////////

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = adjustment.Id }, new
                {
                    message = "Adjustment created.",
                    adjustment
                });
            }


            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateAdjustment(int id, [FromBody] BenzeneAdjustmentRequest request)
            {
                var adjustment = await _context.BenzeneAdjustments.FindAsync(id);
                if (adjustment == null)
                    return NotFound(new { message = "Adjustment not found." });

                adjustment.amount = request.amount ?? adjustment.amount;
                adjustment.increase = request.increase ?? adjustment.increase;
                adjustment.comment = request.comment ?? adjustment.comment;
                adjustment.date = request.date?.ToUniversalTime() ?? adjustment.date;

                // Find the balance created at the same time as this adjustment
                var createdBalance = await _context.Balances
                    .Where(b => b.DateTime <= adjustment.date)
                    .OrderByDescending(b => b.DateTime)
                    .ThenByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                // Get previous balance before it (same date, smaller id)
                int createdBalanceId = createdBalance?.Id ?? int.MaxValue;

                var previousBalance = await _context.Balances
                    .Where(b =>
                        b.DateTime < adjustment.date || 
                        (b.DateTime == adjustment.date && b.Id < createdBalanceId))
                    .OrderByDescending(b => b.DateTime)
                    .ThenByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                var baseAmount = previousBalance?.BalanceAmount ?? 0;
                var adjustmentValue = adjustment.increase ? (decimal)adjustment.amount : -(decimal)adjustment.amount;
                var adjustedBalanceAmount = baseAmount + adjustmentValue;

                var newBalance = new Balance
                {
                    BalanceAmount = adjustedBalanceAmount,
                    DateTime = adjustment.date
                };

                _context.Balances.Add(newBalance);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Adjustment updated and new balance created.", adjustment });
            }

            // [HttpPut("{id}")]
            // public async Task<IActionResult> UpdateAdjustment(int id, [FromBody] BenzeneAdjustmentRequest request)
            // {
            //     var adjustment = await _context.BenzeneAdjustments.FindAsync(id);
            //     if (adjustment == null) return NotFound(new { message = "Adjustment not found." });

            //     adjustment.amount = request.amount ?? adjustment.amount;
            //     adjustment.increase = request.increase ?? adjustment.increase;
            //     adjustment.comment = request.comment ?? adjustment.comment;

            //      ////////////update the balance here
            //      // Find the balance created at the same time as this adjustment
            //     var createdBalance = await _context.Balances
            //         .Where(b => b.DateTime <= adjustment.date) // Find balances on or before the given date
            //         .OrderByDescending(b => b.DateTime)
            //         .ThenByDescending(b => b.Id)
            //         .FirstOrDefaultAsync();



            //     if (createdBalance != null)
            //     {
            //         // Get previous balance before it (same date, smaller id)
            //         var previousBalance = await _context.Balances
            //             .Where(b =>
            //                 b.DateTime < createdBalance.DateTime ||
            //                 (b.DateTime == createdBalance.DateTime && b.Id < createdBalance.Id))
            //             .OrderByDescending(b => b.DateTime)
            //             .ThenByDescending(b => b.Id)
            //             .FirstOrDefaultAsync();

            //         var baseAmount = previousBalance?.BalanceAmount ?? 0;

            //         var adjustmentValue = adjustment.increase ? (decimal)adjustment.amount : -(decimal)adjustment.amount;

            //         createdBalance.BalanceAmount = baseAmount + adjustmentValue;
            //             var newBalance = new Balance
            //         {
            //             BalanceAmount = createdBalance.BalanceAmount,
            //             DateTime = adjustment.date
            //         };
            //     }
            //     await _context.SaveChangesAsync();
            //     return Ok(new { message = "Adjustment updated.", adjustment });
            // }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var adjustment = await _context.BenzeneAdjustments.FindAsync(id);
            if (adjustment == null) return NotFound();

            _context.BenzeneAdjustments.Remove(adjustment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }


    public class BenzeneAdjustmentRequest
    {
        public float? amount { get; set; }
        public bool? increase { get; set; }
        public string comment { get; set; }
        public DateTime? date { get; set; }
    }
}
