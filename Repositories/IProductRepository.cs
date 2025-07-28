using ResaleApi.Models;

namespace ResaleApi.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<Product>> GetActiveAsync(int page = 1, int pageSize = 10);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Product>> GetByBrandAsync(string brand);
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    }
} 