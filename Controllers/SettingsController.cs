using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartInventorySystem.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            // In a real app, you'd load user-specific settings from a database here
            return View();
        }

        [HttpPost]
        public IActionResult UpdatePreference(string theme, bool emailNotifications)
        {
            // Logic to save preferences to cookie or database
            TempData["Success"] = "Preferences updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
