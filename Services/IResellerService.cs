using ResaleApi.DTOs;

namespace ResaleApi.Services
{
    public interface IResellerService
    {
        Task<ResellerDto?> GetByIdAsync(Guid id);
        Task<ResellerDto?> GetByCnpjAsync(string cnpj);
        Task<(IEnumerable<ResellerDto> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10);
        Task<Guid> CreateAsync(CreateResellerCommand command);
        Task<bool> UpdateAsync(Guid id, CreateResellerCommand command);
        Task<bool> DeleteAsync(Guid id);
    }
} 