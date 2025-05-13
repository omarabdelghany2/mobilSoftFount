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
    public class ExpensesGeneralReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpensesGeneralReportController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("report")]
        public async Task<ActionResult<ExpensesGeneralReport>> GetExpensesGeneralReport(DateTime startDate, DateTime endDate)
        {
            // Ensure UTC compatibility
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            // Get expenses in the date range, including category info
            var expenses = await _context.Expenses
                .Include(e => e.ExpenseCategory)
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            // Group by category and create members
            var members = expenses
                .GroupBy(e => e.ExpenseCategory)
                .Select(group => new ExpensesGeneralReportMember
                {
                    Id = group.Key.Id,
                    Name = group.Key.Name,
                    Value = group.Sum(e => e.Value)
                })
                .ToList();

            // Calculate total
            var report = new ExpensesGeneralReport
            {
                Members = members,
                TotalValue = members.Sum(m => m.Value)
            };

            return Ok(report);
        }

    }
}
