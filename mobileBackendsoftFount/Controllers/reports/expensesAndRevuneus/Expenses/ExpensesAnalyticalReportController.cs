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
    public class ExpensesAnalyticalReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpensesAnalyticalReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("report")]
        public async Task<ActionResult<ExpensesAnalyticalReport>> GetReport(DateTime startDate, DateTime endDate)
        {
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            // Load all expenses with categories in range
            var expenses = await _context.Expenses
                .Include(e => e.ExpenseCategory)
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .OrderBy(e => e.Date)
                .ToListAsync();

            var groupedExpenses = expenses
                .GroupBy(e => e.ExpenseCategory.Name);

            var report = new ExpensesAnalyticalReport();

            foreach (var group in groupedExpenses)
            {
                var member = new ExpensesAnalyticalReportMember
                {
                    Name = group.Key,
                    Entries = group.Select(e => new ExpensesAnalyticalEntry
                    {
                        Id = e.Id, // <-- Add this line
                        Date = e.Date,
                        BankName = e.BankName,
                        Round = e.Round,
                        Value = e.Value,
                        Comment = e.Comment
                    }).ToList()

                };
                member.TotalValue = member.Entries.Sum(e => e.Value);


                report.Members.Add(member);
            }

            return Ok(report);
        }


    }
}
