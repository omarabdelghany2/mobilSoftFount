using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace mobileBackendsoftFount.Controllers{

    [Route("api/oil-suppliers")]
    [ApiController]
    public class OilSupplierController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilSupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/oil-suppliers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OilSupplier>>> GetSuppliers()
        {
            return await _context.OilSuppliers.ToListAsync();
        }

        // GET: api/oil-suppliers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OilSupplier>> GetSupplier(int id)
        {
            var supplier = await _context.OilSuppliers.FindAsync(id);
            if (supplier == null) return NotFound();
            return supplier;
        }

        // POST: api/oil-suppliers
        [HttpPost]
        public async Task<ActionResult<OilSupplier>> CreateSupplier(OilSupplier supplier)
        {
            if (await _context.OilSuppliers.AnyAsync(s => s.Name == supplier.Name))
                return Conflict("Supplier with the same name already exists.");

            _context.OilSuppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplier);
        }

        // PUT: api/oil-suppliers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, OilSupplier supplier)
        {
            if (id != supplier.Id) return BadRequest("ID mismatch.");

            var existingSupplier = await _context.OilSuppliers.FindAsync(id);
            if (existingSupplier == null) return NotFound();

            existingSupplier.Name = supplier.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/oil-suppliers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.OilSuppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            _context.OilSuppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}