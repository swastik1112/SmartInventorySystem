using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySystem.Models;
using SmartInventorySystem.Repositories;
using SmartInventorySystem.Services;

namespace SmartInventorySystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class PurchaseController : Controller
    {
        private readonly IGenericRepository<Purchase> _repo;
        private readonly IProductService _productService;
        private readonly IGenericRepository<Supplier> _supplierRepo;

        public PurchaseController(IGenericRepository<Purchase> repo, IProductService productService, IGenericRepository<Supplier> supplierRepo)
        {
            _repo = repo; _productService = productService; _supplierRepo = supplierRepo;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Purchase purchase)
        {
            await _repo.AddAsync(purchase);
            await _productService.UpdateStockAsync(purchase.ProductId, purchase.Quantity, "Purchase", $"Purchase#{purchase.Id}");
            return RedirectToAction("Index");
        }
    }
}
