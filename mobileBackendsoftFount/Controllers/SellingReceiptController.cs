using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;

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

                // 🔹 Calculate total sold before updating counters
                long totalSold = gunCounterRequest.EndRoundThreeCount - existingCounter.EndRoundThreeCount;

                // 🔹 Update counter values
                existingCounter.StartCount = existingCounter.EndRoundThreeCount;
                existingCounter.EndRoundOneCount = gunCounterRequest.EndRoundOneCount;
                existingCounter.EndRoundTwoCount = gunCounterRequest.EndRoundTwoCount;
                existingCounter.EndRoundThreeCount = gunCounterRequest.EndRoundThreeCount;

                // 🔹 Ensure total sold is updated
                existingCounter.TotalSold = totalSold;

                if (existingCounter.BenzeneType == "92")
                    total92 += totalSold;
                else if (existingCounter.BenzeneType == "95")
                    total95 += totalSold;

                updatedCounters.Add(existingCounter);
            }

            // 🔹 Fetch the Benzene price from the database
            var benzene92 = await _context.Benzenes.FirstOrDefaultAsync(b => b.Name == "92");
            var benzene95 = await _context.Benzenes.FirstOrDefaultAsync(b => b.Name == "95");

            if (benzene92 == null || benzene95 == null)
            {
                return BadRequest("Benzene pricing information is missing.");
            }

            // 🔹 Calculate total money
            long totalMoney92 = total92 * (long)benzene92.PriceOfSelling;
            long totalMoney95 = total95 * (long)benzene95.PriceOfSelling;
            long totalMoney = totalMoney92 + totalMoney95;

            request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);

            var newReceipt = new SellingReceipt
            {
                Date = request.Date, // Ensure the date has no time
                BenzeneGunCounters = updatedCounters,
                TotalLitre92 = total92,
                TotalLitre95 = total95,
                TotalMoney92 = totalMoney92,
                TotalMoney95 = totalMoney95,
                TotalMoney = totalMoney,
                OpenAmount = request.OpenAmount  // ✅ Set OpenAmount from request
            };

            _context.SellingReceipts.Add(newReceipt);
            await _context.SaveChangesAsync();

            // 🔹 Return response with properly calculated values
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
                    totalSold = g.TotalSold // ✅ Now correctly returns calculated totalSold
                }),
                totalLitre92 = newReceipt.TotalLitre92,
                totalLitre95 = newReceipt.TotalLitre95,
                totalMoney92 = newReceipt.TotalMoney92,
                totalMoney95 = newReceipt.TotalMoney95,
                totalMoney = newReceipt.TotalMoney
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

        // [HttpPut("update/{date}")]
        // public async Task<IActionResult> UpdateSellingReceiptByDate(DateTime date, [FromBody] SellingReceiptRequest request)
        // {
        //     if (request == null || request.BenzeneGunCounters == null || request.BenzeneGunCounters.Count == 0)
        //         return BadRequest("Invalid input data.");

        //     // 🔹 Ensure input date is UTC
        //     var inputDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

        //     var existingReceipt = await _context.SellingReceipts
        //         .Include(r => r.BenzeneGunCounters)
        //         .FirstOrDefaultAsync(r => r.Date == inputDate);

        //     if (existingReceipt == null)
        //         return NotFound("No selling receipt found for this date.");

        //     // 🔹 Ensure request.Date is explicitly set to UTC
        //     request.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);

        //     bool dateExists = await _context.SellingReceipts
        //         .AnyAsync(r => r.Date.Date == request.Date.Date && r.Date.Date != inputDate);

        //     if (dateExists)
        //         return Conflict("A selling receipt already exists for this new date.");

        //     long total92 = 0, total95 = 0;
        //     var updatedCounters = new List<BenzeneGunCounter>();

        //     await using var transaction = await _context.Database.BeginTransactionAsync();

        //     foreach (var gunCounterRequest in request.BenzeneGunCounters)
        //     {
        //         var existingCounter = await _context.BenzeneGunCounters
        //             .FirstOrDefaultAsync(g => g.GunNumber == gunCounterRequest.GunNumber);

        //         if (existingCounter == null)
        //             return NotFound($"Gun with number {gunCounterRequest.GunNumber} not found.");

        //         long totalSold = gunCounterRequest.EndRoundThreeCount - existingCounter.EndRoundThreeCount;
        //         if (totalSold < 0) return BadRequest($"Invalid counter values for Gun {gunCounterRequest.GunNumber}.");

        //         existingCounter.StartCount = existingCounter.EndRoundThreeCount;
        //         existingCounter.EndRoundOneCount = gunCounterRequest.EndRoundOneCount;
        //         existingCounter.EndRoundTwoCount = gunCounterRequest.EndRoundTwoCount;
        //         existingCounter.EndRoundThreeCount = gunCounterRequest.EndRoundThreeCount;
        //         existingCounter.TotalSold = totalSold;

        //         if (existingCounter.BenzeneType == "92") total92 += totalSold;
        //         if (existingCounter.BenzeneType == "95") total95 += totalSold;

        //         updatedCounters.Add(existingCounter);
        //     }

        //     var benzene92 = await _context.Benzenes.FirstOrDefaultAsync(b => b.Name == "92");
        //     var benzene95 = await _context.Benzenes.FirstOrDefaultAsync(b => b.Name == "95");

        //     if (benzene92 == null || benzene95 == null)
        //         return BadRequest("Benzene pricing information is missing.");

        //     var totalMoney92 = total92 * (long)benzene92.PriceOfSelling;
        //     var totalMoney95 = total95 * (long)benzene95.PriceOfSelling;
        //     var totalMoney = totalMoney92 + totalMoney95;

        //     // 🔹 Set all DateTime fields to UTC before saving
        //     existingReceipt.Date = request.Date;
        //     existingReceipt.BenzeneGunCounters = updatedCounters;
        //     existingReceipt.TotalLitre92 = total92;
        //     existingReceipt.TotalLitre95 = total95;

        //     existingReceipt.TotalMoney92 = totalMoney92;
        //     existingReceipt.TotalMoney95 = totalMoney95;
        //     existingReceipt.TotalMoney = totalMoney;
        //     existingReceipt.OpenAmount = request.OpenAmount;

        //     await _context.SaveChangesAsync();
        //     await transaction.CommitAsync();

        //     return Ok(existingReceipt);
        // }

        

        // 🔹 Request Model for input
        public class SellingReceiptRequest
        {
            public DateTime Date { get; set; }
            public List<BenzeneGunCounterRequest> BenzeneGunCounters { get; set; }
            public long OpenAmount { get; set; } // New property
        }

        public class BenzeneGunCounterRequest
        {
            public int GunNumber { get; set; }
            public long EndRoundOneCount { get; set; }
            public long EndRoundTwoCount { get; set; }
            public long EndRoundThreeCount { get; set; }
        }
    }
}
