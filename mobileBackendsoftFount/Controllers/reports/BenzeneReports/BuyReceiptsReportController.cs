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
using System.Security.Claims;
using System.Text;
using mobileBackendsoftFount.Data;
using BCrypt.Net;






namespace mobileBackendsoftFount.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyReceiptsReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BuyReceiptsReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("generate-buy-recipt-report")]
        public async Task<IActionResult> GenerateReport(string type, string dateInput)
        {
            DateTime startDate, endDate;
            Console.WriteLine($"Received type: {type}, dateInput: '{dateInput}'");


            try
            {
                switch (type.ToLower())
                {
                    case "period":
                        var dates = dateInput.Split(" - ");
                        startDate = DateTime.Parse(dates[0]).ToUniversalTime();
                        endDate = DateTime.Parse(dates[1]).ToUniversalTime();
                        break;

                    case "month":
                        dateInput = dateInput.Trim();
                        startDate = DateTime.ParseExact(dateInput + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture).ToUniversalTime();
                        endDate = startDate.AddMonths(1).AddDays(-1).ToUniversalTime();
                        break;

                    case "year":
                    {
                        int year = int.Parse(dateInput);
                        startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        endDate = startDate.AddYears(1).AddDays(-1).ToUniversalTime();

                        var yearlyReceipts = await _context.BenzeneBuyReceipts
                            .Include(r => r.Products)
                            .Where(r => r.MobilReceiptDate >= startDate && r.MobilReceiptDate <= endDate) // Correcting to use MobilReceiptDate
                            .ToListAsync();

                        // Group the receipts by month and year
                        var monthlyGroups = yearlyReceipts
                            .GroupBy(r => new { r.MobilReceiptDate.Year, r.MobilReceiptDate.Month })
                            .OrderBy(g => g.Key.Month);

                        // Create a list of all months (January to December)
                        var allMonths = Enumerable.Range(1, 12).Select(month => new { Year = year, Month = month }).ToList();

                        // Now, for each month in the year, make sure there's data (if missing, set to zeros)
                        var yearlyMembers = allMonths.Select(month =>
                        {
                            var matchingGroup = monthlyGroups.FirstOrDefault(g => g.Key.Month == month.Month);

                            var litre95 = 0.0m;
                            var litre92 = 0.0m;
                            var value95 = 0.0m;
                            var value92 = 0.0m;
                            var totalAmount = 0.0m;
                            var totalValue = 0.0m;
                            var totalEvaporation = 0.0m;
                            var totalTaxes = 0.0m;
                            var netValue = 0.0m;

                            if (matchingGroup != null)
                            {
                                var allProducts = matchingGroup.SelectMany(r => r.Products).ToList();

                                litre95 = Math.Round(allProducts
                                    .Where(p => p.ProductName == "95")
                                    .Sum(p => (decimal)p.Amount), 2);

                                litre92 = Math.Round(allProducts
                                    .Where(p => p.ProductName == "92")
                                    .Sum(p => (decimal)p.Amount), 2);

                                value95 = Math.Round(allProducts
                                    .Where(p => p.ProductName == "95")
                                    .Sum(p => (decimal)p.Amount * (decimal)p.PricePerLiter), 2);

                                value92 = Math.Round(allProducts
                                    .Where(p => p.ProductName == "92")
                                    .Sum(p => (decimal)p.Amount * (decimal)p.PricePerLiter), 2);

                                totalAmount = Math.Round(allProducts.Sum(p => (decimal)p.Amount), 2);
                                totalValue = Math.Round(allProducts.Sum(p => (decimal)p.Amount * (decimal)p.PricePerLiter), 2);
                                totalEvaporation = Math.Round(allProducts.Sum(p => (decimal)p.ValueOfEvaporation), 2);
                                totalTaxes = Math.Round(allProducts.Sum(p => (decimal)p.ValueOfTaxes), 2);
                                netValue = Math.Round(matchingGroup.Sum(r => (decimal)r.TotalValue), 2);
                            }

                            // Use the first day of the month for the Date (e.g., January 1st)
                            var monthDate = new DateTime(month.Year, month.Month, 1);

                            return new BuyReciptsInvesigationMember
                            {
                                Date = monthDate,  // Keep Date as DateTime
                                Benzene95Litre = litre95,
                                Benzene92Litre = litre92,
                                Benzene95Value = value95,
                                Benzene92Value = value92,
                                BenzeneTotalAmount = totalAmount,
                                BenzeneTotalValue = totalValue,
                                BenzeneTotalEvaporationValue = totalEvaporation,
                                BenzeneTotalTaxesValue = totalTaxes,
                                BuyReciptNetValue = netValue
                            };
                        }).ToList();

                        // Construct the final response
                        var yearlyReport = new BuyReciptsInvesigation
                        {
                            Members = yearlyMembers
                        };

                        return Ok(yearlyReport);
                    }


                    default:
                        return BadRequest("Invalid type. Use 'period', 'month', or 'year'.");
                }
            }
            catch
            {
                return BadRequest("Invalid date format.");
            }

            // This code is executed for "period" and "month" types:
            var receipts = await _context.BenzeneBuyReceipts
                .Include(r => r.Products)
                .Where(r => r.MobilReceiptDate >= startDate && r.MobilReceiptDate <= endDate)
                .ToListAsync();

            var members = receipts.Select(r =>
            {
                var litre95 = Math.Round(r.Products
                    .Where(p => p.ProductName == "95")
                    .Sum(p => (decimal)p.Amount), 2);

                var litre92 = Math.Round(r.Products
                    .Where(p => p.ProductName == "92")
                    .Sum(p => (decimal)p.Amount), 2);

                var value95 = Math.Round(r.Products
                    .Where(p => p.ProductName == "95")
                    .Sum(p => (decimal)p.Amount * (decimal)p.PricePerLiter), 2);

                var value92 = Math.Round(r.Products
                    .Where(p => p.ProductName == "92")
                    .Sum(p => (decimal)p.Amount * (decimal)p.PricePerLiter), 2);

                var totalAmount = Math.Round(r.Products.Sum(p => (decimal)p.Amount), 2);
                var totalValue = Math.Round(r.Products.Sum(p => (decimal)p.Amount * (decimal)p.PricePerLiter), 2);

                var totalEvaporation = Math.Round(r.Products.Sum(p => (decimal)p.ValueOfEvaporation), 2);
                var totalTaxes = Math.Round(r.Products.Sum(p => (decimal)p.ValueOfTaxes), 2);
                var netValue = Math.Round((decimal)r.TotalValue, 2);

                return new BuyReciptsInvesigationMember
                {
                    Id = r.Id,  // Correctly using Id here
                    Date = r.MobilReceiptDate,
                    Benzene95Litre = litre95,
                    Benzene92Litre = litre92,
                    Benzene95Value = value95,
                    Benzene92Value = value92,
                    BenzeneTotalAmount = totalAmount,
                    BenzeneTotalValue = totalValue,
                    BenzeneTotalEvaporationValue = totalEvaporation,
                    BenzeneTotalTaxesValue = totalTaxes,
                    BuyReciptNetValue = netValue
                };
            }).OrderBy(m => m.Date).ToList();  // Sort by date

            var report = new BuyReciptsInvesigation
            {
                Members = members
            };

            return Ok(report);
        }
    }
}
