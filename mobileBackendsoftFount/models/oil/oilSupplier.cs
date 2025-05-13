using System.Collections.Generic;

namespace mobileBackendsoftFount.Models
{
    public class OilSupplier
    {
        public int Id { get; set; } // Primary Key
        public string Name { get; set; } = string.Empty; // Default empty string

        // Navigation Property: One supplier can have many oils
        public List<Oil> Oils { get; set; } = new List<Oil>();
    }
}
