using ResaleApi.Models;

namespace ResaleApi.Repositories
{
    public interface ICustomerOrderRepository
    {
        Task<CustomerOrder?> GetByIdAsync(Guid id);
        Task<IEnumerable<CustomerOrder>> GetByResellerIdAsync(Guid resellerId, int page = 1, int pageSize = 10);
        Task<IEnumerable<CustomerOrder>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<CustomerOrder> CreateAsync(CustomerOrder order);
        Task<CustomerOrder> UpdateAsync(CustomerOrder order);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetNextOrderNumberAsync();
        Task<IEnumerable<CustomerOrder>> GetPendingOrdersByResellerAsync(Guid resellerId);
        Task<int> GetTotalCountAsync();
        Task<int> GetTotalCountByResellerAsync(Guid resellerId);
    }
} 