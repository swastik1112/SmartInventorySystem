namespace SmartInventorySystem.Models
{
    public class StockTransaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string Type { get; set; } = string.Empty; // "Purchase" or "Sale"
        public int Quantity { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Reference { get; set; } = string.Empty;
    }
}
