using System;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class TotalDailyRevenue
    {
        public MobilFinance MobilFinance { get; set; } = new();
        public ServiceRevenue Services { get; set; } = new();
        public ExpenseSection Expenses { get; set; } = new();
        public RevenueSection Revenues { get; set; } = new();
        public DailySummaryStatement DailySummary { get; set; } = new();
    }

    public class MobilFinance
    {
        public decimal TotalBenzne92 { get; set; }
        public decimal TotalBenzne95 { get; set; }
        public decimal TotalOils { get; set; }
    }

    public class ServiceRevenue
    {
        public decimal TotalServicesMoney { get; set; }
    }

    public class ExpenseSection
    {
        public decimal TotalExpenses { get; set; }
        public List<CategoryMember> Members { get; set; } = new();
    }

    public class RevenueSection
    {
        public decimal TotalRevenues { get; set; }
        public List<CategoryMember> Members { get; set; } = new();
    }

    public class CategoryMember
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
    }

    public class DailySummaryStatement
    {
        public decimal TotalBenzeneAndOilsMoney { get; set; }
        public decimal TotalServicesMoney { get; set; }
        public decimal GeneralTotal { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetValue { get; set; }
        public decimal TotalRevenues { get; set; }
        public decimal TotalFinalMoney { get; set; }
    }
}
