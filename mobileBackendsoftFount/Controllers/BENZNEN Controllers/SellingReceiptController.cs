using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using Newtonsoft.Json;


namespace mobileBackendsoftFount.Controllers
{
    [Route("api/sellingReceipt")]
    [ApiController]
    // [Authorize(Roles = "Admin")]  // 🔹 Restrict access to admins only
    public class SellingReceiptController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SellingReceiptController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSellingReceipt([FromBody] SellingReceiptRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data. Please check your request." });
            }

            if (request == null || request.BenzeneGunCounters == null || request.BenzeneGunCounters.Count == 0)
            {
                return BadRequest(new { message = "Invalid input data." });
            }

            DateTime inputDate = request.Date.Date;

            long total92 = 0, total95 = 0;
            List<BenzeneGunCounter> updatedCounters = new List<BenzeneGunCounter>();


            request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
            // Get last saved SellingReceipt before the current date
            var previousReceipt = await _context.SellingReceipts
                .Where(r => r.Date < request.Date)
                .OrderByDescending(r => r.Date)
                .Include(r => r.BenzeneGunCounters)
                .FirstOrDefaultAsync();

            long openAmount92 = 0;
            long openAmount95 = 0;

            if (previousReceipt != null)
            {
                openAmount92 = previousReceipt.BenzeneGunCounters
                    .Where(c => c.BenzeneType == "92")
                    .Sum(c => c.EndRoundThreeCount);

                openAmount95 = previousReceipt.BenzeneGunCounters
                    .Where(c => c.BenzeneType == "95")
                    .Sum(c => c.EndRoundThreeCount);
            }

            foreach (var gunCounterRequest in request.BenzeneGunCounters)
            {
                // ❌ Validate that none of the values are negative
                if (gunCounterRequest.GunNumber == null ||
                    gunCounterRequest.EndRoundOneCount == null ||
                    gunCounterRequest.EndRoundTwoCount == null ||
                    gunCounterRequest.EndRoundThreeCount == null)
                {
                    return BadRequest(new { message = "Gun counters must not have null values." });
                }

                if (gunCounterRequest.EndRoundOneCount < 0 ||
                    gunCounterRequest.EndRoundTwoCount < 0 ||
                    gunCounterRequest.EndRoundThreeCount < 0)
                {
                    return BadRequest(new { message = "Gun counter values cannot be negative." });
                }

                var existingCounter = await _context.BenzeneGunCounters
                    .FirstOrDefaultAsync(g => g.GunNumber == gunCounterRequest.GunNumber);

                if (existingCounter == null)
                {
                    return NotFound(new { message = $"Gun with number {gunCounterRequest.GunNumber} not found." });
                }

                long totalSold = gunCounterRequest.EndRoundThreeCount.Value - existingCounter.EndRoundThreeCount;

                // ❌ Validate that total sold is not negative
                if (totalSold < 0)
                {
                    return BadRequest(new { message = $"Total sold for gun {gunCounterRequest.GunNumber} cannot be negative." });
                }

                existingCounter.StartCount = existingCounter.EndRoundThreeCount;
                existingCounter.EndRoundOneCount = gunCounterRequest.EndRoundOneCount ?? 0;
                existingCounter.EndRoundTwoCount = gunCounterRequest.EndRoundTwoCount ?? 0;
                existingCounter.EndRoundThreeCount = gunCounterRequest.EndRoundThreeCount ?? 0;
                existingCounter.TotalSold = totalSold;

                if (existingCounter.BenzeneType == "92")
                    total92 += totalSold;
                else if (existingCounter.BenzeneType == "95")
                    total95 += totalSold;

                updatedCounters.Add(existingCounter);
            }

            var benzene92 = await _context.Benzenes.FirstOrDefaultAsync(b => b.Name == "92");
            var benzene95 = await _context.Benzenes.FirstOrDefaultAsync(b => b.Name == "95");

            if (benzene92 == null || benzene95 == null)
            {
                return BadRequest(new { message = "Benzene pricing information is missing." });
            }

            long totalMoney92 = total92 * (long)benzene92.PriceOfSelling;
            long totalMoney95 = total95 * (long)benzene95.PriceOfSelling;
            long totalMoney = totalMoney92 + totalMoney95;

            // ❌ Validate that total money is not negative
            if (totalMoney < 0)
            {
                return BadRequest(new { message = "Total money cannot be negative." });
            }



            var newReceipt = new SellingReceipt
            {
                Date = request.Date,
                BenzeneGunCounters = updatedCounters,
                TotalLitre92 = total92,
                TotalLitre95 = total95,
                TotalMoney92 = totalMoney92,
                TotalMoney95 = totalMoney95,
                TotalMoney = totalMoney,
                OpenAmount92 = openAmount92,
                OpenAmount95 = openAmount95,

            };

            _context.SellingReceipts.Add(newReceipt);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSellingReceiptByDate), new { date = inputDate }, new
            {
                message = "Selling receipt created successfully.",
                id = newReceipt.Id,
                date = inputDate.ToString("yyyy-MM-dd"),
                totalLitre92 = newReceipt.TotalLitre92,
                totalLitre95 = newReceipt.TotalLitre95,
                totalMoney92 = newReceipt.TotalMoney92,
                totalMoney95 = newReceipt.TotalMoney95,
                totalMoney = newReceipt.TotalMoney,
                openAmount92 = newReceipt.OpenAmount92,
                openAmount95 = newReceipt.OpenAmount95

            });
        }

        // 🔹Get SellingReceipt by date
        [HttpGet("{date}")]
        public async Task<IActionResult> GetSellingReceiptByDate(DateTime date)
        {
            date = date.ToUniversalTime(); // Ensure comparison in UTC

            var receipt = await _context.SellingReceipts
                .Include(r => r.BenzeneGunCounters)
                .FirstOrDefaultAsync(r => r.Date == date);

            if (receipt == null)
                return NotFound(new{message="Not created yet."});

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


        // 🔹 Request Model for input
        public class SellingReceiptRequest
        {
            public DateTime Date { get; set; }
            public List<BenzeneGunCounterRequest> BenzeneGunCounters { get; set; }
            // public long OpenAmount { get; set; } // New property
        }

        public class BenzeneGunCounterRequest
        {
            public int? GunNumber { get; set; }
            public long? EndRoundOneCount { get; set; }
            public long? EndRoundTwoCount { get; set; }
            public long? EndRoundThreeCount { get; set; }
        }


    }
}
