using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class OilTotalySellingReport
    {
 
        public List<OilTotalySellingMember> Memebers { get; set; } = new List<OilTotalySellingMember>();
        public decimal TotalWeight { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalSellingValue { get; set; }
        public decimal TotalProfit { get; set; }
    }

    public class OilTotalySellingMember
    {

        public DateTime Date { get; set; } // <-- New: Date field
        public decimal Weight { get; set; }
        public decimal Value { get; set; }
        public decimal SellingValue { get; set; }
        public decimal Profit{get;set;}
    
    }
}
