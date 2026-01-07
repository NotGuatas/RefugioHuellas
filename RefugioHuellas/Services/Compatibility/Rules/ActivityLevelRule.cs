using RefugioHuellas.Models;

namespace RefugioHuellas.Services.Compatibility.Rules
{
    public class ActivityLevelRule : ITraitRule
    {
        public string Key => "activityLevel";

        public double Apply(Dog dog, int answerValue1to5, double currentValue0to100)
        {
            // Match directo: diferencia grande resta fuerte
            int desired = dog.EnergyLevel;
            int userAct = answerValue1to5;
            int diff = Math.Abs(desired - userAct);
            currentValue0to100 += diff switch { 0 => 15, 1 => 8, 2 => -10, _ => -18 };
            return currentValue0to100;
        }
    }
}