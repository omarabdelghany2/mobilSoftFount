 using System;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class MinistryOfSupplyLetterMember
    {
        public int Id { get; set; } // Primary Key
        public decimal StartBalance { get; set; } = 0.0m;
        public decimal CurrentBalance { get; set; } = 0.0m;
        public decimal SoldAmount { get; set; } = 0.0m;
        public decimal Total { get; set; } = 0.0m;
        public string Type { get; set; } = string.Empty;
        public decimal IncomeAmount { get; set; } = 0.0m;

        // Foreign key
        public int MinistryOfSupplyLetterId { get; set; }

        // Navigation property
        public MinistryOfSupplyLetter MinistryOfSupplyLetter { get; set; }
    }

    public class MinistryOfSupplyLetter
    {
        public int Id { get; set; } // Primary Key
        public string Introduction { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime MonthlyDate { get; set; } = DateTime.Now;

        public List<MinistryOfSupplyLetterMember> Members { get; set; } = new List<MinistryOfSupplyLetterMember>();
    }
}
