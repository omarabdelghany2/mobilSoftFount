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


        [HttpPost("create")]
        public async Task<IActionResult> CreateBenzene([FromBody] BenzeneCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid data." });
            }

            // 🔹 Check if Name is provided
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Benzene name is required." });
            }

            // 🔹 Ensure Name is Unique
            if (_context.Benzenes.Any(b => b.Name == request.Name))
            {
                return BadRequest(new { message = "Benzene name must be unique." });
            }

            // 🔹 Validate that numerical values are not negative
            if (request.PriceOfLitre < 0 ||
                request.RateOfEvaporation < 0 ||
                request.RateOfTaxes < 0 ||
                request.RateOfVats < 0 ||
                request.PriceOfSelling < 0)
            {
                return BadRequest(new { message = "All numeric values must be non-negative." });
            }

            // 🔹 Create new Benzene entity
            var benzene = new Benzene
            {
                Name = request.Name,
                PriceOfLitre = request.PriceOfLitre ?? 0.0f,  // Default to 0 if null
                RateOfEvaporation = request.RateOfEvaporation ?? 0.0f,
                RateOfTaxes = request.RateOfTaxes ?? 0.0f,
                RateOfVats = request.RateOfVats ?? 0.0f,
                PriceOfSelling = request.PriceOfSelling ?? 0.0f
            };

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
        public async Task<IActionResult> UpdateBenzene(int id, [FromBody] BenzeneUpdateRequest request)
        {
            var benzene = _context.Benzenes.Find(id);
            if (benzene == null) return NotFound(new { message = "Benzene record not found" });

            // 🔹 Validate that Name is provided and not empty
            if (request.Name is null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Benzene name cannot be empty." });
            }

            // 🔹 Validate Name Uniqueness (only if it's different from the current one)
            if (benzene.Name != request.Name && _context.Benzenes.Any(b => b.Name == request.Name && b.Id != id))
            {
                return BadRequest(new { message = "Benzene name must be unique." });
            }

            // 🔹 Validate that numerical values are not negative
            if ((request.PriceOfLitre < 0) ||
                (request.RateOfEvaporation < 0) ||
                (request.RateOfTaxes < 0) ||
                (request.RateOfVats < 0) ||
                (request.PriceOfSelling < 0))
            {
                return BadRequest(new { message = "All numeric values must be non-negative." });
            }

            // 🔹 Update values
            benzene.Name = request.Name; // Now required
            if (request.PriceOfLitre.HasValue) benzene.PriceOfLitre = request.PriceOfLitre.Value;
            if (request.RateOfEvaporation.HasValue) benzene.RateOfEvaporation = request.RateOfEvaporation.Value;
            if (request.RateOfTaxes.HasValue) benzene.RateOfTaxes = request.RateOfTaxes.Value;
            if (request.RateOfVats.HasValue) benzene.RateOfVats = request.RateOfVats.Value;
            if (request.PriceOfSelling.HasValue) benzene.PriceOfSelling = request.PriceOfSelling.Value;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Benzene record updated successfully", benzene });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteBenzene(int id)
        {
            var benzene = await _context.Benzenes.FindAsync(id);
            if (benzene == null) 
                return NotFound(new { message = "Benzene record not found." });

            _context.Benzenes.Remove(benzene);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }



    }


    public class BenzeneCreateRequest
{
    public string? Name { get; set; }  // Nullable, but required validation will be enforced later
    public float? PriceOfLitre { get; set; }
    public float? RateOfEvaporation { get; set; }
    public float? RateOfTaxes { get; set; }
    public float? RateOfVats { get; set; }
    public float? PriceOfSelling { get; set; }
}

public class BenzeneUpdateRequest
{
    public string? Name { get; set; } // Nullable, but cannot be empty in the update method
    public float? PriceOfLitre { get; set; }
    public float? RateOfEvaporation { get; set; }
    public float? RateOfTaxes { get; set; }
    public float? RateOfVats { get; set; }
    public float? PriceOfSelling { get; set; }
}




}
