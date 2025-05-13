using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
namespace mobileBackendsoftFount.Controllers{

    [Route("api/oils")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class OilController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetOils()
        {
            var oils = await _context.Oils
                .Include(o => o.Supplier)
                .Select(o => new 
                {
                    o.Id,
                    o.Name,
                    o.Price,
                    o.PriceOfSelling,
                    o.Weight,
                    o.Order,
                    o.Amount,
                    o.Enable,
                    Supplier = new 
                    {
                        o.SupplierId,
                        o.Supplier.Name
                    }
                })
                .ToListAsync();

            return Ok(oils);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOil(int id)
        {
            var oil = await _context.Oils
                .Include(o => o.Supplier)
                .Where(o => o.Id == id)
                .Select(o => new 
                {
                    o.Id,
                    o.Name,
                    o.Price,
                    o.PriceOfSelling,
                    o.Weight,
                    o.Order,
                    o.Amount,
                    o.Enable,
                        o.SupplierId,

                })
                .FirstOrDefaultAsync();

            if (oil == null) return NotFound();
            
            return Ok(oil);
        }

    // public async Task<IActionResult> CreateOil([FromBody] Oil oil)
    // {
    //     if (string.IsNullOrWhiteSpace(oil.Name))
    //         return BadRequest(new { message = "Oil name is required." });
            

    //     if (oil.SupplierId == 0)
    //         return BadRequest(new { message = "SupplierId is required." });

    //     var supplier = await _context.OilSuppliers.FindAsync(oil.SupplierId);
    //     if (supplier == null)
    //         return BadRequest(new { message = "Supplier not found." });

    //     oil.Supplier = supplier;

    //     // ✅ Allow setting enable status during creation
    //     _context.Oils.Add(oil);
    //     await _context.SaveChangesAsync();

    //     return CreatedAtAction(nameof(GetOil), new { id = oil.Id }, oil);
    // }


[HttpPost]
public async Task<IActionResult> CreateOil([FromBody] OilRequest request)
{
    if (request == null)
        return BadRequest(new { message = "Invalid request body." });

    if (string.IsNullOrWhiteSpace(request.Name))
        return BadRequest(new { message = "Oil name is required." });

    if (request.SupplierId == null || request.SupplierId == 0)
        return BadRequest(new { message = "SupplierId is required." });

    var supplier = await _context.OilSuppliers.FindAsync(request.SupplierId);
    if (supplier == null)
        return BadRequest(new { message = "Supplier not found." });

    var oil = new Oil
    {
        Name = request.Name,
        Price = request.Price ?? 0.0f,           // Default to 0 if null
        PriceOfSelling = request.PriceOfSelling ?? 0.0f,
        Weight = request.Weight ?? 0,           // Default to 0
        Order = request.Order ?? 0,             // Default to 0
        Amount = request.Amount ?? 0,           // Default to 0
        SupplierId = request.SupplierId ?? 0,   // Default to 0 (should never happen)
        Enable = request.Enable ?? true,        // Default to true if null
        Supplier = supplier
    };

    _context.Oils.Add(oil);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetOil), new { id = oil.Id }, new 
    { 
        message = "Oil created successfully.", 
        oil 
    });
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
            existingOil.Enable = oil.Enable; // ✅ Correct: Matches the property name


            await _context.SaveChangesAsync();
            return Ok();
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





    public class OilRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public float? Price { get; set; }

        [JsonPropertyName("priceOfSelling")]
        public float? PriceOfSelling { get; set; }

        [JsonPropertyName("weight")]
        public int? Weight { get; set; }

        [JsonPropertyName("order")]
        public int? Order { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("supplierId")]
        public int? SupplierId { get; set; }

        [JsonPropertyName("enable")]
        public bool? Enable { get; set; }
    }

}