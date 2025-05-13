namespace mobileBackendsoftFount.Models
{
    public class oilDeposit
    {
        public int Id { get; set; } // Primary Key
        public int monthlyId { get; set; } = 1; // Default empty string
        public float amount { get; set; } = 0.0f;
        public string comment { get; set; } = "";
         public DateTime date { get; set; } = DateTime.UtcNow;
        // 🔥 Add this:
        public int? OilBalanceId { get; set; } // FK to oilAccountBalance
        public oilAccountBalance OilBalance { get; set; } // Navigation property (optional, good for EF)
    }
}
