using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Models;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using mobileBackendsoftFount.Data;

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OilAnalyticallySellingReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilAnalyticallySellingReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("analytical-selling-report")]
        public async Task<ActionResult<OilAnalyticalSellingReport>> GetAnalyticalSellingReport(DateTime startDate, DateTime endDate)
        {
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            // Load all oils
            var oils = await _context.Oils.ToListAsync();

            // Build maps
            var oilWeightMap = oils.ToDictionary(o => o.Name, o => o.Weight);
            var oilPriceMap = oils.ToDictionary(o => o.Name, o => o.Price);

            var recipes = await _context.OilSellRecipes
                .Include(r => r.OilSellProducts)
                    .ThenInclude(p => p.OilSupplier)
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .ToListAsync();

            var allProducts = recipes
                .SelectMany(r => r.OilSellProducts.Select(p => new
                {
                    SupplierName = p.OilSupplier.Name,
                    Name = p.Name,
                    SoldAmount = p.SoldAmount,
                    SoldPrice = p.SoldPrice
                }))
                .ToList();

            var report = new OilAnalyticalSellingReport();

            report.Memebers = allProducts
                .GroupBy(p => new { p.SupplierName, p.Name })
                .Select(g =>
                {
                    var oilWeight = oilWeightMap.TryGetValue(g.Key.Name, out var w) ? w : 0;
                    var oilPrice = oilPriceMap.TryGetValue(g.Key.Name, out var p) ? p : 0;

                    return new OilAnalyticalSellingMember
                    {
                        SupplierName = g.Key.SupplierName,
                        Name = g.Key.Name,
                        Amount = g.Sum(x => x.SoldAmount),
                        Weight = g.Sum(x => x.SoldAmount * (decimal)oilWeight),
                        Value = g.Sum(x => x.SoldAmount * (decimal)oilPrice),
                        SellingValue = g.Sum(x => x.SoldPrice),
                        Profit = g.Sum(x => x.SoldPrice) - g.Sum(x => x.SoldAmount * (decimal)oilPrice)
                    };
                })
                .ToList();


            report.TotalAmount = report.Memebers.Sum(m => m.Amount);
            report.TotalWeight = report.Memebers.Sum(m => m.Weight);
            report.TotalValue = report.Memebers.Sum(m => m.Value);
            report.TotalSellingValue = report.Memebers.Sum(m => m.SellingValue);
            report.TotalProfit = report.Memebers.Sum(m => m.Profit);

            return Ok(report);
        }


    }

}
