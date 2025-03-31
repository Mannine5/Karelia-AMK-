namespace InvoicingApp.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = "";

        public string StreetAddress { get; set; } = "";

        public string PostalCode { get; set; } = "";

        public string City { get; set; } = "";
    }
}