using Microsoft.AspNetCore.Mvc;
using mobileBackendsoftFount.Models;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using System.Globalization;

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisabilityAndIncreaseReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DisabilityAndIncreaseReportController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost("generate")]
        public async Task<IActionResult> GenerateMonthlyReport([FromBody] ReportInputDto input)
        {
            var reportDate = new DateTime(input.ReportMonth.Year, input.ReportMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // 🔒 Check for existing report for the same month and benzene type
            var existingReport = await _context.DisabilityAndIncreaseReports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReportDate == reportDate && r.BenzeneType == input.BenzeneType);

            if (existingReport != null)
            {
                return Conflict(new { message = $"A report for {reportDate:yyyy-MM} with BenzeneType '{input.BenzeneType}' already exists." });
            }


            int daysInMonth = DateTime.DaysInMonth(input.ReportMonth.Year, input.ReportMonth.Month);

            var report = new DisabilityAndIncreaseReport
            {
                ReportDate = reportDate
            };

            decimal sellingCumulative = 0;
            decimal totalCumulative = 0;

            var table2Entries = new List<ReportTable2>();

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(input.ReportMonth.Year, input.ReportMonth.Month, day, 0, 0, 0, DateTimeKind.Utc);

                decimal atg = input.TotalAmountInTankATG.ElementAtOrDefault(day - 1);
                decimal water = 0;
                decimal totalWithoutWater = atg - water;

                var table2 = new ReportTable2
                {
                    day = day,
                    TotalAmountInTankATG = atg,
                    WaterAmountInCM = 0,
                    WaterAmount = water,
                    TotalAmountWithoutWater = totalWithoutWater
                };

                table2Entries.Add(table2);

                decimal startingBalance = (day == 1)
                    ? input.StartingBalanceOfDay
                    : table2Entries[day - 2].TotalAmountInTankATG;

                var receiving = _context.BenzeneBuyReceipts
                    .Include(r => r.Products)
                    .Where(r => r.MobilReceiptDate.ToUniversalTime().Date == date)
                    .SelectMany(r => r.Products)
                    .Where(p => p.ProductName == input.BenzeneType)
                    .Select(p => (decimal?)p.Amount)
                    .Sum() ?? 0;

                decimal calibration = _context.BenzeneCalibrations
                    .Where(c => c.date.ToUniversalTime().Date == date)
                    .Select(c => (decimal?)(input.BenzeneType == "92" ? c.amount92 : c.amount95))
                    .Sum() ?? 0;

                var selling = _context.SellingReceipts.FirstOrDefault(s => s.Date.Date == date);
                decimal sellingLitres = selling != null
                    ? (input.BenzeneType == "92" ? selling.TotalLitre92 : selling.TotalLitre95)
                    : 0;

                decimal endingBalance = startingBalance + receiving + calibration - sellingLitres;

                var table1 = new ReportTable1
                {
                    day = day,
                    StartingBalanceOfDay = startingBalance,
                    ReceivingOfBuyReceipt = receiving,
                    Calibrations = calibration,
                    SellingInGunCounters = sellingLitres,
                    EndingBalanceOfDay = endingBalance
                };

                decimal diffATG = input.DifferenceInAmountATG.ElementAtOrDefault(day - 1);
                decimal diffBetweenReceiptAndMeasure = diffATG - receiving;
                decimal diffPercentage = receiving != 0 ? (diffBetweenReceiptAndMeasure / receiving) * 100 : 0;
                decimal total = totalWithoutWater - endingBalance;
                decimal totalPercentage = sellingLitres != 0 ? (total / sellingLitres) * 100 : 0;

                var table3 = new ReportTable3
                {
                    day = day,
                    DifferenceInAmountATG = diffATG,
                    DifferenceBetweenReceiptAndMeasure = diffBetweenReceiptAndMeasure,
                    DifferencePercentage = diffPercentage,
                    Total = total,
                    TotalPercentage = totalPercentage
                };

                sellingCumulative += sellingLitres;
                totalCumulative += total;
                decimal cumulativePercentage = sellingCumulative != 0 ? (totalCumulative / sellingCumulative) * 100 : 0;

                var table4 = new ReportTable4
                {
                    day = day,
                    SellingInGunCountersCumulative = sellingCumulative,
                    TotalCumulative = totalCumulative,
                    PercentageCumulative = cumulativePercentage,
                    Valid = Math.Abs(cumulativePercentage) < 0.5m
                };
                report.BenzeneType=input.BenzeneType;

                report.Table1.Add(table1);
                report.Table2.Add(table2);
                report.Table3.Add(table3);
                report.Table4.Add(table4);
            }

            _context.DisabilityAndIncreaseReports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(report);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDisabilityReport(int id)
        {
            var report = await _context.DisabilityAndIncreaseReports.FindAsync(id);

            if (report == null)
            {
                return NotFound(new { message = $"Report with ID {id} not found." });
            }

            _context.DisabilityAndIncreaseReports.Remove(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Report with ID {id} deleted successfully." });
        }


        [HttpGet("by-date")]
        public async Task<IActionResult> GetReportByDate([FromQuery] GetReportByDateInputModel input)
        {
            if (!DateTime.TryParseExact(input.ReportMonth, "yyyy-MM", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var parsedDate))
            {
                return BadRequest(new { message = "Invalid format. Use 'yyyy-MM' (e.g., 2025-01)" });
            }

            var startDate = new DateTime(parsedDate.Year, parsedDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var report = await _context.DisabilityAndIncreaseReports
                .AsNoTracking()
                .Where(r => r.ReportDate >= startDate && r.ReportDate < endDate && r.BenzeneType == input.BenzeneType)
                .Select(r => new 
                {
                    r.Id,
                    r.ReportDate,
                    r.BenzeneType,
                    Table1 = r.Table1.Select(t => new {
                        t.day,
                        t.StartingBalanceOfDay,
                        t.ReceivingOfBuyReceipt,
                        t.Calibrations,
                        t.SellingInGunCounters,
                        t.EndingBalanceOfDay
                    }),
                    Table2 = r.Table2.Select(t => new {
                        t.day,
                        t.TotalAmountInTankATG,
                        t.WaterAmountInCM,
                        t.WaterAmount,
                        t.TotalAmountWithoutWater
                    }),
                    Table3 = r.Table3.Select(t => new {
                        t.day,
                        t.DifferenceInAmountATG,
                        t.DifferenceBetweenReceiptAndMeasure,
                        t.DifferencePercentage,
                        t.Total,
                        t.TotalPercentage
                    }),
                    Table4 = r.Table4.Select(t => new {
                        t.day,
                        t.SellingInGunCountersCumulative,
                        t.TotalCumulative,
                        t.PercentageCumulative,
                        t.Valid
                    })
                })
                .FirstOrDefaultAsync();

            if (report == null)
                return NotFound(new { message = $"No report found for {input.ReportMonth} with BenzeneType {input.BenzeneType}" });

            return Ok(report);
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _context.DisabilityAndIncreaseReports
                .AsNoTracking()
                .Select(r => new 
                {
                    r.Id,
                    r.BenzeneType,
                    r.ReportDate
                })
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();

            return Ok(reports);
        }


    }




        public class ReportInputDto
        {
            public DateTime ReportMonth { get; set; } // Only year and month matter
            public string BenzeneType { get; set; } // "92" or "95"
            public decimal StartingBalanceOfDay { get; set; }
            public List<decimal> TotalAmountInTankATG { get; set; }
            public List<decimal> DifferenceInAmountATG { get; set; }
        }

        public class GetReportByDateInputModel
        {
            public string ReportMonth { get; set; }
            public string BenzeneType { get; set; }
        }


}
