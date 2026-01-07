using RefugioHuellas.Models;

namespace RefugioHuellas.Data.Repositories
{
    public interface IUserTraitResponseRepository
    {
        Task<bool> HasProfileAsync(string userId);
        Task<List<UserTraitResponse>> GetForUserAsync(string userId);
    }
}
