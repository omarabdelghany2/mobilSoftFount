using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace mobileBackendsoftFount.Models
{
        public class OilBuyReceipt
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Round { get; set; }
        public int SupplierId { get; set; }  // ✅ Add this property if missing
        public DateTime MonthlyDate { get; set; }
        public int MonthlyBuyIndex { get; set; }
        public List<OilBuyProduct> OilBuyProducts { get; set; } = new List<OilBuyProduct>();
        public decimal TotalValue { get; set; }
    }




}
