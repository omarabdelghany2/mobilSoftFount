namespace mobileBackendsoftFount.Models
{
    public class OilWorker
    {
        public int Id { get; set; } // Primary Key
        public string Name { get; set; } = string.Empty; // Default empty string
        public string MobileNumber { get; set; } = string.Empty; // Default empty string
        public string NationalID { get; set; } = string.Empty; // Default empty string
    }
}
