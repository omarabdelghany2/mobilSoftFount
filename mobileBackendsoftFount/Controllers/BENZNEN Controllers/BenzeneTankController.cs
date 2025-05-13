using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;

namespace mobileBackendsoftFount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BenzeneTankController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BenzeneTankController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST
        [HttpPost]
        public async Task<ActionResult<BenzeneTank>> Create([FromBody] BenzeneTankRequest request)
        {
            var errors = new List<string>();

            if (request.tankOne92ATG <= 0) errors.Add("tankOne92ATG must be a positive number.");
            if (request.tankTwo92ATG <= 0) errors.Add("tankTwo92ATG must be a positive number.");
            if (request.tankOne95ATG <= 0) errors.Add("tankOne95ATG must be a positive number.");
            if (request.date == default) errors.Add("Date must be a valid DateTime.");

            var dateUtc = DateTime.SpecifyKind(request.date.Date, DateTimeKind.Utc);

            bool exists = await _context.BenzeneTanks.AnyAsync(x => x.date.Date == dateUtc.Date);
            if (exists)
                errors.Add("A record already exists for this date.");

            if (errors.Any())
                return BadRequest(new { errors });

            var tank = new BenzeneTank
            {
                tankOne92ATG = request.tankOne92ATG,
                tankTwo92ATG = request.tankTwo92ATG,
                tankOne95ATG = request.tankOne95ATG,
                date = dateUtc
            };

            _context.BenzeneTanks.Add(tank);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = tank.id }, tank);
        }

        // GET by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<BenzeneTank>> GetById(int id)
        {
            var item = await _context.BenzeneTanks.FindAsync(id);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // GET all
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BenzeneTank>>> GetAll()
        {
            return Ok(await _context.BenzeneTanks.ToListAsync());
        }

        // GET by date
        [HttpGet("by-date")]
        public async Task<ActionResult<IEnumerable<BenzeneTank>>> GetByDate([FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest("Invalid date format. Use yyyy-MM-dd.");

            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

            var results = await _context.BenzeneTanks
                .Where(x => x.date.Date == parsedDate.Date)
                .ToListAsync();

            return Ok(results);
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.BenzeneTanks.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.BenzeneTanks.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO
    public class BenzeneTankRequest
    {
        public double tankOne92ATG { get; set; }
        public double tankTwo92ATG { get; set; }
        public double tankOne95ATG { get; set; }
        public DateTime date { get; set; }
    }
}
