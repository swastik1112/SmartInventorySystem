using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySystem.Models;
using SmartInventorySystem.Repositories;
using SmartInventorySystem.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartInventorySystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly IGenericRepository<Supplier> _supplierRepo;
        private readonly IGenericRepository<Warehouse> _warehouseRepo;
        private readonly IGenericRepository<Sale> _saleRepo;

        public DashboardController(
            IProductService productService,
            IGenericRepository<Supplier> supplierRepo,
            IGenericRepository<Warehouse> warehouseRepo,
            IGenericRepository<Sale> saleRepo)
        {
            _productService = productService;
            _supplierRepo = supplierRepo;
            _warehouseRepo = warehouseRepo;
            _saleRepo = saleRepo;
        }

        public async Task<IActionResult> Index()
        {
            // Basic counts
            ViewBag.TotalProducts = await _productService.GetTotalCountAsync();
            ViewBag.TotalSuppliers = await _supplierRepo.CountAsync();
            ViewBag.TotalWarehouses = await _warehouseRepo.CountAsync();
            ViewBag.LowStockItems = await _productService.GetLowStockCountAsync();
            ViewBag.OutOfStockItems = await _productService.GetOutOfStockCountAsync();   // ← you should implement this

            // Today's performance
            var today = DateTime.Today;
            ViewBag.TodaySalesCount = await _saleRepo.GetAllAsync()
                .ContinueWith(t => t.Result.Count(s => s.SaleDate.Date == today));

            ViewBag.TodayRevenue = await _saleRepo.GetAllAsync()
                .ContinueWith(t => t.Result
                    .Where(s => s.SaleDate.Date == today)
                    .Sum(s => s.Quantity * s.PricePerUnit));

            // This month revenue
            var firstOfMonth = new DateTime(today.Year, today.Month, 1);
            ViewBag.MonthlyRevenue = await _saleRepo.GetAllAsync()
                .ContinueWith(t => t.Result
                    .Where(s => s.SaleDate >= firstOfMonth)
                    .Sum(s => s.Quantity * s.PricePerUnit));

            // ── Chart: Revenue last 30 days ───────────────────────────────────────
            var thirtyDaysAgo = today.AddDays(-30);
            var last30DaysSales = (await _saleRepo.GetAllAsync())
                .Where(s => s.SaleDate >= thirtyDaysAgo)
                .ToList();

            // Group by date and calculate daily revenue
            var dailyRevenue = last30DaysSales
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(s => s.Quantity * s.PricePerUnit)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Fill missing days with 0
            var result = new List<decimal>();
            var labels = new List<string>();

            for (var date = thirtyDaysAgo.Date; date <= today; date = date.AddDays(1))
            {
                labels.Add(date.ToString("MMM dd"));
                var day = dailyRevenue.FirstOrDefault(x => x.Date == date);
                result.Add(day?.Revenue ?? 0);
            }

            ViewBag.ChartLabels = labels.ToArray();
            ViewBag.ChartData = result.ToArray();

            // ── Optional: Top 5 low stock products for bar chart ──────────────────
            var topLowStock = await _productService.GetTopLowStockProductsAsync(5);

            ViewBag.LowStockProductNames = topLowStock.Select(p => p.Name).ToArray();
            ViewBag.LowStockQuantities = topLowStock.Select(p => p.Quantity).ToArray();
            ViewBag.LowStockMinLevels = topLowStock.Select(p => p.MinStockLevel).ToArray();

            return View();
        }
    }
}