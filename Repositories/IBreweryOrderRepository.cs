using ResaleApi.Models;

namespace ResaleApi.Repositories
{
    public interface IBreweryOrderRepository
    {
        Task<BreweryOrder?> GetByIdAsync(Guid id);
        Task<IEnumerable<BreweryOrder>> GetByResellerIdAsync(Guid resellerId, int page = 1, int pageSize = 10);
        Task<IEnumerable<BreweryOrder>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<BreweryOrder> CreateAsync(BreweryOrder order);
        Task<BreweryOrder> UpdateAsync(BreweryOrder order);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetNextBreweryOrderNumberAsync();
        Task<IEnumerable<BreweryOrder>> GetPendingOrdersAsync();
        Task<IEnumerable<BreweryOrder>> GetFailedOrdersAsync();
        Task<int> GetTotalCountAsync();
        Task<int> GetTotalCountByResellerAsync(Guid resellerId);
    }
} 