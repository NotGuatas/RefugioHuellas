using RefugioHuellas.Models;

namespace RefugioHuellas.Services.Compatibility.Rules
{
    public class HousingTypeRule : ITraitRule
    {
        public string Key => "housingType";

        public double Apply(Dog dog, int answerValue1to5, double currentValue0to100)
        {
            // Sí=5 => Depa; No=1 => NO Depa
            if (dog.IdealEnvironment?.Contains("Departamento", StringComparison.OrdinalIgnoreCase) == true)
                currentValue0to100 += (answerValue1to5 == 5 ? 10 : -25);

            if (dog.IdealEnvironment?.Contains("Casa", StringComparison.OrdinalIgnoreCase) == true)
                currentValue0to100 += (answerValue1to5 == 1 ? 10 : -15);

            return currentValue0to100;
        }
    }
}
