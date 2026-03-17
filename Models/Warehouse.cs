using System.ComponentModel.DataAnnotations;

namespace SmartInventorySystem.Models
{
    public class Warehouse
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }
}
