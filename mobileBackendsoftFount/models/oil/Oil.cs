using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace mobileBackendsoftFount.Models
{
    [Index(nameof(Name), IsUnique = true)]  // Ensure Name is unique
    [Index(nameof(Order), IsUnique = true)] // Ensure Order is unique
    public class Oil
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public float Price { get; set; } = 0.0f;
        public float PriceOfSelling { get; set; } = 0.0f;
        public int Weight { get; set; } = 0;
        public int Order { get; set; } = 0;
        public int Amount { get; set; } = 0;

        public int SupplierId { get; set; }  // Required
        public OilSupplier? Supplier { get; set; } // Make nullable
        public bool Enable { get; set; } = true; 
    }

}
