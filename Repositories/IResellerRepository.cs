using ResaleApi.Models;

namespace ResaleApi.Repositories
{
    public interface IResellerRepository
    {
        Task<Reseller?> GetByIdAsync(Guid id);
        Task<Reseller?> GetByCnpjAsync(string cnpj);
        Task<Reseller?> GetByEmailAsync(string email);
        Task<IEnumerable<Reseller>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<Reseller> CreateAsync(Reseller reseller);
        Task<Reseller> UpdateAsync(Reseller reseller);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsByCnpjAsync(string cnpj);
        Task<bool> ExistsByEmailAsync(string email);
        Task<int> GetTotalCountAsync();
    }
} 