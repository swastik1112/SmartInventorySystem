using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySystem.Models;
using SmartInventorySystem.Repositories;
using System.Threading.Tasks;

namespace SmartInventorySystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Staff")]
    public class WarehouseController : Controller
    {
        private readonly IGenericRepository<Warehouse> _warehouseRepo;

        public WarehouseController(IGenericRepository<Warehouse> warehouseRepo)
        {
            _warehouseRepo = warehouseRepo;
        }

        // GET: /Warehouse/
        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseRepo.GetAllAsync();
            return View(warehouses);
        }

        // GET: /Warehouse/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepo.GetByIdAsync(id.Value);
            if (warehouse == null) return NotFound();

            return View(warehouse);
        }

        // GET: /Warehouse/Create
        [Authorize(Roles = "SuperAdmin,Admin")]
        // Inside WarehouseController.cs
        public IActionResult Create()
        {
            // Fix: Pass an initialized model so the view doesn't crash on @Model check
            return View(new Warehouse());
        }

        // POST: /Warehouse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create(Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                await _warehouseRepo.AddAsync(warehouse);
                TempData["Success"] = "Warehouse created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

        // GET: /Warehouse/Edit/5
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepo.GetByIdAsync(id.Value);
            if (warehouse == null) return NotFound();

            return View(warehouse);
        }

        // POST: /Warehouse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int id, Warehouse warehouse)
        {
            if (id != warehouse.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _warehouseRepo.UpdateAsync(warehouse);
                    TempData["Success"] = "Warehouse updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Error updating warehouse.");
                }
            }
            return View(warehouse);
        }

        // GET: /Warehouse/Delete/5
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepo.GetByIdAsync(id.Value);
            if (warehouse == null) return NotFound();

            return View(warehouse);
        }

        // POST: /Warehouse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _warehouseRepo.DeleteAsync(id);
            TempData["Success"] = "Warehouse deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}