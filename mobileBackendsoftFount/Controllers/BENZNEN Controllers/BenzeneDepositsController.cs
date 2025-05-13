using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/benzene-deposits")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class BenzeneDepositsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BenzeneDepositsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<benzeneDeposit>>> GetAll()
        {
            return await _context.BenzeneDeposits.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<benzeneDeposit>> GetById(int id)
        {
            var deposit = await _context.BenzeneDeposits.FindAsync(id);
            if (deposit == null) return NotFound();

            return deposit;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeposit([FromBody] BenzeneDepositRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body." });

            var date = request.date?.ToUniversalTime() ?? DateTime.UtcNow;

            var countForMonth = await _context.BenzeneDeposits
                .CountAsync(d => d.date.Month == date.Month && d.date.Year == date.Year);

            var deposit = new benzeneDeposit
            {
                amount = request.amount ?? 0.0f,
                comment = request.comment ?? string.Empty,
                date = date,
                monthlyId = countForMonth + 1
            };

            _context.BenzeneDeposits.Add(deposit);
            await _context.SaveChangesAsync();

            // Get the latest balance before this deposit date
            var latestBalance = await _context.Balances
                .Where(b => b.DateTime <= deposit.date)
                .OrderByDescending(b => b.DateTime)
                .ThenByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            decimal baseAmount = latestBalance?.BalanceAmount ?? 0;
            decimal depositAmount = (decimal)deposit.amount;

            var newBalance = new Balance
            {
                BalanceAmount = baseAmount + depositAmount,
                DateTime = deposit.date
            };

            _context.Balances.Add(newBalance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = deposit.Id }, new
            {
                message = "Deposit created.",
                deposit
            });
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeposit(int id, [FromBody] BenzeneDepositRequest request)
        {
            var deposit = await _context.BenzeneDeposits.FindAsync(id);
            if (deposit == null) return NotFound(new { message = "Deposit not found." });

            deposit.amount = request.amount ?? deposit.amount;
            deposit.comment = request.comment ?? deposit.comment;
            deposit.date = request.date?.ToUniversalTime() ?? deposit.date;

            // Find the balance created with this deposit
            var createdBalance = await _context.Balances
                .Where(b => b.DateTime <= deposit.date)
                .OrderByDescending(b => b.DateTime)
                .ThenByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            if (createdBalance != null)
            {
                int createdBalanceId = createdBalance.Id;

                var previousBalance = await _context.Balances
                    .Where(b =>
                        b.DateTime < deposit.date ||
                        (b.DateTime == deposit.date && b.Id < createdBalanceId))
                    .OrderByDescending(b => b.DateTime)
                    .ThenByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                decimal baseAmount = previousBalance?.BalanceAmount ?? 0;
                decimal depositAmount = (decimal)deposit.amount;

                createdBalance.BalanceAmount = baseAmount + depositAmount;
            }
            else
            {
                // No balance before, assume starting from 0
                var newBalance = new Balance
                {
                    BalanceAmount = (decimal)deposit.amount,
                    DateTime = deposit.date
                };
                _context.Balances.Add(newBalance);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Deposit updated.", deposit });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deposit = await _context.BenzeneDeposits.FindAsync(id);
            if (deposit == null) return NotFound();

            _context.BenzeneDeposits.Remove(deposit);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("by-date")]
        public async Task<ActionResult<IEnumerable<benzeneDeposit>>> GetByMonthAndYear([FromQuery] DateTime date)
        {
            var results = await _context.BenzeneDeposits
                .Where(d => d.date.Month == date.Month && d.date.Year == date.Year)
                .ToListAsync();

            return Ok(results);
        }

    }


    public class BenzeneDepositRequest
    {
        public float? amount { get; set; }
        public string comment { get; set; }
        public DateTime? date { get; set; }
    }

}
