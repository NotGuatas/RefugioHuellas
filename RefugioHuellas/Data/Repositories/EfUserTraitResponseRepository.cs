using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Models;

namespace RefugioHuellas.Data.Repositories
{
    public class EfUserTraitResponseRepository : IUserTraitResponseRepository
    {
        private readonly ApplicationDbContext _db;
        public EfUserTraitResponseRepository(ApplicationDbContext db) => _db = db;

        public Task<bool> HasProfileAsync(string userId)
            => _db.UserTraitResponses.AnyAsync(r => r.UserId == userId);

        public Task<List<UserTraitResponse>> GetForUserAsync(string userId)
            => _db.UserTraitResponses.Where(r => r.UserId == userId).ToListAsync();
    }
}
