
using System;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class OilBuyProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal PriceOfBuy { get; set; }
        public decimal Weight { get; set; }
        public decimal Value { get; set; }  // ✅ Ensure it has both `get; set;`
        public int SupplierId { get; set; }  // ✅ Add this property if missing

    }


}
