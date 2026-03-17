using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Models;
using SmartInventorySystem.Repositories;
using SmartInventorySystem.Services;
using System.Threading.Tasks;

namespace SmartInventorySystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Staff")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IGenericRepository<Supplier> _supplierRepo;
        private readonly IGenericRepository<Warehouse> _warehouseRepo;

        public ProductController(
            IProductService productService,
            IGenericRepository<Supplier> supplierRepo,
            IGenericRepository<Warehouse> warehouseRepo)
        {
            _productService = productService;
            _supplierRepo = supplierRepo;
            _warehouseRepo = warehouseRepo;
        }

        // GET: /Product/
        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            var products = await _productService.GetPagedAsync(search, page);

            // Keep search term for pagination
            ViewBag.CurrentFilter = search;

            return View(products);
        }

        // GET: /Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetByIdAsync(id.Value);
            if (product == null) return NotFound();

            return View(product);
        }

        // GET: /Product/Create
        // GET: /Product/Create
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            // Pass a new instance to the view so Model.Id exists (as 0)
            return View(new Product());
        }

        // POST: /Product/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "SuperAdmin,Admin")]
        //public async Task<IActionResult> Create(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            await _productService.CreateAsync(product);
        //            TempData["Success"] = "Product created successfully.";
        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (Exception ex)
        //        {
        //            ModelState.AddModelError(string.Empty, $"Error creating product: {ex.Message}");
        //        }
        //    }

        //    await LoadDropdowns();
        //    return View(product);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productService.CreateAsync(product);
                return RedirectToAction(nameof(Index));
            }

            // If we got here, something failed. We MUST reload these or the view will break.
            await LoadDropdowns();
            return View(product);
        }

        // GET: /Product/Edit/5
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetByIdAsync(id.Value);
            if (product == null) return NotFound();

            await LoadDropdowns();
            return View(product);
        }

        // POST: /Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _productService.UpdateAsync(product);
                    TempData["Success"] = "Product updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProductExists(product.Id))
                        return NotFound();
                    ModelState.AddModelError(string.Empty, "Concurrency error - product was modified by another user.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error updating product: {ex.Message}");
                }
            }

            await LoadDropdowns();
            return View(product);
        }

        // GET: /Product/Delete/5
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetByIdAsync(id.Value);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: /Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _productService.DeleteAsync(id);
                TempData["Success"] = "Product deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting product: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to load dropdowns (used in Create & Edit)
        private async Task LoadDropdowns()
        {
            ViewBag.Suppliers = await _supplierRepo.GetAllAsync();
            ViewBag.Warehouses = await _warehouseRepo.GetAllAsync();
        }

        // Helper to check if product still exists (concurrency)
        private async Task<bool> ProductExists(int id)
        {
            return await _productService.GetByIdAsync(id) != null;
        }
    }
}