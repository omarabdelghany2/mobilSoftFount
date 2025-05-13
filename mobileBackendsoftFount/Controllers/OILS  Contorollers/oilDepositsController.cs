using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/oil-deposits")]
    [ApiController]
    public class OilDepositsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilDepositsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<oilDeposit>>> GetAll()
        {
            return await _context.OilDeposits.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<oilDeposit>> GetById(int id)
        {
            var deposit = await _context.OilDeposits.FindAsync(id);
            if (deposit == null) return NotFound();

            return deposit;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeposit([FromBody] OilDepositRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body." });

            var date = request.date?.ToUniversalTime() ?? DateTime.UtcNow;

            var countForMonth = await _context.OilDeposits
                .CountAsync(d => d.date.Month == date.Month && d.date.Year == date.Year);

            var deposit = new oilDeposit
            {
                amount = request.amount ?? 0.0f,
                comment = request.comment ?? string.Empty,
                date = date,
                monthlyId = countForMonth + 1
            };

            _context.OilDeposits.Add(deposit);
            await _context.SaveChangesAsync(); // Save to get deposit.Id

            // Get the latest balance before this deposit date
            var latestBalance = await _context.oilAccountBalances
                .Where(b => b.DateTime <= deposit.date)
                .OrderByDescending(b => b.DateTime)
                .ThenByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            decimal baseAmount = latestBalance?.BalanceAmount ?? 0;
            decimal depositAmount = (decimal)deposit.amount;

            var newBalance = new oilAccountBalance
            {
                BalanceAmount = baseAmount + depositAmount,
                DateTime = deposit.date
            };

            _context.oilAccountBalances.Add(newBalance);
            await _context.SaveChangesAsync(); // Save to get newBalance.Id

            // 🔥 Link the deposit to the created balance
            deposit.OilBalanceId = newBalance.Id;
            _context.OilDeposits.Update(deposit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = deposit.Id }, new
            {
                message = "Deposit created.",
                deposit
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeposit(int id, [FromBody] OilDepositRequest request)
        {
            var deposit = await _context.OilDeposits.FindAsync(id);
            if (deposit == null) return NotFound(new { message = "Deposit not found." });

            deposit.amount = request.amount ?? deposit.amount;
            deposit.comment = request.comment ?? deposit.comment;
            deposit.date = request.date?.ToUniversalTime() ?? deposit.date;

            // Find the balance linked to this deposit
            oilAccountBalance createdBalance = null;
            if (deposit.OilBalanceId.HasValue)
            {
                createdBalance = await _context.oilAccountBalances.FindAsync(deposit.OilBalanceId.Value);
            }

            if (createdBalance != null)
            {
                // Update the linked balance
                var previousBalance = await _context.oilAccountBalances
                    .Where(b => b.DateTime < deposit.date ||
                                (b.DateTime == deposit.date && b.Id < createdBalance.Id))
                    .OrderByDescending(b => b.DateTime)
                    .ThenByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                decimal baseAmount = previousBalance?.BalanceAmount ?? 0;
                decimal depositAmount = (decimal)deposit.amount;

                createdBalance.BalanceAmount = baseAmount + depositAmount;
                createdBalance.DateTime = deposit.date; // Update the date if needed
            }
            else
            {
                // Create a new balance if it didn't exist
                var newBalance = new oilAccountBalance
                {
                    BalanceAmount = (decimal)deposit.amount,
                    DateTime = deposit.date
                };
                _context.oilAccountBalances.Add(newBalance);
                await _context.SaveChangesAsync();

                // 🔥 Link the new balance to deposit
                deposit.OilBalanceId = newBalance.Id;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Deposit updated.", deposit });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deposit = await _context.OilDeposits.FindAsync(id);
            if (deposit == null) return NotFound();

            _context.OilDeposits.Remove(deposit);

            // 🔥 Optionally delete linked balance too
            if (deposit.OilBalanceId.HasValue)
            {
                var balance = await _context.oilAccountBalances.FindAsync(deposit.OilBalanceId.Value);
                if (balance != null)
                {
                    _context.oilAccountBalances.Remove(balance);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("by-date")]
        public async Task<ActionResult<IEnumerable<oilDeposit>>> GetByMonthAndYear([FromQuery] DateTime date)
        {
            var results = await _context.OilDeposits
                .Where(d => d.date.Month == date.Month && d.date.Year == date.Year)
                .ToListAsync();

            return Ok(results);
        }
    }

    public class OilDepositRequest
    {
        public float? amount { get; set; }
        public string comment { get; set; }
        public DateTime? date { get; set; }
    }
}
