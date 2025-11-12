namespace RefugioHuellas.Models
{
    public class AdoptionApplicationAnswer
    {
        public int Id { get; set; } // id único 

        public int AdoptionApplicationId { get; set; } // id de aplicacón de adopción
        public AdoptionApplication? AdoptionApplication { get; set; }

        public int TraitId { get; set; }
        public PersonalityTrait? Trait { get; set; }

        
        public int Value { get; set; }
    }
}
