using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SmartInventorySystem.Models;
using SmartInventorySystem.Repositories;
using X.PagedList;
using ZXing;

namespace SmartInventorySystem.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _repo;
        private readonly IGenericRepository<StockTransaction> _transRepo;
        private readonly IGenericRepository<AuditLog> _auditRepo;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContext;

        public ProductService(IGenericRepository<Product> repo, IGenericRepository<StockTransaction> transRepo,
            IGenericRepository<AuditLog> auditRepo, ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContext)
        {
            _repo = repo; _transRepo = transRepo; _auditRepo = auditRepo; _context = context;
            _userManager = userManager; _httpContext = httpContext;
        }
        // ProductService.cs
        public async Task<int> GetTotalCountAsync(string search = "")
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));
            }

            return await query.CountAsync();
        }

        public async Task<IPagedList<Product>> GetPagedAsync(string search = "", int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Warehouse)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));
            }

            int pageSize = 10;
            int totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Id)               // ← add ordering if not already sorted
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new StaticPagedList<Product>(items, page, pageSize, totalCount);
        }

        public async Task<Product?> GetByIdAsync(int id) => await _context.Products.Include(p => p.Supplier).Include(p => p.Warehouse).FirstOrDefaultAsync(p => p.Id == id);

        public async Task CreateAsync(Product product)
        {
            product.Barcode = GenerateBarcode(product.SKU);
            product.QRCodeBase64 = GenerateQRCode(product.SKU);
            await _repo.AddAsync(product);
            await LogAudit("Create", "Product", product.Id);
        }

        public async Task UpdateStockAsync(int productId, int qty, string type, string reference)
        {
            var product = await _repo.GetByIdAsync(productId);
            if (product == null) return;

            if (type == "Sale" && product.Quantity < qty)
                throw new InvalidOperationException("Insufficient stock!");

            product.Quantity += (type == "Purchase" ? qty : -qty);
            await _repo.UpdateAsync(product);

            await _transRepo.AddAsync(new StockTransaction
            {
                ProductId = productId,
                Type = type,
                Quantity = qty,
                Reference = reference
            });

            await LogAudit(type, "Stock", productId);
        }

        private string GenerateBarcode(string sku)
        {
            var writer = new BarcodeWriterPixelData { Format = BarcodeFormat.CODE_128 };
            var result = writer.Write(sku);
            // Convert to base64 if needed (for simplicity we store text)
            return sku;
        }

        private string GenerateQRCode(string data)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCode = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qr = new PngByteQRCode(qrCode);
            var qrBytes = qr.GetGraphic(20);
            return Convert.ToBase64String(qrBytes);
        }

        private async Task LogAudit(string action, string entity, int entityId)
        {
            var user = await _userManager.GetUserAsync(_httpContext.HttpContext.User);
            await _auditRepo.AddAsync(new AuditLog
            {
                UserId = user?.Id ?? "System",
                Action = action,
                Entity = entity,
                EntityId = entityId
            });
        }

        public async Task<int> GetLowStockCountAsync() => await _context.Products.CountAsync(p => p.Quantity < p.MinStockLevel);
        public async Task UpdateAsync(Product product) => await _repo.UpdateAsync(product);
        public async Task DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}
