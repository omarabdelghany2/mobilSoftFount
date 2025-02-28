using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System.Linq;
using System.Threading.Tasks;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/benzene")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class BenzeneController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BenzeneController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 1️⃣ Create a new Benzene record
        [HttpPost("create")]
        public async Task<IActionResult> CreateBenzene([FromBody] Benzene benzene)
        {

            if (_context.Benzenes.Any(b => b.Name == benzene.Name))
            {
                return BadRequest(new { message = "Benzene name must be unique" });
            }
            _context.Benzenes.Add(benzene);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Benzene record created successfully", benzene });
        }

        // 🔹 2️⃣ Get all Benzene records
        [HttpGet("all")]
        public IActionResult GetAllBenzene()
        {
            var benzenes = _context.Benzenes.ToList();
            return Ok(benzenes);
        }

        // 🔹 3️⃣ Get a single Benzene record by ID
        [HttpGet("{id}")]
        public IActionResult GetBenzeneById(int id)
        {
            var benzene = _context.Benzenes.Find(id);
            if (benzene == null) return NotFound("Benzene record not found");
            return Ok(benzene);
        }

        // 🔹 4️⃣ Update a Benzene record
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBenzene(int id, [FromBody] Benzene updatedBenzene)
        {
            var benzene = _context.Benzenes.Find(id);
            if (benzene == null) return NotFound("Benzene record not found");

            benzene.Name = updatedBenzene.Name;
            benzene.PriceOfLitre = updatedBenzene.PriceOfLitre;
            benzene.RateOfEvaporation = updatedBenzene.RateOfEvaporation;
            benzene.RateOfTaxes = updatedBenzene.RateOfTaxes;
            benzene.RateOfVats = updatedBenzene.RateOfVats;
            benzene.PriceOfSelling = updatedBenzene.PriceOfSelling;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Benzene record updated successfully", benzene });
        }

        // 🔹 5️⃣ Delete a Benzene record
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteBenzene(int id)
        {
            var benzene = _context.Benzenes.Find(id);
            if (benzene == null) return NotFound("Benzene record not found");

            _context.Benzenes.Remove(benzene);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Benzene record deleted successfully" });
        }
    }
}
