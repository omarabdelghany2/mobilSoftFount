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

        public long TotalLiter92 { get; set; }
        public long TotalLiter95 { get; set; }

        public long TotalMoney92{get; set; }
        public long TotalMoney95{get; set; }

        public long TotalMoney{get; set; }

    }
}
