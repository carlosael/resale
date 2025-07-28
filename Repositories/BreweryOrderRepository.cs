using Microsoft.EntityFrameworkCore;
using ResaleApi.Data;
using ResaleApi.Models;

namespace ResaleApi.Repositories
{
    public class BreweryOrderRepository : IBreweryOrderRepository
    {
        private readonly AppDbContext _context;

        public BreweryOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BreweryOrder?> GetByIdAsync(Guid id)
        {
            return await _context.BreweryOrders
                .Include(o => o.Reseller)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<BreweryOrder>> GetByResellerIdAsync(Guid resellerId, int page = 1, int pageSize = 10)
        {
            return await _context.BreweryOrders
                .Where(o => o.ResellerId == resellerId)
                .Include(o => o.Reseller)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<BreweryOrder>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.BreweryOrders
                .Include(o => o.Reseller)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<BreweryOrder> CreateAsync(BreweryOrder order)
        {
            _context.BreweryOrders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<BreweryOrder> UpdateAsync(BreweryOrder order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.BreweryOrders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var order = await _context.BreweryOrders.FindAsync(id);
            if (order == null)
                return false;

            _context.BreweryOrders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetNextBreweryOrderNumberAsync()
        {
            var lastOrder = await _context.BreweryOrders
                .OrderByDescending(o => o.BreweryOrderNumber)
                .FirstOrDefaultAsync();

            return lastOrder?.BreweryOrderNumber + 1 ?? 1000;
        }

        public async Task<IEnumerable<BreweryOrder>> GetPendingOrdersAsync()
        {
            return await _context.BreweryOrders
                .Where(o => o.Status == "Pending" || o.Status == "Failed")
                .Include(o => o.Reseller)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BreweryOrder>> GetFailedOrdersAsync()
        {
            return await _context.BreweryOrders
                .Where(o => o.Status == "Failed")
                .Include(o => o.Reseller)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.UpdatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.BreweryOrders.CountAsync();
        }

        public async Task<int> GetTotalCountByResellerAsync(Guid resellerId)
        {
            return await _context.BreweryOrders.CountAsync(o => o.ResellerId == resellerId);
        }
    }
} 