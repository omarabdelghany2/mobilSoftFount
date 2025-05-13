using System;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class ProfitReport
    {
        public List<ProfitMember> Members { get; set; } = new();

        public decimal TotalProfitOfBenzeneAndOilAndServices { get; set; } = 0.0m;
        public decimal TotalExpenses { get; set; } = 0.0m;
        public decimal TotalRevenues { get; set; } = 0.0m;
        public decimal NetProfit { get; set; } = 0.0m;

        public decimal OilStock { get; set; } = 0.0m;
        public decimal BenzeneStock { get; set; } = 0.0m;
    }

    public class ProfitMember
    {
        public string Name { get; set; } = string.Empty;
        public decimal SoldAmount { get; set; } = 0.0m;
        public decimal Price { get; set; } = 0.0m;
        public decimal ValueOfSold { get; set; } = 0.0m;
        public decimal Commission { get; set; } = 0.0m;
        public decimal CostOfBuy { get; set; } = 0.0m;
        public decimal Profit { get; set; } = 0.0m;
    }
}
