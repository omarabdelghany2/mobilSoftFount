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
        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateGunCounter(int id, [FromBody] BenzeneGunCounter updatedCounter)
        // {
        //     var existingCounter = await _context.BenzeneGunCounters.FindAsync(id);
        //     if (existingCounter == null)
        //         return NotFound();

        //     existingCounter.StartCount = updatedCounter.StartCount;
        //     existingCounter.EndRoundOneCount = updatedCounter.EndRoundOneCount;
        //     existingCounter.EndRoundTwoCount = updatedCounter.EndRoundTwoCount;
        //     existingCounter.EndRoundThreeCount = updatedCounter.EndRoundThreeCount;
        //     existingCounter.BenzeneType = updatedCounter.BenzeneType;
        //     existingCounter.GunNumber = updatedCounter.GunNumber;

        //     await _context.SaveChangesAsync();
        //     return Ok(existingCounter);
        // }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGunCounter(int id, [FromBody] BenzeneGunRequest updatedCounter)
        {
            if (updatedCounter == null)
            {
                return BadRequest(new { message = "Request body cannot be null." });
            }

            // ✅ Create a list to collect missing fields
            var missingFields = new List<string>();

            if (updatedCounter.EndRoundOneCount == null) missingFields.Add("EndRoundOneCount");
            if (updatedCounter.EndRoundTwoCount == null) missingFields.Add("EndRoundTwoCount");
            if (updatedCounter.EndRoundThreeCount == null) missingFields.Add("EndRoundThreeCount");
            if (string.IsNullOrEmpty(updatedCounter.BenzeneType)) missingFields.Add("BenzeneType");
            if (updatedCounter.GunNumber == null) missingFields.Add("GunNumber");

            // ❌ If any field is missing, return an error message listing them
            if (missingFields.Count > 0)
            {
                return BadRequest(new { message = $"The following fields cannot be null or empty: {string.Join(", ", missingFields)}" });
            }

            // ✅ Check for negative values
            var negativeFields = new List<string>();

            if (updatedCounter.StartCount < 0) negativeFields.Add("StartCount");
            if (updatedCounter.EndRoundOneCount < 0) negativeFields.Add("EndRoundOneCount");
            if (updatedCounter.EndRoundTwoCount < 0) negativeFields.Add("EndRoundTwoCount");
            if (updatedCounter.EndRoundThreeCount < 0) negativeFields.Add("EndRoundThreeCount");

            // ❌ If any field has a negative value, return an error message
            if (negativeFields.Count > 0)
            {
                return BadRequest(new { message = $"The following fields cannot be negative: {string.Join(", ", negativeFields)}" });
            }

            var existingCounter = await _context.BenzeneGunCounters.FindAsync(id);
            if (existingCounter == null)
            {
                return NotFound(new { message = "Gun counter not found." });
            }

            // ✅ Update values safely (we know they are valid due to the checks above)
            existingCounter.StartCount = updatedCounter.StartCount ?? existingCounter.StartCount;
            existingCounter.EndRoundOneCount = updatedCounter.EndRoundOneCount.Value;
            existingCounter.EndRoundTwoCount = updatedCounter.EndRoundTwoCount.Value;
            existingCounter.EndRoundThreeCount = updatedCounter.StartCount ?? existingCounter.StartCount;
            existingCounter.BenzeneType = updatedCounter.BenzeneType;
            existingCounter.GunNumber = updatedCounter.GunNumber.Value;

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



public class BenzeneGunRequest
{
    public int Id { get; set; } 
    public long? StartCount { get; set; }  // Nullable
    public long? EndRoundOneCount { get; set; }  // Nullable
    public long? EndRoundTwoCount { get; set; }  // Nullable
    public long? EndRoundThreeCount { get; set; }  // Nullable
    public string? BenzeneType { get; set; }  // Nullable
    public int? GunNumber { get; set; }  // Nullable
}

}
