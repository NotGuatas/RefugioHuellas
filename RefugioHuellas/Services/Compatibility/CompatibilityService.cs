using RefugioHuellas.Data.Repositories;
using RefugioHuellas.Models;
using RefugioHuellas.Models.ViewModels;
using RefugioHuellas.Services.Compatibility.Rules;

namespace RefugioHuellas.Services.Compatibility
{
  
    /// SRP: este servicio orquesta el cálculo, pero delega:
    /// - Lectura de datos a repositorios
    /// - Reglas por rasgo a estrategias (ITraitRule)
    /// - Selección de regla a una fábrica

    public class CompatibilityService : ICompatibilityService
    {
        private readonly ITraitRepository _traits;
        private readonly IUserTraitResponseRepository _responses;
        private readonly IDogRepository _dogs;
        private readonly ITraitRuleFactory _ruleFactory;

        public CompatibilityService(
            ITraitRepository traits,
            IUserTraitResponseRepository responses,
            IDogRepository dogs,
            ITraitRuleFactory ruleFactory)
        {
            _traits = traits;
            _responses = responses;
            _dogs = dogs;
            _ruleFactory = ruleFactory;
        }

        public async Task<int> CalculateFromAnswersAsync(Dog dog, IEnumerable<CompatibilityAnswerVm> answers)
        {
            var traits = await _traits.GetActiveTraitsAsync();
            if (!traits.Any()) return 50;

            var map = answers
                .GroupBy(a => a.TraitId)
                .ToDictionary(g => g.Key, g => Math.Clamp(g.Last().Value, 1, 5));

            return CalculateInternal(dog, traits, map);
        }

        public async Task<int> CalculateFromUserProfileAsync(Dog dog, string userId)
        {
            var traits = await _traits.GetActiveTraitsAsync();
            if (!traits.Any()) return 50;

            var responses = await _responses.GetForUserAsync(userId);
            if (!responses.Any()) return 50;

            var map = responses
                .GroupBy(r => r.TraitId)
                .ToDictionary(g => g.Key, g => Math.Clamp(g.Last().Value, 1, 5));

            return CalculateInternal(dog, traits, map);
        }

        public async Task<Dictionary<int, int>> CalculateBestMatchesForUserAsync(string userId)
        {
            var dogs = await _dogs.GetAllAsync();
            var result = new Dictionary<int, int>(capacity: dogs.Count);

            foreach (var dog in dogs)
            {
                result[dog.Id] = await CalculateFromUserProfileAsync(dog, userId);
            }

            return result;
        }

        private int CalculateInternal(Dog dog, List<PersonalityTrait> traits, Dictionary<int, int> answers)
        {
            int totalWeight = traits.Sum(t => t.Weight);
            double sum = 0;

            foreach (var t in traits)
            {
                if (!answers.TryGetValue(t.Id, out var val15)) continue;

                // Valor base normalizado 0..100
                double val = (val15 - 1) / 4.0 * 100.0;

                // OCP: la lógica extra por clave NO vive aquí, vive en una Strategy.
                var rule = _ruleFactory.GetForKey(t.Key);
                if (rule != null)
                    val = rule.Apply(dog, val15, val);

                sum += val * t.Weight;
            }

            var score = Math.Clamp(sum / Math.Max(1, totalWeight), 0, 100);
            if (dog.Sterilized) score = Math.Min(100, score + 3);

            return (int)Math.Round(score);
        }
    }
}
