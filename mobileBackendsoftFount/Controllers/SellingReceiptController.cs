using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/sellingReceipt")]
    [ApiController]
    [Authorize(Roles = "Admin")]  // 🔹 Restrict access to admins only
    public class SellingReceiptController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SellingReceiptController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Create a new SellingReceipt
        [HttpPost]
        public async Task<IActionResult> CreateSellingReceipt([FromBody] SellingReceiptRequest request)
        {
            if (request == null || request.BenzeneGunCounters == null || request.BenzeneGunCounters.Count == 0)
                return BadRequest("Invalid input data.");

            // 🔹 Store the date as DateOnly (no time)
            DateTime inputDate = request.Date.Date; // Removes time portion

            long total92 = 0, total95 = 0;
            List<BenzeneGunCounter> updatedCounters = new List<BenzeneGunCounter>();

            foreach (var gunCounterRequest in request.BenzeneGunCounters)
            {
                var existingCounter = await _context.BenzeneGunCounters
                    .FirstOrDefaultAsync(g => g.GunNumber == gunCounterRequest.GunNumber);

                if (existingCounter == null)
                {
                    return NotFound($"Gun with number {gunCounterRequest.GunNumber} not found.");
                }

                long totalSold = gunCounterRequest.EndRoundThreeCount - existingCounter.EndRoundThreeCount;

                existingCounter.StartCount = existingCounter.EndRoundThreeCount;
                existingCounter.EndRoundOneCount = gunCounterRequest.EndRoundOneCount;
                existingCounter.EndRoundTwoCount = gunCounterRequest.EndRoundTwoCount;
                existingCounter.EndRoundThreeCount = gunCounterRequest.EndRoundThreeCount;

                if (existingCounter.BenzeneType == "92")
                    total92 += totalSold;
                else if (existingCounter.BenzeneType == "95")
                    total95 += totalSold;

                updatedCounters.Add(existingCounter);
            }
            request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
            var newReceipt = new SellingReceipt
            {
                Date = request.Date,// Ensure the date has no time
                BenzeneGunCounters = updatedCounters,
                TotalSold92 = total92,
                TotalSold95 = total95
            };

            _context.SellingReceipts.Add(newReceipt);
            await _context.SaveChangesAsync();

            // 🔹 Return date in "YYYY-MM-DD" format
            return CreatedAtAction(nameof(GetSellingReceiptByDate), new { date = inputDate }, new
            {
                id = newReceipt.Id,
                date = inputDate.ToString("yyyy-MM-dd"), // Format response date properly
                benzeneGunCounters = newReceipt.BenzeneGunCounters.Select(g => new
                {
                    g.Id,
                    g.StartCount,
                    g.EndRoundOneCount,
                    g.EndRoundTwoCount,
                    g.EndRoundThreeCount,
                    g.BenzeneType,
                    g.GunNumber,
                    g.TotalSold
                }),
                totalSold92 = newReceipt.TotalSold92,
                totalSold95 = newReceipt.TotalSold95
            });
        }

        // 🔹 Get SellingReceipt by date
        [HttpGet("{date}")]
        public async Task<IActionResult> GetSellingReceiptByDate(DateTime date)
        {
            date = date.ToUniversalTime(); // Ensure comparison in UTC

            var receipt = await _context.SellingReceipts
                .Include(r => r.BenzeneGunCounters)
                .FirstOrDefaultAsync(r => r.Date == date);

            if (receipt == null)
                return NotFound("No SellingReceipt found for this date.");

            return Ok(receipt);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSellingReceipt(int id)
        {
            var receipt = await _context.SellingReceipts
                .Include(r => r.BenzeneGunCounters)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null)
                return NotFound("Selling receipt not found.");

            _context.SellingReceipts.Remove(receipt);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Selling receipt deleted successfully." });
        }

        // 🔹 Get all SellingReceipts
        [HttpGet]
        public async Task<IActionResult> GetAllSellingReceipts()
        {
            var receipts = await _context.SellingReceipts
                .Include(r => r.BenzeneGunCounters)
                .ToListAsync();

            return Ok(receipts);
        }
    }

    // 🔹 Request Model for input
    public class SellingReceiptRequest
    {
        public DateTime Date { get; set; }
        public List<BenzeneGunCounterRequest> BenzeneGunCounters { get; set; }
    }

    public class BenzeneGunCounterRequest
    {
        public int GunNumber { get; set; }
        public long EndRoundOneCount { get; set; }
        public long EndRoundTwoCount { get; set; }
        public long EndRoundThreeCount { get; set; }
    }
}
