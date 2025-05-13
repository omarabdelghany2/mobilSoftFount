namespace mobileBackendsoftFount.Models
{

public class ServiceSellReceipt
{
    public int Id { get; set; } // Unique identifier
    public DateTime Date { get; set; } = DateTime.UtcNow; // Default to current date
    public decimal TotalPrice { get; set; }

    // List of service sell products in this receipt
    public List<ServiceSellProduct> ServiceSellProducts { get; set; } = new List<ServiceSellProduct>();
}
    public class ServiceSellProduct
    {
        public int Id { get; set; } // Unique identifier
        public string ClientName { get; set; }
        public string ClientCarModel { get; set; }
        public string ClientCarNumber { get; set; }
        public string? ClientPhone { get; set; } // Make this nullable
        public string Worker { get; set; }
        public decimal Value { get; set; }

        // Foreign key to ServiceSellReceipt
        public int ServiceSellReceiptId { get; set; }

        // Navigation property to ServiceSellReceipt
        public ServiceSellReceipt ServiceSellReceipt { get; set; }

        // List of services provided to the client
        public List<ClientService> ClientServices { get; set; } = new List<ClientService>();
    }


    public class ClientService
    {
        public int Id { get; set; } // Unique identifier
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public decimal SubCategoryPrice { get; set; }
    }








}
