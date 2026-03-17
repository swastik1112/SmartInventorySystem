using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Models;

namespace SmartInventorySystem.Controllers
{
    public class SaleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SaleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sale
        public async Task<IActionResult> Index()
        {
            // Adding .Include(s => s.Product) tells EF to join the Product table
            var sales = await _context.Sales
                .Include(s => s.Product)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return View(sales);
        }

        // GET: Sale/Create
        public IActionResult Create()
        {
            ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
            return View(new Sale { SaleDate = DateTime.Now });
        }

        // POST: Sale/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sale sale)
        {
            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(sale.ProductId);

                // Validation: Check if enough stock exists
                if (product == null || product.Quantity < sale.Quantity)
                {
                    ModelState.AddModelError("Quantity", $"Insufficient stock. Available: {product?.Quantity ?? 0}");
                    ViewBag.Products = new SelectList(_context.Products, "Id", "Name", sale.ProductId);
                    return View(sale);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Save Sale Record
                    _context.Add(sale);

                    // 2. Reduce Inventory
                    product.Quantity -= sale.Quantity;
                    _context.Update(product);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = "Sale completed successfully. Stock updated (-).";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Transaction failed. Please try again.");
                }
            }

            ViewBag.Products = new SelectList(_context.Products, "Id", "Name", sale.ProductId);
            return View(sale);
        }

        // GET: Sale/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.Product) // Required for @Model.Product.Name
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // GET: Sale/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // POST: Sale/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale != null)
            {
                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Sale record voided.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}