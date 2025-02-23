namespace mobileBackendsoftFount.Models
{
    public class BenzeneGunCounter
    {
        public int Id { get; set; } // Primary Key
        public long StartCount { get; set; }
        public long EndRoundOneCount { get; set; }
        public long EndRoundTwoCount { get; set; }
        public long EndRoundThreeCount { get; set; }
        public string BenzeneType { get; set; } // Benzene name as a string
        public int GunNumber { get; set; } // New field added
        public long TotalSold => EndRoundThreeCount - StartCount;

        
    }
}
