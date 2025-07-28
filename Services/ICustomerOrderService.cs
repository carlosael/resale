using ResaleApi.DTOs;

namespace ResaleApi.Services
{
    public interface ICustomerOrderService
    {
        Task<CustomerOrderDto?> GetByIdAsync(Guid id);
        Task<(IEnumerable<CustomerOrderDto> Items, int TotalCount)> GetByResellerIdAsync(Guid resellerId, int page = 1, int pageSize = 10);
        Task<(IEnumerable<CustomerOrderDto> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10);
        Task<Guid> CreateAsync(CreateCustomerOrderCommand command);
        Task<bool> UpdateStatusAsync(Guid id, string status);
        Task<bool> CancelAsync(Guid id);
        Task<IEnumerable<CustomerOrderDto>> GetPendingOrdersByResellerAsync(Guid resellerId);
    }
} 