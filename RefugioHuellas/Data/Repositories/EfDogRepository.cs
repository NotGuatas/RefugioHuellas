using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Models;

namespace RefugioHuellas.Data.Repositories
{
    public class EfDogRepository : IDogRepository
    {
        private readonly ApplicationDbContext _db;
        public EfDogRepository(ApplicationDbContext db) => _db = db;

        public async Task<List<Dog>> GetAllAsync(bool includeOriginType = false)
        {
            IQueryable<Dog> q = _db.Dogs;
            if (includeOriginType) q = q.Include(d => d.OriginType);
            return await q.OrderByDescending(d => d.IntakeDate).ToListAsync();
        }

        public async Task<Dog?> GetByIdAsync(int id, bool includeOriginType = false)
        {
            IQueryable<Dog> q = _db.Dogs;
            if (includeOriginType) q = q.Include(d => d.OriginType);
            return await q.FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
