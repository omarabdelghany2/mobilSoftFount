using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace mobileBackendsoftFount.Controllers{

    [Route("api/oils")]
    [ApiController]
    public class OilController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/oils
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Oil>>> GetOils()
        {
            return await _context.Oils.Include(o => o.Supplier).ToListAsync();
        }

        // GET: api/oils/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Oil>> GetOil(int id)
        {
            var oil = await _context.Oils.Include(o => o.Supplier).FirstOrDefaultAsync(o => o.Id == id);
            if (oil == null) return NotFound();
            return oil;
        }

        // POST: api/oils
        [HttpPost]
        public async Task<IActionResult> CreateOil([FromBody] Oil oil)
        {
            if (oil.SupplierId == 0)
                return BadRequest(new { message = "SupplierId is required." });

            var supplier = await _context.OilSuppliers.FindAsync(oil.SupplierId);
            if (supplier == null)
                return BadRequest(new { message = "Supplier not found." });

            oil.Supplier = supplier; // ✅ Assign the supplier object

            _context.Oils.Add(oil);
            await _context.SaveChangesAsync();

            // 🔹 Corrected method reference
            return CreatedAtAction(nameof(GetOil), new { id = oil.Id }, oil);
        }



        // PUT: api/oils/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOil(int id, Oil oil)
        {
            if (id != oil.Id) return BadRequest("ID mismatch.");

            var existingOil = await _context.Oils.FindAsync(id);
            if (existingOil == null) return NotFound();

            existingOil.Name = oil.Name;
            existingOil.Price = oil.Price;
            existingOil.PriceOfSelling = oil.PriceOfSelling;
            existingOil.Weight = oil.Weight;
            existingOil.Order = oil.Order;
            existingOil.Amount = oil.Amount;
            existingOil.SupplierId = oil.SupplierId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/oils/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOil(int id)
        {
            var oil = await _context.Oils.FindAsync(id);
            if (oil == null) return NotFound();

            _context.Oils.Remove(oil);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}