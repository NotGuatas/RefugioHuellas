using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Models;
using RefugioHuellas.Models.ViewModels;

namespace RefugioHuellas.Services
{
    public class CompatibilityService
    {
        private readonly ApplicationDbContext _db;
        public CompatibilityService(ApplicationDbContext db) => _db = db;

        // calcula en base a las respuestas del formulario 
        public async Task<int> CalculateFromAnswersAsync(Dog dog, IEnumerable<CompatibilityAnswerVm> answers)
        {
            var traits = await _db.PersonalityTraits.Where(t => t.Active).ToListAsync();
            var map = answers.ToDictionary(a => a.TraitId, a => Math.Clamp(a.Value, 1, 5));

            if (!traits.Any()) return 50;

            int totalWeight = traits.Sum(t => t.Weight);
            double sum = 0;

            foreach (var t in traits)
            {
                if (!map.TryGetValue(t.Id, out var val15)) continue;

                // Valor base normalizado 0..100
                double val = (val15 - 1) / 4.0 * 100.0;

                // Reglas más estrictas por clave, cruzando perro vs respuestas
                switch (t.Key)
                {
                    case "housingType":
                        // Sí=5 => Depa; No=1 => NO Depa
                        if (dog.IdealEnvironment.Contains("Departamento", StringComparison.OrdinalIgnoreCase))
                            val += (val15 == 5 ? 10 : -25); // penaliza fuerte si no vive en departamento
                        if (dog.IdealEnvironment.Contains("Casa", StringComparison.OrdinalIgnoreCase))
                            val += (val15 == 1 ? 10 : -15); // si vive en depa pero perro requiere casa, baja
                        break;

                    case "space":
                        // Patio/jardín: 5 = sí, 1 = no
                        if (dog.Size == "Grande") val += (val15 >= 5 ? 15 : -35);
                        if (dog.Size == "Mediano") val += (val15 >= 3 ? 8 : -12);
                        if (dog.Size == "Pequeño") val += (val15 >= 1 ? 5 : 0); // no exige
                        break;

                    case "time":
                        // Paseos/tiempo: perros enérgicos requieren tiempo
                        if (dog.EnergyLevel >= 4) val += (val15 >= 4 ? 20 : -30);
                        else if (dog.EnergyLevel <= 2) val += (val15 <= 3 ? 8 : -5);
                        break;

                    case "noiseTolerance":
                        // Perros pequeños suelen ser más vocales (simplificación)
                        if (dog.Size == "Pequeño") val += (val15 >= 5 ? 8 : -10);
                        break;

                    case "activityLevel":
                        // Match directo: diferencia grande resta fuerte
                        int desired = dog.EnergyLevel;
                        int userAct = val15;
                        int diff = Math.Abs(desired - userAct);
                        val += diff switch { 0 => 15, 1 => 8, 2 => -10, _ => -18 };
                        break;
                }

                sum += val * t.Weight;
            }

            // esterilizado
            var score = Math.Clamp(sum / Math.Max(1, totalWeight), 0, 100);
            if (dog.Sterilized) score = Math.Min(100, score + 3);

            return (int)Math.Round(score);
        }

        //  calcula usando el perfil guardado del usuario (UserTraitResponses)
        public async Task<int> CalculateFromUserProfileAsync(Dog dog, string userId)
        {
            // mismos rasgos activos
            var traits = await _db.PersonalityTraits.Where(t => t.Active).ToListAsync();
            if (!traits.Any()) return 50;

            // respuestas guardadas del usuario
            var responses = await _db.UserTraitResponses
                                     .Where(r => r.UserId == userId)
                                     .ToListAsync();

            // si el usuario aún no tiene perfil, devolvemos un valor neutral
            if (!responses.Any()) return 50;

            var map = responses.ToDictionary(r => r.TraitId, r => Math.Clamp(r.Value, 1, 5));

            int totalWeight = traits.Sum(t => t.Weight);
            double sum = 0;

            foreach (var t in traits)
            {
                if (!map.TryGetValue(t.Id, out var val15)) continue;

                double val = (val15 - 1) / 4.0 * 100.0;

                switch (t.Key)
                {
                    case "housingType":
                        if (dog.IdealEnvironment.Contains("Departamento", StringComparison.OrdinalIgnoreCase))
                            val += (val15 == 5 ? 10 : -25);
                        if (dog.IdealEnvironment.Contains("Casa", StringComparison.OrdinalIgnoreCase))
                            val += (val15 == 1 ? 10 : -15);
                        break;

                    case "space":
                        if (dog.Size == "Grande") val += (val15 >= 5 ? 15 : -35);
                        if (dog.Size == "Mediano") val += (val15 >= 3 ? 8 : -12);
                        if (dog.Size == "Pequeño") val += (val15 >= 1 ? 5 : 0);
                        break;

                    case "time":
                        if (dog.EnergyLevel >= 4) val += (val15 >= 4 ? 20 : -30);
                        else if (dog.EnergyLevel <= 2) val += (val15 <= 3 ? 8 : -5);
                        break;

                    case "noiseTolerance":
                        if (dog.Size == "Pequeño") val += (val15 >= 5 ? 8 : -10);
                        break;

                    case "activityLevel":
                        int desired = dog.EnergyLevel;
                        int userAct = val15;
                        int diff = Math.Abs(desired - userAct);
                        val += diff switch { 0 => 15, 1 => 8, 2 => -10, _ => -18 };
                        break;
                }

                sum += val * t.Weight;
            }

            var score = Math.Clamp(sum / Math.Max(1, totalWeight), 0, 100);
            if (dog.Sterilized) score = Math.Min(100, score + 3);

            return (int)Math.Round(score);
        }

        //  mejores coincidencias para un usuario (DogId -> compatibilidad)
        public async Task<Dictionary<int, int>> CalculateBestMatchesForUserAsync(string userId)
        {
            var dogs = await _db.Dogs
                               .OrderBy(d => d.IntakeDate)
                               .ToListAsync();

            var result = new Dictionary<int, int>();

            foreach (var dog in dogs)
            {
                var score = await CalculateFromUserProfileAsync(dog, userId);
                result[dog.Id] = score;
            }

            return result;
        }
    }
}
