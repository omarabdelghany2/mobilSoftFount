using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class OilAccountInvestigationReport
    {
        public int Id { get; set; } // Primary Key
        public List<OilAccountInvestigationMember> OilAccountInvestigationMembers { get; set; } = new List<OilAccountInvestigationMember>();

        public decimal BalanceOfStart { get; set; } = 0.0m;
        public decimal TotalBuyReceiptMoney { get; set; } = 0.0m;
        public decimal TotalDeposit { get; set; } = 0.0m;
        public decimal Balance{get ;set; } =0.0m;
    }


        public class OilAccountInvestigationMember
    {
        public int Id { get; set; } // Primary Key

        public DateTime Date { get; set; } = DateTime.Now; // Includes date + hour om creation
        public string name {get;set;}= "";

        public string Type { get; set; } = "buyRecipt"; // Options: "buyRecipt", "deposit",

        public decimal ReciptTotalMoney { get; set; } = 0.0m;

        public decimal DepostMoney { get; set; } = 0.0m; // Only non-zero im Type is "deposit" or "adjustment"

        public decimal Balance { get; set; } = 0.0m; // Calculated based on last balance and type
    }
}
