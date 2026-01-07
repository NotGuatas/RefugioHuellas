using RefugioHuellas.Models;

namespace RefugioHuellas.Data.Repositories
{
    public interface IDogRepository
    {
        Task<List<Dog>> GetAllAsync(bool includeOriginType = false);
        Task<Dog?> GetByIdAsync(int id, bool includeOriginType = false);
    }
}
