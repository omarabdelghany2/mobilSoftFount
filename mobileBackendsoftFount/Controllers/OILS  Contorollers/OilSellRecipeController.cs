

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Data;
using mobileBackendsoftFount.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace mobileBackendsoftFount.Controllers
{
    [Route("api/OilSellRecipe")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔹 Restrict access to Admins only
    public class OilSellRecipeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilSellRecipeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OilSellRecipe>>> GetOilSellRecipes()
        {
            var recipes = await _context.OilSellRecipes
                .Include(r => r.OilSellProducts)
                .ToListAsync();

            foreach (var recipe in recipes)
            {
                recipe.OilSellProducts = recipe.OilSellProducts
                    .Where(p => _context.Oils.Any(o => o.Name == p.Name && o.Enable)) // ✅ Filter disabled oils
                    .ToList();
            }

            return recipes;
        }

        [HttpGet("date/{date}")]
        public async Task<ActionResult<OilSellRecipe>> GetOilSellRecipeByDate(DateTime date)
        {
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            var recipe = await _context.OilSellRecipes
                .Include(r => r.OilSellProducts)
                .ThenInclude(p => p.OilSupplier)
                .FirstOrDefaultAsync(r => r.Date.Date == date.Date);

            if (recipe == null)
            {
                return NotFound("No OilSellRecipe found for the given date.");
            }

            var response = new
            {
                recipe.Id,
                recipe.Name,
                recipe.Date,
                OilSellProducts = recipe.OilSellProducts.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.SoldAmount,
                    SoldPrice = p.SoldAmount * p.Price, // ✅ Include SoldPrice
                    p.ReceiveAmount,
                    p.RoundOneAmount,
                    p.RoundTwoAmount,
                    p.RoundThreeAmount,
                    p.BoughtAmount,
                    p.BoughtRound,
                    p.OilSellRecipeId,
                    p.OilSupplierId,
                    OilSupplier = new
                    {
                        p.OilSupplier.Id,
                        p.OilSupplier.Name
                    }
                }).ToList(),
                TotalPrice = recipe.OilSellProducts.Sum(p => p.SoldAmount * p.Price) // ✅ Total sold price
            };

            return Ok(response);
        }

        [HttpPost("CreateByDate")]
        public async Task<ActionResult<OilSellRecipe>> CreateOilSellRecipeByDate([FromBody] OilSellRecipeRequest request)
        {
            if (request == null || request.Date == default)
            {
                return BadRequest("Invalid date provided.");
            }

            DateTime date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
            if (_context.OilSellRecipes.Any(r => r.Date.Date == date.Date))
            {
                return BadRequest(new { message = "A recipe already exists for this date." });
            }


            var buyReceipt = await _context.OilBuyReceipts
                .Include(r => r.OilBuyProducts)
                .FirstOrDefaultAsync(r => r.Date.Date == date.Date);

            var oils = await _context.Oils
                .Where(o => o.Enable)
                .Include(o => o.Supplier)
                .ToListAsync();

            var newRecipe = new OilSellRecipe
            {
                Name = $"Oil Sell Recipe {date:yyyy-MM-dd}",
                Date = date,
                OilSellProducts = oils.OrderBy(o => o.Order).Select(o =>
                {
                    var matchingBuyProduct = buyReceipt?.OilBuyProducts.FirstOrDefault(bp => bp.Name == o.Name);

                    return new OilSellProduct
                    {
                        Name = o.Name,
                        Price = (decimal)o.PriceOfSelling,
                        ReceiveAmount = o.Amount,
                        OilSupplierId = o.SupplierId,
                        OilSupplier = o.Supplier,
                        RoundOneAmount = 0,
                        RoundTwoAmount = 0,
                        RoundThreeAmount = 0,
                        BoughtAmount = matchingBuyProduct?.Amount ?? 0,
                        BoughtRound = buyReceipt?.Round ?? 0,
                        SoldAmount = 0
                    };
                }).ToList()
            };

            _context.OilSellRecipes.Add(newRecipe);
            await _context.SaveChangesAsync();

            var response = new
            {
                newRecipe.Id,
                newRecipe.Name,
                newRecipe.Date,
                OilSellProducts = newRecipe.OilSellProducts.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.SoldAmount,
                    SoldPrice = p.SoldAmount * p.Price, // ✅ Include SoldPrice
                    p.ReceiveAmount,
                    p.RoundOneAmount,
                    p.RoundTwoAmount,
                    p.RoundThreeAmount,
                    p.BoughtAmount,
                    p.BoughtRound,
                    p.OilSellRecipeId,
                    p.OilSupplierId,
                    OilSupplier = new
                    {
                        p.OilSupplier.Id,
                        p.OilSupplier.Name
                    }
                }).ToList(),
                TotalPrice = 0.0
            };

            return CreatedAtAction(nameof(GetOilSellRecipes), new { id = newRecipe.Id }, response);
        }

        // [HttpPut("date/{date}")]
        // public async Task<IActionResult> UpdateOilSellRecipeByDate(DateTime date, [FromBody] OilSellRecipe recipe)
        // {
        //     date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

        //     var existingRecipe = await _context.OilSellRecipes
        //         .Include(r => r.OilSellProducts)
        //         .FirstOrDefaultAsync(r => r.Date == date);

        //     if (existingRecipe == null)
        //     {
        //         return NotFound("OilSellRecipe not found for the given date.");
        //     }

        //     if (!string.IsNullOrEmpty(recipe.Name))
        //     {
        //         existingRecipe.Name = recipe.Name;
        //     }

        //     foreach (var updatedProduct in recipe.OilSellProducts)
        //     {
        //         var existingProduct = existingRecipe.OilSellProducts.FirstOrDefault(p => p.Id == updatedProduct.Id);

        //         if (existingProduct != null)
        //         {
        //             var oil = await _context.Oils.FirstOrDefaultAsync(o => o.Name == updatedProduct.Name);
        //             if (oil == null || !oil.Enable)
        //             {
        //                 existingRecipe.OilSellProducts.Remove(existingProduct);
        //                 continue;
        //             }

        //             existingProduct.RoundOneAmount = updatedProduct.RoundOneAmount;
        //             existingProduct.RoundTwoAmount = updatedProduct.RoundTwoAmount;
        //             existingProduct.RoundThreeAmount = updatedProduct.RoundThreeAmount;
        //             existingProduct.OilSellRecipeId = existingRecipe.Id;
        //             existingProduct.OilSellRecipe = existingRecipe;
        //             existingProduct.Price = oil.PriceOfSelling > 0 ? (decimal)oil.PriceOfSelling : updatedProduct.Price;
        //             existingProduct.ReceiveAmount = updatedProduct.ReceiveAmount > 0 ? updatedProduct.ReceiveAmount : existingProduct.ReceiveAmount;

        //             existingProduct.SoldAmount = (existingProduct.ReceiveAmount - existingProduct.RoundOneAmount) +
        //                                          (existingProduct.RoundOneAmount - existingProduct.RoundTwoAmount) +
        //                                          (existingProduct.RoundTwoAmount - existingProduct.RoundThreeAmount);
        //         }
        //     }

        //     existingRecipe.TotalPrice = existingRecipe.OilSellProducts.Sum(p => p.SoldAmount * p.Price);

        //     foreach (var product in existingRecipe.OilSellProducts)
        //     {
        //         var oil = await _context.Oils.FirstOrDefaultAsync(o => o.Name == product.Name);
        //         if (oil != null)
        //         {
        //             oil.Amount = (int)product.RoundThreeAmount;
        //         }
        //     }

        //     _context.Entry(existingRecipe).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         return StatusCode(500, "A concurrency error occurred while updating the recipe.");
        //     }

        //     return Ok(existingRecipe);
        // }


        [HttpPut("date/{date}")]
        public async Task<IActionResult> UpdateOilSellRecipeByDate(DateTime date, [FromBody] List<OilSellProductUpdateDto> updatedProducts)
        {
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            var existingRecipe = await _context.OilSellRecipes
                .Include(r => r.OilSellProducts)
                    .ThenInclude(p => p.OilSupplier) // Include supplier
                .FirstOrDefaultAsync(r => r.Date == date);

            if (existingRecipe == null)
            {
                return NotFound(new{message="OilSellRecipe not found for the given date."});
            }

            foreach (var updatedProduct in updatedProducts)
            {
                var existingProduct = existingRecipe.OilSellProducts
                    .FirstOrDefault(p => p.Id == updatedProduct.Id);

                if (existingProduct != null)
                {


                    var oil = await _context.Oils.FirstOrDefaultAsync(o => o.Name == existingProduct.Name);
                    if (oil == null || !oil.Enable)
                    {
                        existingRecipe.OilSellProducts.Remove(existingProduct);
                        continue;
                    }
                    existingProduct.RoundOneAmount = updatedProduct.RoundOneAmount;
                    existingProduct.RoundTwoAmount = updatedProduct.RoundTwoAmount;
                    existingProduct.RoundThreeAmount = updatedProduct.RoundThreeAmount;
                    // existingProduct.BoughtAmount = updatedProduct.BoughtAmount;
                    // existingProduct.BoughtRound-updatedProduct.BoughtRound;


                    if(existingProduct.BoughtAmount != 0){

                            if (existingProduct.BoughtRound == 1)
                            {
                                existingProduct.SoldAmount = existingProduct.ReceiveAmount + existingProduct.BoughtAmount - existingProduct.RoundOneAmount +
                                                    (existingProduct.RoundOneAmount - existingProduct.RoundTwoAmount) +
                                                    (existingProduct.RoundTwoAmount - existingProduct.RoundThreeAmount);
                            }
                            
                            
                            else if (existingProduct.BoughtRound == 2)
                            {
                                existingProduct.SoldAmount = existingProduct.ReceiveAmount - existingProduct.RoundOneAmount +
                                                    (existingProduct.RoundOneAmount + existingProduct.BoughtAmount - existingProduct.RoundTwoAmount) +
                                                    (existingProduct.RoundTwoAmount - existingProduct.RoundThreeAmount);
                            }
                            
                            else if (existingProduct.BoughtRound == 3)
                            {
                                existingProduct.SoldAmount = (existingProduct.ReceiveAmount - existingProduct.RoundOneAmount) +
                                                    (existingProduct.RoundOneAmount - existingProduct.RoundTwoAmount) +
                                                    (existingProduct.RoundTwoAmount + existingProduct.BoughtAmount - existingProduct.RoundThreeAmount);
                            }

                            
                            // Fetch only id & name of the supplier
                            if (existingProduct.OilSupplier == null && existingProduct.OilSupplierId > 0)
                            {
                                var supplier = await _context.OilSuppliers
                                    .Where(s => s.Id == existingProduct.OilSupplierId)
                                    .Select(s => new OilSupplier { Id = s.Id, Name = s.Name }) // Only return ID & Name
                                    .FirstOrDefaultAsync();

                                existingProduct.OilSupplier = supplier;
                            }

                    }
                    else{

                    

                            existingProduct.SoldAmount = (existingProduct.ReceiveAmount - existingProduct.RoundOneAmount) +
                                                        (existingProduct.RoundOneAmount - existingProduct.RoundTwoAmount) +
                                                        (existingProduct.RoundTwoAmount - existingProduct.RoundThreeAmount);

                            // Fetch only id & name of the supplier
                            if (existingProduct.OilSupplier == null && existingProduct.OilSupplierId > 0)
                            {
                                var supplier = await _context.OilSuppliers
                                    .Where(s => s.Id == existingProduct.OilSupplierId)
                                    .Select(s => new OilSupplier { Id = s.Id, Name = s.Name }) // Only return ID & Name
                                    .FirstOrDefaultAsync();

                                existingProduct.OilSupplier = supplier;
                            }
                    }


                    // **Validation for Negative RoundOneAmount (EndRound1)**
                    if (existingProduct.RoundOneAmount < 0)
                    {
                        return BadRequest(new { message = $"RoundOneAmount (EndRound1) cannot be negative for product ID {existingProduct.Name}." });
                    }

                    // **Validation for Negative RoundTwoAmount (EndRound2)**
                    if (existingProduct.RoundTwoAmount < 0)
                    {
                        return BadRequest(new { message = $"RoundTwoAmount (EndRound2) cannot be negative for product ID {existingProduct.Name}." });
                    }

                    // **Validation for Negative RoundThreeAmount (EndRound3)**
                    if (existingProduct.RoundThreeAmount < 0)
                    {
                        return BadRequest(new { message = $"RoundThreeAmount (EndRound3) cannot be negative for product ID {existingProduct.Name}." });
                    }

                    // **Validation for Negative Sold Amount**
                    if (existingProduct.SoldAmount < 0)
                    {
                        return BadRequest(new { message = $"SoldAmount cannot be negative for product ID {existingProduct.Name}." });
                    }

                    
                }
            }

            existingRecipe.TotalPrice = existingRecipe.OilSellProducts.Sum(p => p.SoldAmount * p.Price);



            var latestRecipeDate = await _context.OilSellRecipes.MaxAsync(r => r.Date);
            if (date >= latestRecipeDate){
                foreach (var product in existingRecipe.OilSellProducts)
                {
                    var oil = await _context.Oils.FirstOrDefaultAsync(o => o.Name == product.Name);
                    if (oil != null)
                    {
                        oil.Amount = (int)product.RoundThreeAmount;
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

            // Return only the required supplier details in the response
            var response = new
            {
                existingRecipe.Id,
                existingRecipe.Name,
                existingRecipe.Date,
                OilSellProducts = existingRecipe.OilSellProducts.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.ReceiveAmount,
                    p.RoundOneAmount,
                    p.RoundTwoAmount,
                    p.RoundThreeAmount,
                    p.BoughtAmount,
                    p.BoughtRound,
                    p.SoldAmount,
                    p.SoldPrice,
                    p.OilSellRecipeId,
                    p.OilSupplierId,
                    OilSupplier = p.OilSupplier != null ? new { p.OilSupplier.Id, p.OilSupplier.Name } : null
                }),
                existingRecipe.TotalPrice
            };

            return Ok(response);
        }





        [HttpDelete("date/{date}")]
        public async Task<IActionResult> DeleteOilSellRecipeByDate(DateTime date)
        {
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            var oilSellRecipe = await _context.OilSellRecipes.FirstOrDefaultAsync(r => r.Date == date);

            if (oilSellRecipe == null)
            {
                return NotFound();
            }

            _context.OilSellRecipes.Remove(oilSellRecipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class OilSellRecipeRequest
    {
        public DateTime Date { get; set; }
    }



    public class OilSellProductUpdateDto
    {
        public int Id { get; set; }
        public int RoundOneAmount { get; set; }
        public int RoundTwoAmount { get; set; }
        public int RoundThreeAmount { get; set; }
    }


}
