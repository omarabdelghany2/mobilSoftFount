using System;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class ExpensesAnalyticalReport
    {
        public List<ExpensesAnalyticalReportMember> Members { get; set; } = new();
    }

    public class ExpensesAnalyticalReportMember
    {
        public string Name { get; set; }
        public decimal TotalValue { get; set; } // <-- NEW
        public List<ExpensesAnalyticalEntry> Entries { get; set; } = new();
    }


        public class ExpensesAnalyticalEntry
        {
            public int Id { get; set; } // <-- Add this line
            public DateTime Date { get; set; }
            public string BankName { get; set; }
            public int Round { get; set; }
            public decimal Value { get; set; }
            public string Comment { get; set; }
        }

}
