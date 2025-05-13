using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesSalesReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesSalesReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("report")]
        public async Task<ActionResult<ServicesSalesReport>> GetReport(DateTime startDate, DateTime endDate)
        {
            // Ensure UTC kind for compatibility
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            // Get all receipts in date range
            var receipts = await _context.ServiceSellReceipts
                .Include(r => r.ServiceSellProducts)
                    .ThenInclude(p => p.ClientServices)
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .OrderBy(r => r.Date)
                .ToListAsync();

            var subCategories = await _context.SubCategories.ToListAsync();

            var report = new ServicesSalesReport();

            // Group receipts by day
            var groupedByDay = receipts
                .GroupBy(r => r.Date.Date);

            foreach (var dayGroup in groupedByDay)
            {
                var member = new ServicesSalesReportMember
                {
                    Date = dayGroup.Key
                };

                var serviceMap = new Dictionary<string, BoughtService>();

                foreach (var receipt in dayGroup)
                {
                    foreach (var product in receipt.ServiceSellProducts)
                    {
                        foreach (var clientService in product.ClientServices)
                        {
                            string serviceName = clientService.SubCategoryName;

                            // Ensure unique BoughtService per service name
                            if (!serviceMap.ContainsKey(serviceName))
                            {
                                var subCat = subCategories.FirstOrDefault(s => s.Name == serviceName);
                                if (subCat == null)
                                    continue;

                                serviceMap[serviceName] = new BoughtService
                                {
                                    Name = serviceName,
                                    Amount = 0,
                                    SoldValue = 0,
                                    SoldPrice = 0,
                                    Profit = 0
                                };
                            }

                            var service = serviceMap[serviceName];
                            var subCategory = subCategories.FirstOrDefault(s => s.Name == serviceName);
                            if (subCategory == null) continue;

                            service.Amount += 1;
                            service.SoldValue += (decimal)subCategory.PriceOfBuy;
                            service.SoldPrice += (decimal)subCategory.Price;
                            service.Profit += (decimal)(subCategory.Price - subCategory.PriceOfBuy);
                        }
                    }
                }

                member.BoughtServices = serviceMap.Values.ToList();

                // Summary values
                member.TotalAmount = member.BoughtServices.Sum(s => s.Amount);
                member.TotalSoldValue = member.BoughtServices.Sum(s => s.SoldValue);
                member.TotalSoldPrice = member.BoughtServices.Sum(s => s.SoldPrice);
                member.TotalProfit = member.BoughtServices.Sum(s => s.Profit);

                report.Members.Add(member);
            }

            return Ok(report);
        }
    }
}
