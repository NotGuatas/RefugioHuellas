using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Models;

namespace RefugioHuellas.Data.Repositories
{
    public class EfTraitRepository : ITraitRepository
    {
        private readonly ApplicationDbContext _db;
        public EfTraitRepository(ApplicationDbContext db) => _db = db;

        public Task<List<PersonalityTrait>> GetActiveTraitsAsync()
            => _db.PersonalityTraits.Where(t => t.Active).ToListAsync();
    }
}
