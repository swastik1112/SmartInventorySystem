using SmartInventorySystem.Models;
using System.Threading.Tasks;

namespace SmartInventorySystem.Services
{
    public interface IProductService
    {
        Task<X.PagedList.IPagedList<Product>> GetPagedAsync(string search = "", int page = 1);
        Task<Product?> GetByIdAsync(int id);
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task UpdateStockAsync(int productId, int qty, string type, string reference);
        Task<int> GetLowStockCountAsync();
        // IProductService.cs
        Task<int> GetTotalCountAsync(string search = "");

        Task<int> GetOutOfStockCountAsync();               // new
        Task<IEnumerable<Product>> GetTopLowStockProductsAsync(int count);
    }
}
