
using System;
using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class DisabilityAndIncreaseReport
    {
        public int Id { get; set; } // Primary Key
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;
        public string BenzeneType{get;set;}= "";

        // Navigation properties to each table
        public List<ReportTable1> Table1 { get; set; } = new List<ReportTable1>();
        public List<ReportTable2> Table2 { get; set; } = new List<ReportTable2>();
        public List<ReportTable3> Table3 { get; set; } = new List<ReportTable3>();
        public List<ReportTable4> Table4 { get; set; } = new List<ReportTable4>();
    }

    public class ReportTable1
    {
        public int Id { get; set; }
        public int day { get; set; }
        public decimal StartingBalanceOfDay { get; set; } = 0.0m;
        public decimal ReceivingOfBuyReceipt { get; set; } = 0.0m;
        public decimal Calibrations { get; set; } = 0.0m;
        public decimal SellingInGunCounters { get; set; } = 0.0m;
        public decimal EndingBalanceOfDay { get; set; } = 0.0m;

        // Foreign key + navigation
        public int DisabilityAndIncreaseReportId { get; set; }
        public DisabilityAndIncreaseReport DisabilityAndIncreaseReport { get; set; }
    }

    public class ReportTable2
    {
        public int Id { get; set; }
        public int day { get; set; }
        public decimal TotalAmountInTankATG { get; set; } = 0.0m;
        public decimal WaterAmountInCM { get; set; } = 0.0m;
        public decimal WaterAmount { get; set; } = 0.0m;
        public decimal TotalAmountWithoutWater { get; set; } = 0.0m;

        public int DisabilityAndIncreaseReportId { get; set; }
        public DisabilityAndIncreaseReport DisabilityAndIncreaseReport { get; set; }
    }

    public class ReportTable3
    {
        public int Id { get; set; }
        public int day { get; set; }
        public decimal DifferenceInAmountATG { get; set; } = 0.0m;
        public decimal DifferenceBetweenReceiptAndMeasure { get; set; } = 0.0m;
        public decimal DifferencePercentage { get; set; } = 0.0m;
        public decimal Total { get; set; } = 0.0m;
        public decimal TotalPercentage { get; set; } = 0.0m; // ✅ You had a missing property name here!

        public int DisabilityAndIncreaseReportId { get; set; }
        public DisabilityAndIncreaseReport DisabilityAndIncreaseReport { get; set; }
    }

    public class ReportTable4
    {
        public int Id { get; set; }
        public int day { get; set; }
        public decimal SellingInGunCountersCumulative { get; set; } = 0.0m;
        public decimal TotalCumulative { get; set; } = 0.0m;
        public decimal PercentageCumulative { get; set; } = 0.0m;
        public bool Valid {get;set;}=true; // 👈 Computed property

        public int DisabilityAndIncreaseReportId { get; set; }
        public DisabilityAndIncreaseReport DisabilityAndIncreaseReport { get; set; }
    }
}

