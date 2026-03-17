using System.ComponentModel.DataAnnotations;

namespace SmartInventorySystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string QRCodeBase64 { get; set; } = string.Empty;

        [Required][Range(0.01, double.MaxValue)] public decimal Price { get; set; }
        public int Quantity { get; set; } = 0;
        [Required] public int MinStockLevel { get; set; } = 10;

        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}
