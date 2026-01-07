using RefugioHuellas.Models;
using RefugioHuellas.Models.ViewModels;

namespace RefugioHuellas.Services.Compatibility
{
    /// Abstracción del cálculo de compatibilidad.
    /// (DIP) Los controladores consumen esta interfaz y no la implementación concreta.
    public interface ICompatibilityService
    {
        Task<int> CalculateFromAnswersAsync(Dog dog, IEnumerable<CompatibilityAnswerVm> answers);
        Task<int> CalculateFromUserProfileAsync(Dog dog, string userId);
        Task<Dictionary<int, int>> CalculateBestMatchesForUserAsync(string userId);
    }
}
