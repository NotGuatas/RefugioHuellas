using RefugioHuellas.Models;

namespace RefugioHuellas.Services.Compatibility.Rules
{
    public class SpaceRule : ITraitRule
    {
        public string Key => "space";

        public double Apply(Dog dog, int answerValue1to5, double currentValue0to100)
        {
            // Patio/jardín: 5 = sí, 1 = no
            if (dog.Size == "Grande") currentValue0to100 += (answerValue1to5 >= 5 ? 15 : -35);
            if (dog.Size == "Mediano") currentValue0to100 += (answerValue1to5 >= 3 ? 8 : -12);
            if (dog.Size == "Pequeño") currentValue0to100 += (answerValue1to5 >= 1 ? 5 : 0); // no exige
            return currentValue0to100;
        }
    }
}
