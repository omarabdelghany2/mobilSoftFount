using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace mobileBackendsoftFount.Models
{
    public class BenzeneRecipeProduct
    {
        public int Id { get; set; } // Unique identifier
        public float Amount { get; set; } // Amount of benzene in liters
        public float PricePerLiter { get; set; } // Price per liter
        public float EvaporationPercentage { get; set; } // Percentage of evaporation loss
        public float ValueOfEvaporation { get; set; } // Percentage of evaporation loss

        public float Taxes { get; set; } // Applicable taxes
        public float ValueOfTaxes { get; set; } // Applicable taxes
        public float TotalValue{get; set; }

        // Foreign key to link with BenzeneBuyReceipt
        public int BenzeneBuyReceiptId { get; set; } // <-- ADD THIS PROPERTY

        // Navigation property
        [ForeignKey("BenzeneBuyReceiptId")]
        public BenzeneBuyReceipt BenzeneBuyReceipt { get; set; }
    }
}
