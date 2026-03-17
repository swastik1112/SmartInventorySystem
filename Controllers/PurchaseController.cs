using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Models;


namespace SmartInventorySystem.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Purchase
        public async Task<IActionResult> Index()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
            return View(purchases);
        }

        // GET: Purchase/Create
        public IActionResult Create()
        {
            ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Name");
            ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
            return View(new Purchase { PurchaseDate = DateTime.Now });
        }

        // POST: Purchase/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Save the Purchase Record
                    _context.Add(purchase);
                    await _context.SaveChangesAsync();

                    // 2. Update Product Inventory (Logic)
                    var product = await _context.Products.FindAsync(purchase.ProductId);
                    if (product != null)
                    {
                        product.Quantity += purchase.Quantity; // Increase stock
                        _context.Update(product);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = "Purchase order processed. Stock updated (+).";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Error processing purchase. Transaction rolled back.");
                }
            }

            ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Name", purchase.SupplierId);
            ViewBag.Products = new SelectList(_context.Products, "Id", "Name", purchase.ProductId);
            return View(purchase);
        }

        // GET: Purchase/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var purchase = await _context.Purchases
                .Include(p => p.Product)   // Required for @Model.Product.Name
                .Include(p => p.Supplier)  // Required for @Model.Supplier.Name
                .FirstOrDefaultAsync(m => m.Id == id);

            if (purchase == null) return NotFound();

            return View(purchase);
        }

        // GET: Purchase/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var purchase = await _context.Purchases
                .Include(p => p.Product)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (purchase == null) return NotFound();

            return View(purchase);
        }

        // POST: Purchase/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase != null)
            {
                // Note: Deleting a purchase record usually shouldn't 
                // automatically reduce stock unless you want to "void" the arrival.
                _context.Purchases.Remove(purchase);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Purchase record removed.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}