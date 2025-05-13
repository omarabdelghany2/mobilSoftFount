using System;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class OilSellProduct
    {
        public int Id { get; set; } // Primary Key
        public string Name { get; set; } = string.Empty; // Product name
        public decimal Price { get; set; } // Price of the product
        public decimal ReceiveAmount { get; set; } // Amount received
        public decimal RoundOneAmount { get; set; } // First round amount
        public decimal RoundTwoAmount { get; set; } // Second round amount
        public decimal RoundThreeAmount { get; set; } // Third round amount
        public decimal BoughtAmount { get; set; } // Total bought amount
        public int BoughtRound { get; set; } // Round in which it was bought
        public decimal SoldAmount { get; set; } // Total sold amount

        // ✅ New computed property for total sold price
        public decimal SoldPrice => Price * SoldAmount; 

        // ✅ Correct navigation properties
        public OilSellRecipe OilSellRecipe { get; set; }
        public int OilSellRecipeId { get; set; }  // Foreign key
        
        // Foreign key relationship
        public int OilSupplierId { get; set; }
        public OilSupplier OilSupplier { get; set; } // Navigation property
    }
}
