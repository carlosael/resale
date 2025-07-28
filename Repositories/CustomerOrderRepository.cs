using Microsoft.EntityFrameworkCore;
using ResaleApi.Data;
using ResaleApi.Models;

namespace ResaleApi.Repositories
{
    public class CustomerOrderRepository : ICustomerOrderRepository
    {
        private readonly AppDbContext _context;

        public CustomerOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerOrder?> GetByIdAsync(Guid id)
        {
            return await _context.CustomerOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Reseller)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<CustomerOrder>> GetByResellerIdAsync(Guid resellerId, int page = 1, int pageSize = 10)
        {
            return await _context.CustomerOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Reseller)
                .Where(o => o.ResellerId == resellerId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerOrder>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.CustomerOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Reseller)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<CustomerOrder> CreateAsync(CustomerOrder order)
        {
            _context.CustomerOrders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<CustomerOrder> UpdateAsync(CustomerOrder order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.CustomerOrders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var order = await _context.CustomerOrders.FindAsync(id);
            if (order == null)
                return false;

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetNextOrderNumberAsync()
        {
            var lastOrder = await _context.CustomerOrders
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            return lastOrder?.OrderNumber + 1 ?? 1;
        }

        public async Task<IEnumerable<CustomerOrder>> GetPendingOrdersByResellerAsync(Guid resellerId)
        {
            return await _context.CustomerOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.ResellerId == resellerId && o.Status == "Pending")
                .OrderBy(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.CustomerOrders.CountAsync();
        }

        public async Task<int> GetTotalCountByResellerAsync(Guid resellerId)
        {
            return await _context.CustomerOrders.CountAsync(o => o.ResellerId == resellerId);
        }
    }
} 