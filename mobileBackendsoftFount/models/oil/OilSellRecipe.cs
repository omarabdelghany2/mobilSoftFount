using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace mobileBackendsoftFount.Models
{
    public class OilSellRecipe
    {
        public int Id { get; set; } // Primary Key
        public string Name { get; set; } = string.Empty; // Recipe Name
        // public int OilSupplierId { get; set; } // Supplier ID
        // public OilSupplier OilSupplier { get; set; } // Navigation Property

        [Required]
        public DateTime Date { get; set; } // 🔹 Added Date field

        // List of OilSellProduct populated from Oil table
        public List<OilSellProduct> OilSellProducts { get; set; } = new List<OilSellProduct>();

        // Total price calculated as sum of (SoldAmount * Price) for each product
        // 🔹 Add this property if missing
        public decimal TotalPrice { get; set; } = 0.0m; 

        public void PopulateFromOils(List<Oil> oils)
        {
            OilSellProducts = oils
                .OrderBy(o => o.Order) // Order by Oil.Order property
                .Select(o => new OilSellProduct
                {
                    Name = o.Name,
                    Price = (decimal)o.PriceOfSelling,
                    ReceiveAmount = o.Amount,
                    OilSupplierId = o.SupplierId,
                    OilSupplier = o.Supplier
                }).ToList();
        }
    }
}
