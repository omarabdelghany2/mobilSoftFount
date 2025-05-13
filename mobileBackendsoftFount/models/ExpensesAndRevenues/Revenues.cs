using System;
using System.ComponentModel.DataAnnotations;

namespace mobileBackendsoftFount.Models
{
    public class Revenue
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string BankName { get; set; } = "";

        public int Round { get; set; } = 0;

        public string Comment { get; set; } = "";

        public decimal Value { get; set; } = 0.0m;

        // Foreign Key
        public int RevenueCategoryId { get; set; }

        // Navigation property
        public RevenueCategory RevenueCategory { get; set; }
    }

    public class RevenueCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public ICollection<Revenue> Revenues { get; set; }
    }
}
