using System.ComponentModel.DataAnnotations;

namespace SmartInventorySystem.ViewModels
{
    // ViewModels/CreateUserViewModel.cs
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Roles")]
        public List<string>? SelectedRoles { get; set; }

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
