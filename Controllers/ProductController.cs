using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySystem.Models;
using SmartInventorySystem.Repositories;
using SmartInventorySystem.Services;

namespace SmartInventorySystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Staff")]
    public class ProductController : Controller
    {
        private readonly IProductService _service;
        private readonly IGenericRepository<Supplier> _supplierRepo;
        private readonly IGenericRepository<Warehouse> _warehouseRepo;

        public ProductController(IProductService service, IGenericRepository<Supplier> supplierRepo, IGenericRepository<Warehouse> warehouseRepo)
        {
            _service = service; _supplierRepo = supplierRepo; _warehouseRepo = warehouseRepo;
        }

        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            var products = await _service.GetPagedAsync(search, page);
            return View(products);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = await _supplierRepo.GetAllAsync();
            ViewBag.Warehouses = await _warehouseRepo.GetAllAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(product);
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // Edit, Delete, Details – standard CRUD (copy pattern)
    }
}
