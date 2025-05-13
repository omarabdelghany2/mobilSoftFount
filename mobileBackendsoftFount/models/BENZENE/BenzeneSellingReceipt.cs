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
        public List<BenzeneGunCounter> BenzeneGunCounters { get; set; }

        public long TotalLitre92 { get; set; }
        public long TotalLitre95 { get; set; }

        public long TotalMoney92{get; set; }
        public long TotalMoney95{get; set; }

        public long TotalMoney{get; set; }
        // 🔹 New attribute for OpenAmount
        public long OpenAmount92 { get; set; }
        public long OpenAmount95 { get; set; }


    }
}
