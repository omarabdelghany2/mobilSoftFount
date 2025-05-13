using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class OilStorageBalanceReport
    {
        [Key]
        public int Id { get; set; } // Added Id for EF Core primary key

        public DateTime Date { get; set; } // <-- New: Date field

        public List<OilBalanceProduct> Products { get; set; } = new List<OilBalanceProduct>();

        public decimal TotalBalance { get; set; }
        public decimal StoragePrice { get; set; }
        public decimal StoragePriceOfSell { get; set; }
        public decimal Profit { get; set; }
    }

    public class OilBalanceProduct
    {
        [Key]
        public int Id { get; set; } // Added Id if you want EF to track products (optional)

        public string SupplierName { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal PriceOfSell { get; set; }
        public decimal Amount { get; set; }

        // Add this ForeignKey property
        public int OilStorageBalanceReportId { get; set; } 

        [ForeignKey(nameof(OilStorageBalanceReportId))]
        public OilStorageBalanceReport OilStorageBalanceReport { get; set; }
        
    }
}
