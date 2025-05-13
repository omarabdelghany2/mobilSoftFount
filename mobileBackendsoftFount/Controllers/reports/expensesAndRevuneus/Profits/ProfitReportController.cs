using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfitReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfitReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ProfitReport>> GetProfitReport(DateTime startDate, DateTime endDate)
        {
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var report = new ProfitReport();

            var benzene = await _context.Benzenes.FirstOrDefaultAsync();
            if (benzene == null)
                return BadRequest("Benzene data not found.");

            var benzeneSellReceipts = await _context.SellingReceipts
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .ToListAsync();

            var oilSellReceipts = await _context.OilSellRecipes
                .Include(r => r.OilSellProducts)
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .ToListAsync();

            var serviceReceipts = await _context.ServiceSellReceipts
                .Include(r => r.ServiceSellProducts)
                    .ThenInclude(sp => sp.ClientServices)
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .ToListAsync();

            var subCategories = await _context.SubCategories.ToListAsync();

            // 🔹 Aggregate Benzene
            var totalLitre92 = benzeneSellReceipts.Sum(r => r.TotalLitre92);
            var totalLitre95 = benzeneSellReceipts.Sum(r => r.TotalLitre95);
            var totalMoney92 = benzeneSellReceipts.Sum(r => r.TotalMoney92);
            var totalMoney95 = benzeneSellReceipts.Sum(r => r.TotalMoney95);

            var member92 = new ProfitMember
            {
                Name = "Benzene92",
                SoldAmount = totalLitre92,
                Price = (decimal)benzene.PriceOfSelling,
                ValueOfSold = totalMoney92,
                Commission = (decimal)benzene.RateOfVats,
                CostOfBuy = 0.0m,
                Profit = totalLitre92 * (decimal)benzene.RateOfVats
            };

            var member95 = new ProfitMember
            {
                Name = "Benzene95",
                SoldAmount = totalLitre95,
                Price = (decimal)benzene.PriceOfSelling,
                ValueOfSold = totalMoney95,
                Commission = (decimal)benzene.RateOfVats,
                CostOfBuy = 0.0m,
                Profit = totalLitre95 * (decimal)benzene.RateOfVats
            };

            var memberTotalBenzene = new ProfitMember
            {
                Name = "Benzene",
                SoldAmount = member92.SoldAmount + member95.SoldAmount,
                Price = 0.0m,
                ValueOfSold = member92.ValueOfSold + member95.ValueOfSold,
                Commission = 0.0m,
                CostOfBuy = (member92.SoldAmount + member95.SoldAmount) * (decimal)benzene.PriceOfLitre,
                Profit = member92.Profit + member95.Profit
            };

            // 🔹 Oil
            var oilValueOfSold = oilSellReceipts.Sum(r => r.TotalPrice);
            decimal oilCostOfBuy = 0.0m;

            foreach (var receipt in oilSellReceipts)
            {
                foreach (var product in receipt.OilSellProducts)
                {
                    var oil = await _context.Oils.FirstOrDefaultAsync(o => o.Name == product.Name);
                    if (oil != null)
                        oilCostOfBuy += product.SoldAmount * (decimal)oil.Price;
                }
            }

            var memberOils = new ProfitMember
            {
                Name = "Oils",
                SoldAmount = 0.0m,
                Price = 0.0m,
                ValueOfSold = oilValueOfSold,
                CostOfBuy = oilCostOfBuy,
                Profit = oilValueOfSold - oilCostOfBuy
            };

            // 🔹 Services
            var servicesValueOfSold = serviceReceipts.Sum(r => r.TotalPrice);
            decimal servicesCostOfBuy = 0.0m;

            foreach (var receipt in serviceReceipts)
            {
                foreach (var product in receipt.ServiceSellProducts)
                {
                    foreach (var service in product.ClientServices)
                    {
                        var sub = subCategories.FirstOrDefault(s =>
                            s.Name == service.SubCategoryName);

                        if (sub != null)
                            servicesCostOfBuy += (decimal)sub.PriceOfBuy;
                    }
                }
            }

            var memberServices = new ProfitMember
            {
                Name = "Services",
                SoldAmount = 0.0m,
                Price = 0.0m,
                ValueOfSold = servicesValueOfSold,
                CostOfBuy = servicesCostOfBuy,
                Profit = servicesValueOfSold - servicesCostOfBuy
            };

            // 🔹 Add members to report
            report.Members.Add(member92);
            report.Members.Add(member95);
            report.Members.Add(memberTotalBenzene);
            report.Members.Add(memberOils);
            report.Members.Add(memberServices);

            // ✅ Total Profit of all Members
            report.TotalProfitOfBenzeneAndOilAndServices = report.Members.Sum(m => m.Profit);

            // ✅ Total Expenses (DeductionFromProfit = true)
            report.TotalExpenses = await _context.Expenses
                .Where(e => e.Date >= startDate && e.Date <= endDate && e.ExpenseCategory.DeductionFromProfit)
                .SumAsync(e => e.Value);

            // ✅ Total Revenues
            report.TotalRevenues = await _context.Revenues
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .SumAsync(r => r.Value);

            // ✅ Net Profit
            report.NetProfit = report.TotalProfitOfBenzeneAndOilAndServices - report.TotalExpenses + report.TotalRevenues;


            // Get the oil sell recipe with the same date as endDate (ignoring time)
            var oilSellRecipe = await _context.OilSellRecipes
                .Include(r => r.OilSellProducts)
                .Where(r => r.Date.Date == endDate.Date)
                .FirstOrDefaultAsync();

            decimal oilStock = 0.0m;

            if (oilSellRecipe != null)
            {
                foreach (var product in oilSellRecipe.OilSellProducts)
                {
                    // Find the matching oil by name
                    var oil = await _context.Oils.FirstOrDefaultAsync(o => o.Name == product.Name);
                    if (oil != null)
                    {
                        oilStock += product.RoundThreeAmount * (decimal)oil.PriceOfSelling;
                    }
                }

            }
            else
            {
                oilStock = 0.0m; // No recipe found on that day
            }


            // 🔸 BENZENE STOCK
            var latestTank = await _context.BenzeneTanks
                .Where(t => t.date.Date == endDate.Date)
                .FirstOrDefaultAsync();

            decimal tankTwo92 = 0.0m;
            decimal tankOne95 = 0.0m;

            if (latestTank != null)
            {
                tankTwo92 = (decimal)latestTank.tankTwo92ATG;
                tankOne95 = (decimal)latestTank.tankOne95ATG;
            }

            var benzene92 = await _context.Benzenes.FirstOrDefaultAsync(b => b.Name == "92");
            var benzene95 = await _context.Benzenes.FirstOrDefaultAsync(b => b.Name == "95");

            decimal price92 = benzene92?.PriceOfSelling != null ? (decimal)benzene92.PriceOfSelling : 0.0m;
            decimal price95 = benzene95?.PriceOfSelling != null ? (decimal)benzene95.PriceOfSelling : 0.0m;

            report.BenzeneStock = (tankTwo92 * price92) + (tankOne95 * price95);
            report.OilStock=oilStock;



            return Ok(report);
        }


    }
}
