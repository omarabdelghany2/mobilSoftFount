using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/getOilBalances")]
    [ApiController]
    public class OilBalanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilBalanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/getOilBalances
        [HttpGet]
        public async Task<IActionResult> GetAllOilBalances()
        {
            try
            {
                // Retrieve all oil balance entries
                var oilBalances = await _context.oilAccountBalances
                                                .OrderByDescending(b => b.DateTime)
                                                .ToListAsync();

                if (oilBalances == null || oilBalances.Count == 0)
                    return NotFound(new { message = "No oil balance entries found." });

                return Ok(new { message = "Oil balance entries retrieved successfully.", oilBalances });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving oil balances.", error = ex.Message });
            }
        }

        // DELETE: api/getOilBalances/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOilBalance(int id)
        {
            try
            {
                var oilBalance = await _context.oilAccountBalances.FindAsync(id);

                if (oilBalance == null)
                    return NotFound(new { message = $"Oil balance with ID {id} not found." });

                _context.oilAccountBalances.Remove(oilBalance);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Oil balance with ID {id} has been deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the oil balance.", error = ex.Message });
            }
        }
    }
}
