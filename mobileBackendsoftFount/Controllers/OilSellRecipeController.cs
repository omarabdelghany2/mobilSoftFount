using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OilSellRecipeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilSellRecipeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/OilSellRecipe
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OilSellRecipe>>> GetOilSellRecipes()
        {
            return await _context.OilSellRecipes.Include(r => r.OilSellProducts).ToListAsync();
        }

        // GET: api/OilSellRecipe/date/{date}
        [HttpGet("date/{date}")]
        public async Task<ActionResult<OilSellRecipe>> GetOilSellRecipeByDate(DateTime date)
        {
            var recipe = await _context.OilSellRecipes
                                    .Include(r => r.OilSellProducts)
                                    .FirstOrDefaultAsync(r => r.Date == date);

            if (recipe == null)
            {
                return NotFound("No OilSellRecipe found for the given date.");
            }

            return recipe;
        }


        // POST: api/OilSellRecipe/CreateByDate
        [HttpPost("CreateByDate")]
        public async Task<ActionResult<OilSellRecipe>> CreateOilSellRecipeByDate([FromBody] DateTime date)
        {
            if (_context.OilSellRecipes.Any(r => r.Date.Date == date.Date))
            {
                return BadRequest("A recipe already exists for this date.");
            }

            var oils = await _context.Oils.Include(o => o.Supplier).ToListAsync();
            var newRecipe = new OilSellRecipe
            {
                Name = $"Oil Sell Recipe {date:yyyy-MM-dd}",
                OilSupplierId = oils.FirstOrDefault()?.SupplierId ?? 0,
                OilSupplier = oils.FirstOrDefault()?.Supplier,
                Date = date,
                OilSellProducts = oils.OrderBy(o => o.Order).Select(o => new OilSellProduct
                {
                    Name = o.Name,
                    Price = (decimal)o.PriceOfSelling,
                    ReceiveAmount = o.Amount,
                    OilSupplierId = o.SupplierId,
                    OilSupplier = o.Supplier,
                    RoundOneAmount = 0,
                    RoundTwoAmount = 0,
                    RoundThreeAmount = 0,
                    BoughtAmount = 0,
                    BoughtRound = 0,
                    SoldAmount = 0
                }).ToList()
            };

            _context.OilSellRecipes.Add(newRecipe);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOilSellRecipe), new { id = newRecipe.Id }, newRecipe);
        }

        // PUT: api/OilSellRecipe/date/{date}
        [HttpPut("date/{date}")]
        public async Task<IActionResult> UpdateOilSellRecipeByDate(DateTime date, OilSellRecipe recipe)
        {
            var existingRecipe = await _context.OilSellRecipes
                                            .Include(r => r.OilSellProducts)
                                            .FirstOrDefaultAsync(r => r.Date == date);

            if (existingRecipe == null)
            {
                return NotFound("OilSellRecipe not found for the given date.");
            }

            // 🔹 Update properties
            existingRecipe.Name = recipe.Name;
            existingRecipe.OilSupplierId = recipe.OilSupplierId;

            foreach (var product in existingRecipe.OilSellProducts)
            {
                var updatedProduct = recipe.OilSellProducts.FirstOrDefault(p => p.Id == product.Id);
                if (updatedProduct != null)
                {
                    product.RoundOneAmount = updatedProduct.RoundOneAmount;
                    product.RoundTwoAmount = updatedProduct.RoundTwoAmount;
                    product.RoundThreeAmount = updatedProduct.RoundThreeAmount;

                    // 🔹 Calculate Sold Amount
                    if (product.BoughtAmount != 0)
                    {
                        if (product.BoughtRound == 1)
                        {
                            product.SoldAmount = product.ReceiveAmount + product.BoughtAmount - product.RoundOneAmount +
                                                (product.RoundOneAmount - product.RoundTwoAmount) +
                                                (product.RoundTwoAmount - product.RoundThreeAmount);
                        }
                        else if (product.BoughtRound == 2)
                        {
                            product.SoldAmount = product.ReceiveAmount - product.RoundOneAmount +
                                                (product.RoundOneAmount + product.BoughtAmount - product.RoundTwoAmount) +
                                                (product.RoundTwoAmount - product.RoundThreeAmount);
                        }
                        else if (product.BoughtRound == 3)
                        {
                            product.SoldAmount = (product.ReceiveAmount - product.RoundOneAmount) +
                                                (product.RoundOneAmount - product.RoundTwoAmount) +
                                                (product.RoundTwoAmount + product.BoughtAmount - product.RoundThreeAmount);
                        }
                    }
                    else
                    {
                        // Normal sell calculation
                        product.SoldAmount = (product.ReceiveAmount - product.RoundOneAmount) +
                                            (product.RoundOneAmount - product.RoundTwoAmount) +
                                            (product.RoundTwoAmount - product.RoundThreeAmount);
                    }
                }
            }

            // 🔹 Recalculate total price
            existingRecipe.TotalPrice = existingRecipe.OilSellProducts.Sum(p => p.SoldAmount * p.Price);

            // 🔹 Check if this date is the most recent one in the database
            var latestRecipeDate = await _context.OilSellRecipes.MaxAsync(r => r.Date);
            if (date >= latestRecipeDate)
            {
                // If the updated recipe has the most recent date, update Oil.Amount
                foreach (var product in existingRecipe.OilSellProducts)
                {
                    var oil = await _context.Oils.FirstOrDefaultAsync(o => o.Name == product.Name);
                    if (oil != null)
                    {
                        oil.Amount = (int)product.RoundThreeAmount; // Update Amount to RoundThreeAmount
                    }
                }
            }

            _context.Entry(existingRecipe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "A concurrency error occurred while updating the recipe.");
            }

            return NoContent();
        }



        // DELETE: api/OilSellRecipe/date/{date}
        [HttpDelete("date/{date}")]
        public async Task<IActionResult> DeleteOilSellRecipeByDate(DateTime date)
        {
            var recipe = await _context.OilSellRecipes.FirstOrDefaultAsync(r => r.Date == date);

            if (recipe == null)
            {
                return NotFound("No OilSellRecipe found for the given date.");
            }

            _context.OilSellRecipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<OilSellRecipe>> GetOilSellRecipe(int id)
        {
            var recipe = await _context.OilSellRecipes.FindAsync(id);
            if (recipe == null) return NotFound();
            return recipe;
}


    }
}
