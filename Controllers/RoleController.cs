using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Models;
using SmartInventorySystem.ViewModels;
using System.ComponentModel.DataAnnotations;

[Authorize(Roles = "SuperAdmin")]
public class RoleController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    // GET: /Role/
    public async Task<IActionResult> Index()
    {
        var roles = await _roleManager.Roles
            .Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name!,
                NormalizedName = r.NormalizedName!
            })
            .ToListAsync();

        foreach (var role in roles)
        {
            // Correct:
            var users = await _userManager.GetUsersInRoleAsync(role.Name!);
            role.UserCount = users.Count;
        }

        return View(roles);
    }

    // GET: /Role/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Role/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _roleManager.RoleExistsAsync(model.Name))
        {
            ModelState.AddModelError("Name", "Role name already exists.");
            return View(model);
        }

        var role = new IdentityRole { Name = model.Name };
        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            TempData["Success"] = $"Role '{model.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // GET: /Role/Edit/abc123-role-id
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        // 1. Await the task first
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);

        // 2. Now you can use LINQ on the result (IList<ApplicationUser>)
        var userEmailsOrNames = usersInRole
            .Select(u => u.Email ?? u.UserName!)
            .ToList();

        var model = new RoleEditViewModel
        {
            Id = role.Id,
            Name = role.Name!,
            UsersInRole = userEmailsOrNames
        };

        return View(model);
    }
    // POST: /Role/Edit/abc123-role-id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, RoleEditViewModel model)
    {
        if (id != model.Id) return NotFound();

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Prevent renaming built-in roles (optional safety)
        if (role.Name is "SuperAdmin" or "Admin" or "Staff")
        {
            if (role.Name != model.Name)
            {
                ModelState.AddModelError("Name", "Cannot rename built-in roles.");
                return View(model);
            }
        }

        role.Name = model.Name;
        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            TempData["Success"] = "Role updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // GET: /Role/Delete/abc123-role-id
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        // Safety: prevent deletion of built-in roles
        if (role.Name is "SuperAdmin" or "Admin" or "Staff")
        {
            TempData["Error"] = $"Cannot delete built-in role: {role.Name}";
            return RedirectToAction(nameof(Index));
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
        {
            TempData["Error"] = $"Cannot delete role '{role.Name}' because {usersInRole.Count} user(s) are assigned to it.";
            return RedirectToAction(nameof(Index));
        }

        var model = new RoleViewModel
        {
            Id = role.Id,
            Name = role.Name!,
            UserCount = 0
        };

        return View(model);
    }

    // POST: /Role/DeleteConfirmed
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        var result = await _roleManager.DeleteAsync(role);

        if (result.Succeeded)
        {
            TempData["Success"] = $"Role '{role.Name}' deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to delete role.";
        }

        return RedirectToAction(nameof(Index));
    }
}