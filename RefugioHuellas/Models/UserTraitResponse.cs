namespace RefugioHuellas.Models
{
    public class UserTraitResponse
    {
        public int Id { get; set; }

        // Usuario de Identity
        public string UserId { get; set; } = "";

        // Relación al rasgo
        public int TraitId { get; set; }
        public PersonalityTrait? Trait { get; set; }

        // Valor del usuario (1 a 5, por ejemplo)
        public int Value { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
