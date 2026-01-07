using RefugioHuellas.Models;

namespace RefugioHuellas.Services.Compatibility.Rules
{
    /// Strategy: cada regla encapsula el ajuste de score para un rasgo específico (t.Key).
 
    public interface ITraitRule
    {
        /// Clave del rasgo que esta regla entiende (ej: "housingType").
        string Key { get; }

        /// Ajusta el valor (0..100) para el rasgo actual en función del perro y la respuesta del usuario (1..5).
        double Apply(Dog dog, int answerValue1to5, double currentValue0to100);
    }
}
