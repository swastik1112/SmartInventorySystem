using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;        // Required for MemoryStream
namespace SmartInventorySystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Gather statistics
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.LowStockCount = await _context.Products.Where(p => p.Quantity < 10).CountAsync();

            var sales = await _context.Sales.ToListAsync();
            ViewBag.TotalRevenue = sales.Sum(s => s.Quantity * s.PricePerUnit);

            var purchases = await _context.Purchases.ToListAsync();
            ViewBag.TotalExpenditure = purchases.Sum(p => p.Quantity * p.PricePerUnit);

            // Recent Transactions for the report table
            var recentSales = await _context.Sales.Include(s => s.Product)
                                .OrderByDescending(s => s.SaleDate).Take(5).ToListAsync();

            return View(recentSales);
        }

        //// Placeholder for Excel Export (Requires a library like ClosedXML or EPPlus)
        //public IActionResult ExportExcel()
        //{
        //    // Implementation for Excel export would go here
        //    TempData["Info"] = "Excel Export functionality is ready to be integrated.";
        //    return RedirectToAction(nameof(Index));
        //}

        public IActionResult ExportExcel() => View();

        [HttpGet]
        public async Task<IActionResult> DownloadSales()
        {
            var data = await _context.Sales.Include(s => s.Product).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sales Ledger");
                worksheet.Cell(1, 1).Value = "Date";
                worksheet.Cell(1, 2).Value = "Customer";
                worksheet.Cell(1, 3).Value = "Product";
                worksheet.Cell(1, 4).Value = "Qty";
                worksheet.Cell(1, 5).Value = "Total";

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = data[i].SaleDate;
                    worksheet.Cell(i + 2, 2).Value = data[i].CustomerName;
                    worksheet.Cell(i + 2, 3).Value = data[i].Product?.Name;
                    worksheet.Cell(i + 2, 4).Value = data[i].Quantity;
                    worksheet.Cell(i + 2, 5).Value = data[i].Quantity * data[i].PricePerUnit;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Sales_Report_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        [HttpGet] // Ensure this is here
        public async Task<IActionResult> DownloadInventory()
        {
            try
            {
                var data = await _context.Products.ToListAsync(); // Test without Include first

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Inventory");
                    worksheet.Cell(1, 1).Value = "Product Name";
                    worksheet.Cell(1, 2).Value = "Category";
                    worksheet.Cell(1, 3).Value = "Stock";

                    for (int i = 0; i < data.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = data[i].Name;
                        worksheet.Cell(i + 2, 2).Value = data[i].Category?.ToString() ?? "N/A";
                        worksheet.Cell(i + 2, 3).Value = data[i].Quantity;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        // Adding a return here ensures the browser gets the file signal
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Inventory_Report.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                // If it fails, this will show you the error instead of doing nothing
                return Content($"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPurchases()
        {
            var data = await _context.Purchases.Include(p => p.Product).Include(p => p.Supplier).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Purchases");
                worksheet.Cell(1, 1).Value = "Date";
                worksheet.Cell(1, 2).Value = "Supplier";
                worksheet.Cell(1, 3).Value = "Product";
                worksheet.Cell(1, 4).Value = "Total Cost";

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = data[i].PurchaseDate;
                    worksheet.Cell(i + 2, 2).Value = data[i].Supplier?.Name ?? "N/A";
                    worksheet.Cell(i + 2, 3).Value = data[i].Product?.Name ?? "N/A";
                    worksheet.Cell(i + 2, 4).Value = data[i].Quantity * data[i].PricePerUnit;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Purchases.xlsx");
                }
            }
        }

    }
}