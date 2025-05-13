using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/getBalances")]
    [ApiController]
    public class BalanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BalanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/getBalances
        [HttpGet]
        public async Task<IActionResult> GetAllBalances()
        {
            try
            {
                // Retrieve all balance entries
                var balances = await _context.Balances
                                              .OrderByDescending(b => b.DateTime) // Sort by DateTime in descending order
                                              .ToListAsync();

                if (balances == null || balances.Count == 0)
                    return NotFound(new { message = "No balance entries found." });

                return Ok(new { message = "Balance entries retrieved successfully", balances });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving balances.", error = ex.Message });
            }
        }

        // DELETE: api/getBalances/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBalance(int id)
        {
            try
            {
                // Find the balance by id
                var balance = await _context.Balances.FindAsync(id);

                if (balance == null)
                    return NotFound(new { message = $"Balance with ID {id} not found." });

                // Remove the balance from the database
                _context.Balances.Remove(balance);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Balance with ID {id} has been deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the balance.", error = ex.Message });
            }
        }
    }
}
