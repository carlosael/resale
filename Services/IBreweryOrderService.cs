using ResaleApi.DTOs;

namespace ResaleApi.Services
{
    public interface IBreweryOrderService
    {
        Task<BreweryOrderDto?> GetByIdAsync(Guid id);
        Task<(IEnumerable<BreweryOrderDto> Items, int TotalCount)> GetByResellerIdAsync(Guid resellerId, int page = 1, int pageSize = 10);
        Task<(IEnumerable<BreweryOrderDto> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10);
        Task<Guid> CreateAndSendOrderAsync(CreateBreweryOrderCommand command);
        Task<bool> RetryOrderAsync(Guid id);
        Task<bool> UpdateOrderStatusAsync(Guid id, string status);
        Task<IEnumerable<BreweryOrderDto>> GetPendingOrdersAsync();
        Task<IEnumerable<BreweryOrderDto>> GetFailedOrdersAsync();
        Task ProcessPendingOrdersAsync();
    }
} 