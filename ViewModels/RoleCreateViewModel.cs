using System.ComponentModel.DataAnnotations;

namespace SmartInventorySystem.ViewModels
{
    public class RoleCreateViewModel
    {
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Only letters, numbers, underscore and hyphen allowed")]
        public string Name { get; set; } = string.Empty;
    }

  
}
