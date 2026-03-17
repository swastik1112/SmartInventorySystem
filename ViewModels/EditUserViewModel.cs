using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SmartInventorySystem.ViewModels
{
    public class EditUserViewModel
    {
        [HiddenInput]
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Roles")]
        public List<string>? SelectedRoles { get; set; }

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
