using Microsoft.AspNetCore.Identity;

namespace SmartInventorySystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
