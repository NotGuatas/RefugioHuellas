using RefugioHuellas.Models;

namespace RefugioHuellas.Services.Compatibility.Rules
{
    public class NoiseToleranceRule : ITraitRule
    {
        public string Key => "noiseTolerance";

        public double Apply(Dog dog, int answerValue1to5, double currentValue0to100)
        {
            // Perros pequeños suelen ser más vocales (simplificación)
            if (dog.Size == "Pequeño") currentValue0to100 += (answerValue1to5 >= 5 ? 8 : -10);
            return currentValue0to100;
        }
    }
}
