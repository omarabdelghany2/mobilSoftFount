using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace mobileBackendsoftFount.Models
{
    public class OilChangeReport
    {
        public int Id { get; set; }
        public DateTime ReportDate { get; set; } // maybe monthly or for a specific day?
        public List<OilChangeProduct> Products { get; set; } = new List<OilChangeProduct>();
    }
    public class OilChangeProduct
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string MovementType { get; set; } // e.g., "Income", "Export"
        public decimal IncomeAmount { get; set; }
        public decimal ExportAmount { get; set; }
        public decimal Balance { get; set; }
        public string Comment { get; set; }

        // Foreign Key
        public int OilChangeReportId { get; set; }
        public OilChangeReport OilChangeReport { get; set; }
    }


}
