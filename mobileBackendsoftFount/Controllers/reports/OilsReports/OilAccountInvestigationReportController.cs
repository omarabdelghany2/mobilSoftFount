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

    [Route("api/[controller]")]
    [ApiController]
    public class OilAccountInvestigationReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilAccountInvestigationReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("generate")]
        public async Task<IActionResult> GenerateOilReport(string startDateStr, string endDateStr)
        {
            if (!DateTime.TryParse(startDateStr, out DateTime startDate) ||
                !DateTime.TryParse(endDateStr, out DateTime endDate))
            {
                return BadRequest("Invalid date format. Please use ISO format or valid DateTime format.");
            }

            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            // Get the latest balance *before* the start date
            var startingBalanceEntry = await _context.oilAccountBalances
                .Where(b => b.DateTime < startDate)
                .OrderByDescending(b => b.DateTime)
                .ThenByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            decimal startBalance = Math.Round(startingBalanceEntry?.BalanceAmount ?? 0.0m, 2);
            decimal balance = startBalance;

            var receipts = await _context.OilBuyReceipts
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .ToListAsync();

            var deposits = await _context.OilDeposits
                .Where(d => d.date >= startDate && d.date <= endDate)
                .ToListAsync();

            var adjustments = await _context.OilAdjustments
                .Where(a => a.date >= startDate && a.date <= endDate)
                .ToListAsync();

            var allEntries = new List<(DateTime date, string type, object data)>();
            allEntries.AddRange(receipts.Select(r => (r.Date, "buyReceipt", (object)r)));
            allEntries.AddRange(deposits.Select(d => (d.date, "deposit", (object)d)));
            allEntries.AddRange(adjustments.Select(a => (a.date, "adjustment", (object)a)));

            var sortedEntries = allEntries.OrderBy(e => e.date).ThenBy(e =>
            {
                if (e.type == "buyReceipt") return ((OilBuyReceipt)e.data).Id;
                if (e.type == "deposit") return ((oilDeposit)e.data).Id;
                if (e.type == "adjustment") return ((oilAdjustment)e.data).Id;
                return 0;
            }).ToList();

            var members = new List<OilAccountInvestigationMember>();

            // Add starting balance member
            members.Add(new OilAccountInvestigationMember
            {
                Date = startDate,
                name = "رصيد أول",
                Type = "", // No type
                ReciptTotalMoney = 0,
                DepostMoney = 0,
                Balance = startBalance
            });

            // Track cumulative totals
            decimal totalReceipts = 0.0m;
            decimal totalDeposits = 0.0m;

            foreach (var (date, type, data) in sortedEntries)
            {
                var previousBalance = members.Last().Balance;
                var member = new OilAccountInvestigationMember
                {
                    Date = date
                };

                if (type == "buyReceipt")
                {
                    var receipt = (OilBuyReceipt)data;
                    decimal value = Math.Round(receipt.TotalValue, 2);
                    balance = previousBalance - value;
                    totalReceipts += value;

                    member.Type = "buyRecipt";
                    member.name = $"ف ز {receipt.MonthlyBuyIndex}";
                    member.ReciptTotalMoney = value;
                    member.DepostMoney = 0;
                    member.Balance = balance;
                }
                else if (type == "deposit")
                {
                    var deposit = (oilDeposit)data;
                    decimal amount = Math.Round((decimal)deposit.amount, 2);
                    balance = previousBalance + amount;
                    totalDeposits += amount;

                    member.Type = "deposit";
                    member.name = $"ايداع {deposit.monthlyId}";
                    member.ReciptTotalMoney = 0;
                    member.DepostMoney = amount;
                    member.Balance = balance;
                }
                else if (type == "adjustment")
                {
                    var adj = (oilAdjustment)data;
                    decimal adjAmount = Math.Round((decimal)(adj.increase ? adj.amount : -adj.amount), 2);
                    balance = previousBalance + adjAmount;
                    totalDeposits += adjAmount;

                    member.Type = "adjustment";
                    member.name = $"تسوية {adj.monthlyId}";
                    member.ReciptTotalMoney = 0;
                    member.DepostMoney = adjAmount;
                    member.Balance = balance;
                }

                members.Add(member);
            }

            var report = new OilAccountInvestigationReport
            {
                BalanceOfStart = startBalance,
                TotalBuyReceiptMoney = Math.Round(totalReceipts, 2),
                TotalDeposit = Math.Round(totalDeposits, 2),
                Balance = balance,
                OilAccountInvestigationMembers = members
            };

            return Ok(report);
        }
    }

}

