using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using mobileBackendsoftFount.Data; // For ApplicationDbContext
using mobileBackendsoftFount.Models; // For BenzeneCalibration

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BenzeneCalibrationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BenzeneCalibrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<BenzeneCalibration>> Create([FromBody] BenzeneCalibrationRequest request)
        {
            var validationErrors = new List<string>();

            if (request.amount92 < 0) validationErrors.Add("Amount92 must be a positive number.");
            if (request.TotalMoney92 < 0) validationErrors.Add("TotalMoney92 must be a positive number.");
            if (request.amount95 < 0) validationErrors.Add("Amount95 must be a positive number.");
            if (request.TotalMoney95 < 0) validationErrors.Add("TotalMoney95 must be a positive number.");
            if (request.date == default) validationErrors.Add("Date must be a valid DateTime.");

            // Force UTC
            var dateUtc = DateTime.SpecifyKind(request.date.Date, DateTimeKind.Utc);

            // Check for existing record with same date
            bool exists = await _context.BenzeneCalibrations.AnyAsync(x => x.date.Date == dateUtc.Date);
            if (exists)
                validationErrors.Add("A record already exists for this date.");

            if (validationErrors.Any())
                return BadRequest(new { errors = validationErrors });

            var calibration = new BenzeneCalibration
            {
                amount92 = request.amount92,
                TotalMoney92 = request.TotalMoney92,
                amount95 = request.amount95,
                TotalMoney95 = request.TotalMoney95,
                date = dateUtc
            };

            _context.BenzeneCalibrations.Add(calibration);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = calibration.id }, calibration);
        }



        // GET by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<BenzeneCalibration>> GetById(int id)
        {
            var item = await _context.BenzeneCalibrations.FindAsync(id);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // GET all Benzene Calibrations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BenzeneCalibration>>> GetAll()
        {
            return Ok(await _context.BenzeneCalibrations.ToListAsync());
        }

        // DELETE by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.BenzeneCalibrations.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.BenzeneCalibrations.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("by-date")]
        public async Task<ActionResult<IEnumerable<BenzeneCalibration>>> GetByDate([FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest("Invalid date format. Use yyyy-MM-dd.");

            // Ensure DateTime.Kind is UTC
            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

            var results = await _context.BenzeneCalibrations
                .Where(c => c.date.Date == parsedDate.Date)
                .ToListAsync();

            return Ok(results);
        }


    }
        // Request DTO for input validation
        public class BenzeneCalibrationRequest
        {
            public double amount92 { get; set; }
            public double TotalMoney92 { get; set; }
            public double amount95 { get; set; }
            public double TotalMoney95 { get; set; }
            public DateTime date { get; set; }
        }

}





