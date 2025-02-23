namespace mobileBackendsoftFount.Models
{
    public class Benzene
    {
        public int Id { get; set; } // Primary Key
        public string Name { get; set; } = string.Empty; // Default empty string
        public float PriceOfLitre { get; set; } = 0.0f;
        public float RateOfEvaporation { get; set; } = 0.0f;
        public float RateOfTaxes { get; set; } = 0.0f;
        public float RateOfVats { get; set; } = 0.0f;
        public float PriceOfSelling { get; set; } = 0.0f;
    }
}
