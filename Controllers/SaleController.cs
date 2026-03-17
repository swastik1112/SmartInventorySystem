using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySystem.Models;
using SmartInventorySystem.Repositories;
using SmartInventorySystem.Services;

namespace SmartInventorySystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Staff")]
    public class SaleController : Controller
    {
        private readonly IGenericRepository<Sale> _repo;
        private readonly IProductService _productService;

        [HttpPost]
        public async Task<IActionResult> Create(Sale sale)
        {
            await _productService.UpdateStockAsync(sale.ProductId, sale.Quantity, "Sale", $"Sale#{sale.Id}");
            await _repo.AddAsync(sale);
            return RedirectToAction("Index");
        }
    }
}
