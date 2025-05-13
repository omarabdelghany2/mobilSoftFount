using System;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class ExpensesGeneralReport
    {
        public decimal TotalValue { get; set; }
        public List<ExpensesGeneralReportMember> Members { get; set; } = new();
    }

    public class ExpensesGeneralReportMember
    {
        public int Id { get; set; }         // ExpenseCategory Id
        public string Name { get; set; }    // ExpenseCategory Name
        public decimal Value { get; set; }  // Sum of Expenses in this category
    }


}
