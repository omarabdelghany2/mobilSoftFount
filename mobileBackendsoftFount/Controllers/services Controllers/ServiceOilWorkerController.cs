using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/oil-workers")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class OilWorkerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilWorkerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/oil-workers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OilWorker>>> GetWorkers()
        {
            return await _context.OilWorkers.ToListAsync();
        }

        // GET: api/oil-workers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OilWorker>> GetWorker(int id)
        {
            var worker = await _context.OilWorkers.FindAsync(id);
            if (worker == null) return NotFound();
            return worker;
        }

        // POST: api/oil-workers
        [HttpPost]
        public async Task<ActionResult<OilWorker>> CreateWorker([FromBody] OilWorkerRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request data." });

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Name is required." });

            if (string.IsNullOrWhiteSpace(request.MobileNumber))
                return BadRequest(new { message = "Mobile Number is required." });

            if (string.IsNullOrWhiteSpace(request.NationalID))
                return BadRequest(new { message = "National ID is required." });

            if (await _context.OilWorkers.AnyAsync(w => w.NationalID == request.NationalID))
                return Conflict(new { message = "Worker with the same National ID already exists." });

            var worker = new OilWorker
            {
                Name = request.Name,
                MobileNumber = request.MobileNumber,
                NationalID = request.NationalID
            };

            _context.OilWorkers.Add(worker);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWorker), new { id = worker.Id }, worker);
        }


        // DELETE: api/oil-workers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            var worker = await _context.OilWorkers.FindAsync(id);
            if (worker == null) return NotFound();

            _context.OilWorkers.Remove(worker);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }



    public class OilWorkerRequest
{
    public string? Name { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalID { get; set; }
}

}
