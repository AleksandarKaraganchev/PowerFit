namespace PowerFIt.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; } //FK
        public Category Categories { get; set; } //vruska s tablica Category
        public int DosageFormId { get; set; } //FK
        public DosageForm DosageForms { get; set; }//vruska s tablica DosageForm
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string RecommendedFor { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public DateTime RegOn { get; set; }
        public string MeasureUnit { get; set; }
        public ICollection<Order> Orders { get; set; } // 1:M
    }
}
