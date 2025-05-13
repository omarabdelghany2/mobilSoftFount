using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class OilAnalyticalSellingReport
    {
 
        public List<OilAnalyticalSellingMember> Memebers { get; set; } = new List<OilAnalyticalSellingMember>();
        public decimal TotalAmount { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalSellingValue { get; set; }
        public decimal TotalProfit { get; set; }
    }

    public class OilAnalyticalSellingMember
    {

        public string SupplierName{get;set;}
        public string Name { get; set; }
        public decimal Amount{get;set;}
        public decimal Weight { get; set; }
        public decimal Value { get; set; }
        public decimal SellingValue { get; set; }
        public decimal Profit{get;set;}
    
    }
}