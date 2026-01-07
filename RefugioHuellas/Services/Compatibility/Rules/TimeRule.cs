using RefugioHuellas.Models;

namespace RefugioHuellas.Services.Compatibility.Rules
{
    public class TimeRule : ITraitRule
    {
        public string Key => "time";

        public double Apply(Dog dog, int answerValue1to5, double currentValue0to100)
        {
            // Paseos/tiempo: perros enérgicos requieren tiempo
            if (dog.EnergyLevel >= 4) currentValue0to100 += (answerValue1to5 >= 4 ? 20 : -30);
            else if (dog.EnergyLevel <= 2) currentValue0to100 += (answerValue1to5 <= 3 ? 8 : -5);

            return currentValue0to100;
        }
    }
}
