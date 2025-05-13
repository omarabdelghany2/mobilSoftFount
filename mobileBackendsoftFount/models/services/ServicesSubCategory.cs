


namespace mobileBackendsoftFount.Models
{
    public class SubCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double PriceOfBuy { get; set; }
        public double Price { get; set; }
        // ✅ Many-to-Many Relationship (Keep this if needed)
        public List<Category> Categories { get; set; } = new(); 
    }





}
