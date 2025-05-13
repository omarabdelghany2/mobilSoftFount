using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OilTotalySellingReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilTotalySellingReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("total-selling-report")]
        public async Task<ActionResult<OilTotalySellingReport>> GetTotalSellingReport(DateTime startDate, DateTime endDate)
        {

                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
                endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            // Fetch OilSellRecipes within date range
            var recipes = await _context.OilSellRecipes
                .Include(r => r.OilSellProducts)
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .OrderBy(r => r.Date)
                .ToListAsync();

            // Get all relevant oils to lookup Price and Weight by name
            var oils = await _context.Oils.ToListAsync();

            var report = new OilTotalySellingReport();

            foreach (var recipe in recipes)
            {
                decimal totalWeight = 0;
                decimal totalValue = 0;
                decimal totalSellingValue = 0;

                foreach (var product in recipe.OilSellProducts)
                {
                    var oil = oils.FirstOrDefault(o => o.Name == product.Name);
                    if (oil == null) continue;

                    decimal productWeight = product.SoldAmount * oil.Weight;
                    decimal productValue = product.SoldAmount * (decimal)oil.Price;
                    decimal productSellingValue = product.SoldPrice;

                    totalWeight += productWeight;
                    totalValue += productValue;
                    totalSellingValue += productSellingValue;
                }

                var member = new OilTotalySellingMember
                {
                    Date = recipe.Date,
                    Weight = totalWeight,
                    Value = totalValue,
                    SellingValue = totalSellingValue,
                    Profit = totalSellingValue - totalValue
                };

                report.Memebers.Add(member);
            }

            // Aggregate totals
            report.TotalWeight = report.Memebers.Sum(m => m.Weight);
            report.TotalValue = report.Memebers.Sum(m => m.Value);
            report.TotalSellingValue = report.Memebers.Sum(m => m.SellingValue);
            report.TotalProfit = report.Memebers.Sum(m => m.Profit);

            return Ok(report);
        }
    }
}
