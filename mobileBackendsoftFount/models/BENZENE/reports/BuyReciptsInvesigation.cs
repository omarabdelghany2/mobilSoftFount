using System;
using System.Collections.Generic;
using System.Linq;

namespace mobileBackendsoftFount.Models
{
    public class BuyReciptsInvesigation
    {
        public List<BuyReciptsInvesigationMember> Members { get; set; } = new List<BuyReciptsInvesigationMember>();

        // Totals at the end of the report
        public decimal TotalBenzene95Litre => Members.Sum(m => m.Benzene95Litre);
        public decimal TotalBenzene92Litre => Members.Sum(m => m.Benzene92Litre);
        public decimal TotalBenzene95Value => Members.Sum(m => m.Benzene95Value);
        public decimal TotalBenzene92Value => Members.Sum(m => m.Benzene92Value);
        public decimal TotalBenzeneAmount => Members.Sum(m => m.BenzeneTotalAmount);
        public decimal TotalBenzeneValue => Members.Sum(m => m.BenzeneTotalValue);
        public decimal TotalEvaporation => Members.Sum(m => m.BenzeneTotalEvaporationValue);
        public decimal TotalTaxes => Members.Sum(m => m.BenzeneTotalTaxesValue);
        public decimal TotalNetValue => Members.Sum(m => m.BuyReciptNetValue);
    }


        
    public class BuyReciptsInvesigationMember
    {
        public int Id { get; set; } // Primary Key

        public DateTime Date { get; set; } = DateTime.Now; // Includes date + hour om creation

        public decimal Benzene95Litre { get; set; } = 0.0m;
        public decimal Benzene92Litre { get; set; } = 0.0m;
        public decimal Benzene95Value{get;set;}=0.0m;
        public decimal Benzene92Value{get;set;}=0.0m;
        public decimal BenzeneTotalAmount{get;set;}=0.0m;
        public decimal BenzeneTotalValue{get;set;}=0.0m;
        public decimal BenzeneTotalEvaporationValue{get;set;}=0.0m;
        public decimal BenzeneTotalTaxesValue{get;set;}=0.0m;
        public decimal BuyReciptNetValue{get;set;}=0.0m;

    }
}
