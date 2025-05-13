using System;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
        public class OilBuyReciptsReport
        {
            public List<OilBuyReciptsReportMember> Members { get; set; } = new List<OilBuyReciptsReportMember>();
            public decimal GeneralProfit{get;set;}=0.0m;
        }

        public class OilBuyReciptsReportMember
        {
            public decimal ReciptNumber { get; set; } = 0.0m;
            public DateTime Date { get; set; } = DateTime.Now;
            public decimal TotalMoney { get; set; } = 0.0m;
            public decimal TotalMoneyBeforeTaxes { get; set; } = 0.0m;
            public decimal TotalMoneyOfSell { get; set; } = 0.0m;
            public decimal TotalMoneyOfSellBeforeTaxes { get; set; } = 0.0m;
            public decimal ProfitWithoutTaxes { get; set; }

            public List<BoughtOils> BoughtOilsList { get; set; } = new List<BoughtOils>();
        }

        public class BoughtOils
        {
            public string Name { get; set; } = "";
            public decimal Amount { get; set; } = 0.0m;
            public decimal Price { get; set; } = 0.0m;
            public decimal PriceOfSell { get; set; } = 0.0m;
            public decimal ValueOfPrice { get; set; } = 0.0m;
            public decimal ValueOfPriceBeforeTaxes { get; set; } = 0.0m;
            public decimal ValueOfPriceOfSell { get; set; } = 0.0m;
            public decimal ValueOfPriceOfSellBeforeTaxes { get; set; } = 0.0m;
            public decimal ProfitWithoutTaxes { get; set; } = 0.0m;
        }

}
