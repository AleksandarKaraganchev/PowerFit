namespace PowerFIt.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int ProductId { get; set; } //FK
        public string CustomerId { get; set; } //FK
        public int Quantity { get; set; }
        public string Description { get; set; }
        public DateTime OrderDate { get; set; }
        public Product Products { get; set; }//vruska s tablica Product
        public Customer Customers { get; set; }//vruska s tablica Customers
    }
}
