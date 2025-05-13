using Microsoft.AspNetCore.Mvc;
using mobileBackendsoftFount.Models;
using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using BCrypt.Net;


using Microsoft.AspNetCore.Mvc;
using mobileBackendsoftFount.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountInvestigationReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountInvestigationReportController(ApplicationDbContext context)
        {
            _context = context;
        }




        [HttpGet("generate")]
        public async Task<IActionResult> GenerateReport(string type, string dateInput)
        {
            DateTime startDate, endDate;
            Console.WriteLine("Received dateInput: " + dateInput);

            try
            {
                switch (type.ToLower())
                {
                    case "period":
                        var dates = dateInput.Split(" - ");
                        startDate = DateTime.Parse(dates[0]).ToUniversalTime(); // Convert to UTC
                        endDate = DateTime.Parse(dates[1]).ToUniversalTime(); // Convert to UTC
                        break;

                        
                    case "month":
                        try
                        {
                            // Trim to remove any hidden spaces or special characters
                            dateInput = dateInput.Trim();
                            
                            // Log to check the exact value being passed
                            Console.WriteLine("Attempting to parse dateInput: '" + dateInput + "'");

                            // Append '-01' and parse as a date
                            startDate = DateTime.ParseExact(dateInput + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc); // Explicitly set the Kind to UTC

                            endDate = startDate.AddMonths(1).AddDays(-1);  // The endDate will be in UTC since startDate is UTC
                            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc); // Explicitly set the Kind to UTC

                            // Log the results to check if the dates are parsed correctly
                            Console.WriteLine($"Parsed startDate: {startDate}, endDate: {endDate}");
                        }
                        catch (FormatException ex)
                        {
                            // Log the error message if parsing fails
                            Console.WriteLine("Error parsing date: " + ex.Message);
                            return BadRequest("Invalid date input. Use 'yyyy-MM' format for month.");
                        }
                        break;





                case "year":
                    try
                    {
                        // Ensure the dateInput is valid (e.g., "2026")
                        int year = int.Parse(dateInput);
                        
                        // Set startDate to January 1st of the given year at midnight (00:00:00)
                        startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Ensure it's UTC
                        
                        // Set endDate to December 31st of the given year at the last moment (23:59:59)
                        endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc); // Ensure it's UTC
                    }
                    catch (FormatException ex)
                    {
                        // Log error and return a bad request if the year format is incorrect
                        Console.WriteLine("Error parsing year: " + ex.Message);
                        return BadRequest("Invalid year input. Please use a valid year (e.g., '2026').");
                    }
                    break;


                    default:
                        return BadRequest("Invalid type. Use 'period', 'month', or 'year'.");
                }

            }
            catch
            {
                return BadRequest("Invalid date input.");
            }

            var startingBalance = await _context.Balances
                .Where(b => b.DateTime < startDate)
                .OrderByDescending(b => b.DateTime)
                .ThenByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            decimal startBalance = Math.Round((decimal)(startingBalance?.BalanceAmount ?? 0), 2);
            decimal balance = startBalance;

            var receipts = await _context.BenzeneBuyReceipts
                .Include(r => r.Products)
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .ToListAsync();

            var deposits = await _context.BenzeneDeposits
                .Where(d => d.date >= startDate && d.date <= endDate)
                .ToListAsync();

            var adjustments = await _context.BenzeneAdjustments
                .Where(a => a.date >= startDate && a.date <= endDate)
                .ToListAsync();

            var allEntries = new List<(DateTime date, string type, object data)>();
            allEntries.AddRange(receipts.Select(r => (r.Date, "buyRecipt", (object)r)));
            allEntries.AddRange(deposits.Select(d => (d.date, "deposit", (object)d)));
            allEntries.AddRange(adjustments.Select(a => (a.date, "adjustment", (object)a)));

            var sortedEntries = allEntries.OrderBy(e => e.date).ThenBy(e =>
            {
                if (e.type == "buyRecipt") return ((BenzeneBuyReceipt)e.data).Id;
                if (e.type == "deposit") return ((benzeneDeposit)e.data).Id;
                if (e.type == "adjustment") return ((benzeneAdjustment)e.data).Id;
                return 0;
            }).ToList();

            var members = new List<AccountInvestigationMember>();

            int buyReceiptCount = 0;
            int depositCount = 0;
            int adjustmentCount = 0;
            
            foreach (var (date, entryType, data) in sortedEntries)
            {
                if (entryType == "buyRecipt")
                {
                    var receipt = (BenzeneBuyReceipt)data;

                    decimal litre95 = Math.Round(receipt.Products
                        .Where(p => p.ProductName == "95")
                        .Sum(p => (decimal)p.Amount), 2);

                    decimal litre92 = Math.Round(receipt.Products
                        .Where(p => p.ProductName == "92")
                        .Sum(p => (decimal)p.Amount), 2);

                    decimal evaporation = Math.Round(receipt.Products.Sum(p => (decimal)p.ValueOfEvaporation), 2);
                    decimal taxes95 = Math.Round(receipt.Products
                        .Where(p => p.ProductName == "95")
                        .Sum(p => (decimal)p.ValueOfTaxes), 2);

                    decimal taxes92 = Math.Round(receipt.Products
                        .Where(p => p.ProductName == "92")
                        .Sum(p => (decimal)p.ValueOfTaxes), 2);

                    decimal vat = 0; // Adjust if needed

                    balance -= Math.Round((decimal)receipt.TotalValue, 2);

                    buyReceiptCount++;

                    members.Add(new AccountInvestigationMember
                    {
                        Date = receipt.Date,
                        Type = "buyRecipt",
                        name = $"ف ب {buyReceiptCount}",
                        ReciptTotalMoney = Math.Round((decimal)receipt.TotalValue, 2),
                        Benzene95Litre = litre95,
                        Benzene92Litre = litre92,
                        EvaporationMoney = evaporation,
                        VatesMoney = vat,
                        Taxes95 = taxes95,
                        Taxes92 = taxes92,
                        Comment = "",
                        Balance = Math.Round(balance, 2)
                    });
                }
                else if (entryType == "deposit")
                {
                    var deposit = (benzeneDeposit)data;
                    decimal amount = Math.Round((decimal)deposit.amount, 2);
                    balance += amount;

                    depositCount++;

                    members.Add(new AccountInvestigationMember
                    {
                        Date = deposit.date,
                        Type = "deposit",
                        name = $"ايداع {depositCount}",
                        DepostMoney = amount,
                        Comment = deposit.comment,
                        Balance = Math.Round(balance, 2)
                    });
                }
                else if (entryType == "adjustment")
                {
                    var adj = (benzeneAdjustment)data;
                    decimal adjAmount = Math.Round((decimal)(adj.increase ? adj.amount : -adj.amount), 2);
                    balance += adjAmount;

                    adjustmentCount++;

                    members.Add(new AccountInvestigationMember
                    {
                        Date = adj.date,
                        Type = "adjustment",
                        name = $"تسوية {adjustmentCount}",
                        DepostMoney = adjAmount,
                        Comment = adj.comment,
                        Balance = Math.Round(balance, 2)
                    });
                }
            }


            // Set the Result (last balance)
            decimal lastBalance = members.LastOrDefault()?.Balance ?? 0.0m;

            var report = new AccountInvestigationReport
            {
                AccountInvestigationMembers = members,
                BalanceOfStart = startBalance,
                TotalBuyReceiptMoney = Math.Round(receipts.Sum(r => (decimal)r.TotalValue), 2),
                TotalDeposit = Math.Round(deposits.Sum(d => (decimal)d.amount), 2),
                TotalAdjustmentMoney = Math.Round(adjustments.Sum(a => a.increase ? (decimal)a.amount : -(decimal)a.amount), 2),
                Balance = lastBalance  // Set the result to the last balance in the list
            };

            return Ok(report);
        }

    }
}
