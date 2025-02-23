using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mobileBackendsoftFount.Models
{
    public class SellingReceipt
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        public DateTime Date { get; set; } // Receipt Date

        // 🔹 List of BenzeneGunCounters
        public List<BenzeneGunCounter> BenzeneGunCounters { get; set; } = new List<BenzeneGunCounter>();

        public long TotalSold92 { get; set; }
        public long TotalSold95 { get; set; }
    }
}
