using System;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class ServicesSalesReport
    {
        public List<ServicesSalesReportMember> Members { get; set; } = new List<ServicesSalesReportMember>();
    }

    public class ServicesSalesReportMember
    {
        public DateTime Date { get; set; }

        public List<BoughtService> BoughtServices { get; set; } = new List<BoughtService>();

        public decimal TotalAmount { get; set; }           // Sum of Amounts
        public decimal TotalSoldValue { get; set; }        // Sum of SoldValue
        public decimal TotalSoldPrice { get; set; }        // Sum of SoldPrice (avg or total per unit)
        public decimal TotalProfit { get; set; }           // Sum of Profit per item
    }

    public class BoughtService
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }                // Quantity sold
        public decimal SoldValue { get; set; }             // Amount * SoldPrice
        public decimal SoldPrice { get; set; }             // Price per unit
        public decimal Profit { get; set; }                // Profit for this service
    }
}
