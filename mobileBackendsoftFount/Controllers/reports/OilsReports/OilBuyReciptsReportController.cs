using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Models;
using System;
using System.Linq;
using mobileBackendsoftFount.Data;

using System.Threading.Tasks;

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OilBuyReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilBuyReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("report")]
        public async Task<ActionResult<OilBuyReciptsReport>> GetOilBuyReport([FromQuery] OilBuyReportRequest request)
        {


            var supplierName = request.SupplierName;
            var startDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);

            var supplier = await _context.OilSuppliers.FirstOrDefaultAsync(s => s.Name == supplierName);
            if (supplier == null)
            {
                return NotFound("Supplier not found");
            }

            var receipts = await _context.OilBuyReceipts
                .Include(r => r.OilBuyProducts)
                .Where(r => r.Date >= startDate && r.Date <= endDate && r.SupplierId == supplier.Id)
                .ToListAsync();

            var oils = await _context.Oils
                .Where(o => o.SupplierId == supplier.Id)
                .ToListAsync();

            var report = new OilBuyReciptsReport();

            // Populate members...
            foreach (var receipt in receipts)
            {
                var member = new OilBuyReciptsReportMember
                {
                    ReciptNumber = receipt.Id,
                    Date = receipt.Date,
                    TotalMoney = receipt.TotalValue,
                    TotalMoneyBeforeTaxes = receipt.TotalValue * 0.86m, // 14% tax deduction
                    TotalMoneyOfSell = 0.0m, // We'll calculate this
                    BoughtOilsList = new List<BoughtOils>()
                };

                decimal totalSell = 0.0m;

                foreach (var product in receipt.OilBuyProducts)
                {
                    var matchingOil = oils.FirstOrDefault(o => o.Name == product.Name);
                    if (matchingOil == null) continue;

                    var sellValue = product.Amount * (decimal)matchingOil.PriceOfSelling;

                    var boughtOil = new BoughtOils
                    {
                        Name = product.Name,
                        Amount = product.Amount,
                        Price = (decimal)matchingOil.Price,
                        PriceOfSell = (decimal)matchingOil.PriceOfSelling,
                        ValueOfPrice = product.Value,
                        ValueOfPriceBeforeTaxes = product.Value/(1.0m+0.14m),
                        ValueOfPriceOfSell = sellValue,
                        ValueOfPriceOfSellBeforeTaxes = sellValue /(1.0m+0.14m),
                        ProfitWithoutTaxes = (sellValue * 0.86m) - (product.Value * 0.86m)
                    };

                    member.BoughtOilsList.Add(boughtOil);
                    totalSell += sellValue;
                }

                member.TotalMoneyOfSell = totalSell;
                member.TotalMoneyOfSellBeforeTaxes = totalSell * 0.86m;
                member.ProfitWithoutTaxes = member.TotalMoneyOfSell - member.TotalMoney;

                report.Members.Add(member);
            }

            // After populating all the members, calculate TotalProfit:
            report.GeneralProfit = report.Members.Sum(m => m.ProfitWithoutTaxes);


            // Add this after building the report.Members
            var summary = receipts.Select(r => new OilBuyReceiptSummaryDto
            {
                Date = r.Date,
                Round = r.Round,
                Amount = r.OilBuyProducts.Sum(p => p.Amount * p.Weight),
                TotalValue = r.TotalValue,
                NetValue = r.TotalValue
            }).ToList();



            var result = new OilBuyReportWithSummary
                {
                    Report = report,
                    Summary = summary 
                };

                return Ok(result);


        }




        // [HttpGet("summary")]
        // public async Task<ActionResult<IEnumerable<OilBuyReceiptSummaryDto>>> GetBuyReceiptsSummary([FromQuery] DateTime startDate,[FromQuery] DateTime endDate)
        // {

        //     startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        //     endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
        //     var receipts = await _context.OilBuyReceipts
        //         .Include(r => r.OilBuyProducts)
        //         .Where(r => r.Date >= startDate && r.Date <= endDate)
        //         .ToListAsync();

        //     var result = receipts.Select(r => new OilBuyReceiptSummaryDto
        //     {
        //         Date = r.Date,
        //         Round = r.Round, // Adjust or map if `Round` is stored differently
        //         Amount = r.OilBuyProducts.Sum(p => p.Amount * p.Weight),
        //         TotalValue = r.TotalValue,
        //         NetValue = r.TotalValue
        //     }).ToList();

        //     return Ok(result);
        // }

    }


        public class OilBuyReportRequest
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string SupplierName { get; set; } = string.Empty;
        }


        public class OilBuyReceiptSummaryDto
        {
            public DateTime Date { get; set; }
            public int Round { get; set; } // Adjust type if needed
            public decimal Amount { get; set; }
            public decimal TotalValue { get; set; }
            public decimal NetValue { get; set; }
        }




        public class OilBuyReportWithSummary
        {
            public OilBuyReciptsReport Report { get; set; }
            public List<OilBuyReceiptSummaryDto> Summary { get; set; }
        }


}
