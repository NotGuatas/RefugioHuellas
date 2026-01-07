using RefugioHuellas.Models;

namespace RefugioHuellas.Data.Repositories
{
    public interface ITraitRepository
    {
        Task<List<PersonalityTrait>> GetActiveTraitsAsync();
    }
}
