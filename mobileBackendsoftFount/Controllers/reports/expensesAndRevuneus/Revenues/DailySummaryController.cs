using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using System.Globalization;


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mobileBackendsoftFount.Data;
namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DailySummaryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DailySummaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDailySummary(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedDate))
            {
                return BadRequest("Invalid date format. Use yyyy/MM/dd");
            }

            var sellingReceipt = await _context.SellingReceipts
                .FirstOrDefaultAsync(r => r.Date.Date == parsedDate.Date);

            var oilSellRecipe = await _context.OilSellRecipes
                .FirstOrDefaultAsync(r => r.Date.Date == parsedDate.Date);

            var serviceReceipt = await _context.ServiceSellReceipts
                .FirstOrDefaultAsync(r => r.Date.Date == parsedDate.Date);

            var expenses = await _context.Expenses
                .Include(e => e.ExpenseCategory)
                .Where(e => e.Date.Date == parsedDate.Date)
                .ToListAsync();

            var revenues = await _context.Revenues
                .Include(r => r.RevenueCategory)
                .Where(r => r.Date.Date == parsedDate.Date)
                .ToListAsync();

            // --- Mobil Finance ---
            var mobilFinance = new
            {
                TotalBenzene92 = sellingReceipt?.TotalMoney92 ?? 0,
                TotalBenzene95 = sellingReceipt?.TotalMoney95 ?? 0,
                TotalOils = oilSellRecipe?.TotalPrice ?? 0
            };

            // --- Service Revenue ---
            var serviceRevenue = new
            {
                TotalServicesMoney = serviceReceipt?.TotalPrice ?? 0
            };

            // --- Expense Section ---
            var expenseMembers = expenses.Select(e => new CategoryMember
            {
                Name = e.ExpenseCategory.Name,
                Value = e.Value
            }).ToList();

            var totalExpenses = expenses.Sum(e => e.Value);

            // --- Revenue Section ---
            var revenueMembers = revenues.Select(r => new CategoryMember
            {
                Name = r.RevenueCategory.Name,
                Value = r.Value
            }).ToList();

            var totalRevenues = revenues.Sum(r => r.Value);

            // --- Daily Summary Statement ---
            var totalBenzeneAndOils = mobilFinance.TotalBenzene92 + mobilFinance.TotalBenzene95 + mobilFinance.TotalOils;
            var totalServices = serviceRevenue.TotalServicesMoney;
            var generalTotal = totalBenzeneAndOils + totalServices;
            var netValue = generalTotal - totalExpenses;
            var totalFinalMoney = netValue + totalRevenues;

            var totalDailyRevenue = new TotalDailyRevenue
            {
                MobilFinance = new MobilFinance
                {
                    TotalBenzne92 = mobilFinance.TotalBenzene92,
                    TotalBenzne95 = mobilFinance.TotalBenzene95,
                    TotalOils = mobilFinance.TotalOils
                },
                Services = new ServiceRevenue
                {
                    TotalServicesMoney = serviceRevenue.TotalServicesMoney
                },
                Expenses = new ExpenseSection
                {
                    TotalExpenses = totalExpenses,
                    Members = expenseMembers
                },
                Revenues = new RevenueSection
                {
                    TotalRevenues = totalRevenues,
                    Members = revenueMembers
                },
                DailySummary = new DailySummaryStatement
                {
                    TotalBenzeneAndOilsMoney = totalBenzeneAndOils,
                    TotalServicesMoney = totalServices,
                    GeneralTotal = generalTotal,
                    TotalExpenses = totalExpenses,
                    NetValue = netValue,
                    TotalRevenues = totalRevenues,
                    TotalFinalMoney = totalFinalMoney
                }
            };

            return Ok(totalDailyRevenue);
        }

    }
}
