using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Models;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using mobileBackendsoftFount.Data;




namespace mobileBackendsoftFount.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OilChangeReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OilChangeReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Generate")]
        public async Task<IActionResult> GenerateOilChangeReport([FromBody] GenerateOilChangeReportInputModel input)
        {
            var startDate = DateTime.SpecifyKind(input.StartDate, DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(input.EndDate, DateTimeKind.Utc);
            var productName = input.ProductName;


            // 1. Fetch the Oil
            var oil = await _context.Oils.FirstOrDefaultAsync(o => o.Name == productName);
            if (oil == null)
                return NotFound($"Oil with name '{productName}' not found.");

            // 2. Fetch BuyReceipts and SellRecipes in the period
            var buyReceipts = await _context.OilBuyReceipts
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .Include(r => r.OilBuyProducts)
                .ToListAsync();

            var sellRecipes = await _context.OilSellRecipes
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .Include(r => r.OilSellProducts)
                .ToListAsync();

            // 3. Calculate initial Balance (LastState)
            var totalBought = buyReceipts
                .SelectMany(r => r.OilBuyProducts)
                .Where(p => p.Name == productName)
                .Sum(p => p.Amount);

            var totalSold = sellRecipes
                .SelectMany(r => r.OilSellProducts)
                .Where(p => p.Name == productName)
                .Sum(p => p.SoldAmount);

            // Find the last sell receipt before the start date
            var lastSellReceipt = await _context.OilSellRecipes
                .Where(r => r.Date.Date < startDate.Date)
                .OrderByDescending(r => r.Date)
                .Include(r => r.OilSellProducts)
                .FirstOrDefaultAsync();

            decimal initialBalance = 0;

            if (lastSellReceipt != null)
            {
                var lastSellProduct = lastSellReceipt.OilSellProducts
                    .FirstOrDefault(p => p.Name == productName);

                if (lastSellProduct != null)
                {
                    initialBalance = lastSellProduct.RoundThreeAmount;
                }
            }

            var products = new List<OilChangeProduct>();

            products.Add(new OilChangeProduct
            {
                Date = startDate,
                MovementType = "Last Balance",
                Balance = initialBalance,
                Comment = ""
            });

            decimal lastBalance = initialBalance;

            // 4. Merge Buy and Sell records by date
            var movements = new List<(DateTime Date, string Type, decimal Amount, int ReceiptId)>();

            foreach (var receipt in buyReceipts)
            {
                var product = receipt.OilBuyProducts.FirstOrDefault(p => p.Name == productName);
                if (product != null)
                    movements.Add((receipt.Date, "Buy", product.Amount, receipt.Id));
            }

            foreach (var receipt in sellRecipes)
            {
                var product = receipt.OilSellProducts.FirstOrDefault(p => p.Name == productName);
                if (product != null && product.SoldAmount > 0)
                    movements.Add((receipt.Date, "Sell", product.SoldAmount, receipt.Id));
            }

            var orderedMovements = movements.OrderBy(m => m.Date).ToList();

            foreach (var movement in orderedMovements)
            {
                var oilChangeProduct = new OilChangeProduct
                {
                    Date = movement.Date,
                    MovementType = movement.Type,
                    Comment = movement.ReceiptId.ToString()
                };

                if (movement.Type == "Buy")
                {
                    oilChangeProduct.IncomeAmount = movement.Amount;
                    oilChangeProduct.Balance = lastBalance + movement.Amount;
                }
                else if (movement.Type == "Sell")
                {
                    oilChangeProduct.ExportAmount = movement.Amount;
                    oilChangeProduct.Balance = lastBalance - movement.Amount;
                }

                products.Add(oilChangeProduct);

                lastBalance = oilChangeProduct.Balance;
            }

            var productDtos = products.Select(p => new OilChangeProductDto
                {
                    Date = p.Date,
                    MovementType = p.MovementType,
                    IncomeAmount = p.IncomeAmount,
                    ExportAmount = p.ExportAmount,
                    Balance = p.Balance,
                    Comment = p.Comment
                }).ToList();


            return Ok(productDtos);

        }

            public class GenerateOilChangeReportInputModel
            {
                public DateTime StartDate { get; set; }
                public DateTime EndDate { get; set; }
                public string ProductName { get; set; } = string.Empty;
            }



            public class OilChangeProductDto
                {
                    public DateTime Date { get; set; }
                    public string MovementType { get; set; }
                    public decimal? IncomeAmount { get; set; }
                    public decimal? ExportAmount { get; set; }
                    public decimal Balance { get; set; }
                    public string Comment { get; set; }
                }



    }
}
