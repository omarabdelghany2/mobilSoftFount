using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using mobileBackendsoftFount.Data;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OilStorageBalanceReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Your DbContext

        public OilStorageBalanceReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTodayReport()
        {
            var today = DateTime.UtcNow.Date;

            // Check if a report already exists for today
            var existingReport = await _context.OilStorageBalanceReports
                .FirstOrDefaultAsync(r => r.Date == today);

            if (existingReport != null)
            {
                return BadRequest(new { message = "Report for today already exists." });
            }

            // Get all oils
            var oils = await _context.Oils
                .Include(o => o.Supplier)
                .Where(o => o.Enable) // Only enabled oils
                .ToListAsync();

            if (oils == null || oils.Count == 0)
            {
                return BadRequest(new { message = "No oils found." });
            }

            // Create Products list
            var products = oils.Select(oil => new OilBalanceProduct
            {
                SupplierName = oil.Supplier?.Name ?? "Unknown",
                Name = oil.Name,
                Price = (decimal)oil.Price,
                PriceOfSell = (decimal)oil.PriceOfSelling,
                Amount = (decimal)oil.Amount
            }).ToList();

            // Calculate totals
            var totalBalance = products.Sum(p => p.Amount);
            var storagePrice = products.Sum(p => p.Amount * p.Price);
            var storagePriceOfSell = products.Sum(p => p.Amount * p.PriceOfSell);
            var profit = storagePriceOfSell - storagePrice;

            // Create the report
            var report = new OilStorageBalanceReport
            {
                Date = today,
                Products = products,
                TotalBalance = totalBalance,
                StoragePrice = storagePrice,
                StoragePriceOfSell = storagePriceOfSell,
                Profit = profit
            };

            // Save
            _context.OilStorageBalanceReports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(report);
        }

                // GET: api/oilstoragebalancereport/by-date?date=2025-04-26
        [HttpGet("by-date")]
        public async Task<IActionResult> GetReportByDate([FromQuery] DateTime date)
        {
            if (date.Kind == DateTimeKind.Unspecified)
            {
                date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

            var report = await _context.OilStorageBalanceReports
                .Include(r => r.Products)
                .FirstOrDefaultAsync(r => r.Date == date);

            if (report == null)
                return NotFound(new { message = "No report found for this date." });

            return Ok(report);
        }


        // DELETE: api/oilstoragebalancereport/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReportById(int id)
        {
            var report = await _context.OilStorageBalanceReports
                .Include(r => r.Products) // Important for Cascade (just for safety here)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return NotFound(new { message = $"No report found with ID {id}" });

            _context.OilStorageBalanceReports.Remove(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Report with ID {id} deleted successfully" });
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _context.OilStorageBalanceReports
                .OrderByDescending(r => r.Date)
                .Select(r => new 
                {
                    r.Id,
                    r.Date
                })
                .ToListAsync();

            return Ok(reports);
        }


    }
}
