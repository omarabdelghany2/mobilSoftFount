using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/guncounters")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // Only Admins can access these APIs
    public class BenzeneGunCounterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BenzeneGunCounterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Create a new Gun Counter
        [HttpPost]
        public async Task<IActionResult> CreateGunCounter([FromBody] BenzeneGunCounter gunCounter)
        {
            if (gunCounter == null)
                return BadRequest("Invalid data.");

            _context.BenzeneGunCounters.Add(gunCounter);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGunCounterById), new { id = gunCounter.Id }, gunCounter);
        }

        // 🔹 Get a specific Gun Counter by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGunCounterById(int id)
        {
            var gunCounter = await _context.BenzeneGunCounters.FindAsync(id);
            if (gunCounter == null)
                return NotFound();

            return Ok(gunCounter);
        }

        // 🔹 Get all Gun Counters
        [HttpGet]
        public IActionResult GetAllGunCounters()
        {
            return Ok(_context.BenzeneGunCounters.ToList());
        }

        // 🔹 Update an existing Gun Counter
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGunCounter(int id, [FromBody] BenzeneGunCounter updatedCounter)
        {
            var existingCounter = await _context.BenzeneGunCounters.FindAsync(id);
            if (existingCounter == null)
                return NotFound();

            existingCounter.StartCount = updatedCounter.StartCount;
            existingCounter.EndRoundOneCount = updatedCounter.EndRoundOneCount;
            existingCounter.EndRoundTwoCount = updatedCounter.EndRoundTwoCount;
            existingCounter.EndRoundThreeCount = updatedCounter.EndRoundThreeCount;
            existingCounter.BenzeneType = updatedCounter.BenzeneType;
            existingCounter.GunNumber = updatedCounter.GunNumber;

            await _context.SaveChangesAsync();
            return Ok(existingCounter);
        }

        // 🔹 Delete a Gun Counter
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGunCounter(int id)
        {
            var gunCounter = await _context.BenzeneGunCounters.FindAsync(id);
            if (gunCounter == null)
                return NotFound();

            _context.BenzeneGunCounters.Remove(gunCounter);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
