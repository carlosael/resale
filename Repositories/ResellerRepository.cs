using Microsoft.EntityFrameworkCore;
using ResaleApi.Data;
using ResaleApi.Models;

namespace ResaleApi.Repositories
{
    public class ResellerRepository : IResellerRepository
    {
        private readonly AppDbContext _context;

        public ResellerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Reseller?> GetByIdAsync(Guid id)
        {
            return await _context.Resellers
                .Include(r => r.Phones)
                .Include(r => r.Contacts)
                .Include(r => r.Addresses)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Reseller?> GetByCnpjAsync(string cnpj)
        {
            return await _context.Resellers
                .Include(r => r.Phones)
                .Include(r => r.Contacts)
                .Include(r => r.Addresses)
                .FirstOrDefaultAsync(r => r.Cnpj == cnpj);
        }

        public async Task<Reseller?> GetByEmailAsync(string email)
        {
            return await _context.Resellers
                .Include(r => r.Phones)
                .Include(r => r.Contacts)
                .Include(r => r.Addresses)
                .FirstOrDefaultAsync(r => r.Email == email);
        }

        public async Task<IEnumerable<Reseller>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Resellers
                .Include(r => r.Phones)
                .Include(r => r.Contacts)
                .Include(r => r.Addresses)
                .OrderBy(r => r.CompanyName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Reseller> CreateAsync(Reseller reseller)
        {
            _context.Resellers.Add(reseller);
            await _context.SaveChangesAsync();
            return reseller;
        }

        public async Task<Reseller> UpdateAsync(Reseller reseller)
        {
            reseller.UpdatedAt = DateTime.UtcNow;
            _context.Resellers.Update(reseller);
            await _context.SaveChangesAsync();
            return reseller;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var reseller = await _context.Resellers.FindAsync(id);
            if (reseller == null)
                return false;

            reseller.IsActive = false;
            reseller.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByCnpjAsync(string cnpj)
        {
            return await _context.Resellers.AnyAsync(r => r.Cnpj == cnpj);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Resellers.AnyAsync(r => r.Email == email);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Resellers.CountAsync();
        }
    }
} 