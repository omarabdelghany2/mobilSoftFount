using System;

namespace mobileBackendsoftFount.Models
{
    public class BenzeneBuyReceipt
    {
        public int Id { get; set; }
        public int IncrementalId { get; set; } // Incremental ID per month
        public int MobilId { get; set; }
        public DateTime MobilReceiptDate { get; set; }
        public DateTime Date { get; set; } // Month of receipt (yy/MM)

        // Navigation property for related products
        public List<BenzeneRecipeProduct> Products { get; set; } = new List<BenzeneRecipeProduct>();
        public float TotalValue { get; set; }
    }
}
