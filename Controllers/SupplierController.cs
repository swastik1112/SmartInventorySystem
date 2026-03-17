using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySystem.Models;
using SmartInventorySystem.Repositories;
using System.Threading.Tasks;

namespace SmartInventorySystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Staff")]
    public class SupplierController : Controller
    {
        private readonly IGenericRepository<Supplier> _supplierRepo;

        public SupplierController(IGenericRepository<Supplier> supplierRepo)
        {
            _supplierRepo = supplierRepo;
        }

        // GET: /Supplier/
        public async Task<IActionResult> Index()
        {
            var suppliers = await _supplierRepo.GetAllAsync();
            return View(suppliers);
        }

        // GET: /Supplier/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _supplierRepo.GetByIdAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // GET: /Supplier/Create
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult Create()
        {
            return View(new Supplier());
        }

        // POST: /Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                await _supplierRepo.AddAsync(supplier);
                TempData["Success"] = "Supplier created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: /Supplier/Edit/5
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _supplierRepo.GetByIdAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: /Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _supplierRepo.UpdateAsync(supplier);
                    TempData["Success"] = "Supplier updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Error updating supplier.");
                }
            }
            return View(supplier);
        }

        // GET: /Supplier/Delete/5
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _supplierRepo.GetByIdAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: /Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _supplierRepo.DeleteAsync(id);
            TempData["Success"] = "Supplier deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}