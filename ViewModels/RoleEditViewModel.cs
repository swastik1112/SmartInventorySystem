using System.ComponentModel.DataAnnotations;

namespace SmartInventorySystem.ViewModels
{
    public class RoleEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        public List<string> UsersInRole { get; set; } = new List<string>();
    }
}
