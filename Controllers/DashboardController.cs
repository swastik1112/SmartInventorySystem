using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySystem.Models;
using SmartInventorySystem.Repositories;
using SmartInventorySystem.Services;

namespace SmartInventorySystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly IGenericRepository<Supplier> _supplierRepo;
        private readonly IGenericRepository<Warehouse> _warehouseRepo;
        private readonly IGenericRepository<Sale> _saleRepo;

        public DashboardController(IProductService productService, IGenericRepository<Supplier> supplierRepo,
            IGenericRepository<Warehouse> warehouseRepo, IGenericRepository<Sale> saleRepo)
        {
            _productService = productService; _supplierRepo = supplierRepo;
            _warehouseRepo = warehouseRepo; _saleRepo = saleRepo;
        }

      public async Task<IActionResult> Index()
{
    ViewBag.TotalProducts   = await _productService.GetTotalCountAsync();     // ← fast, no paging
    ViewBag.TotalSuppliers  = await _supplierRepo.CountAsync();
    ViewBag.TotalWarehouses = await _warehouseRepo.CountAsync();
    ViewBag.LowStock        = await _productService.GetLowStockCountAsync();

    // Optional: if you still want search support later, pass search term
    // ViewBag.TotalProducts = await _productService.GetTotalCountAsync(searchTerm);

    // Sales chart data
    var last30Sales = await _saleRepo.GetAllAsync();
    ViewBag.ChartLabels = last30Sales
        .Select(s => s.SaleDate.ToShortDateString())
        .Distinct()
        .ToArray();

    ViewBag.ChartData = last30Sales
        .GroupBy(s => s.SaleDate.Date)
        .Select(g => g.Sum(s => s.Quantity * s.PricePerUnit))
        .ToArray();

    return View();
}
    }
}
