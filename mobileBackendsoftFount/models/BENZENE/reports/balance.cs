namespace mobileBackendsoftFount.Models
{
    public class Balance
    {
        public int Id { get; set; } // Primary key
        public decimal BalanceAmount { get; set; }  // Use decimal for large numbers// The actual balance at the time of the transaction
        public DateTime DateTime { get; set; } // Date and time of the balance snapshot
    }
}
