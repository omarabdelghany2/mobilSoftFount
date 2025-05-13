using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class AccountInvestigationReport
    {
        public int Id { get; set; } // Primary Key
        public List<AccountInvestigationMember> AccountInvestigationMembers { get; set; } = new List<AccountInvestigationMember>();

        public decimal BalanceOfStart { get; set; } = 0.0m;
        public decimal TotalBuyReceiptMoney { get; set; } = 0.0m;
        public decimal TotalDeposit { get; set; } = 0.0m;
        public decimal TotalAdjustmentMoney { get; set; } = 0.0m;
        public decimal Balance{get ;set; } =0.0m;
    }


        public class AccountInvestigationMember
    {
        public int Id { get; set; } // Primary Key

        public DateTime Date { get; set; } = DateTime.Now; // Includes date + hour om creation
        public string name {get;set;}= "";

        public string Type { get; set; } = "buyRecipt"; // Options: "buyRecipt", "deposit", "adjustment"

        public decimal ReciptTotalMoney { get; set; } = 0.0m;
        public decimal Benzene95Litre { get; set; } = 0.0m;
        public decimal Benzene92Litre { get; set; } = 0.0m;

        public decimal DepostMoney { get; set; } = 0.0m; // Only non-zero im Type is "deposit" or "adjustment"

        public decimal EvaporationMoney { get; set; } = 0.0m;
        public decimal VatesMoney { get; set; } = 0.0m;
        public decimal Taxes95 { get; set; } = 0.0m;
        public decimal Taxes92 { get; set; } = 0.0m;

        public string Comment { get; set; } = string.Empty; // Only milled mor "deposit" or "adjustment"

        public decimal Balance { get; set; } = 0.0m; // Calculated based on last balance and type
    }
}
