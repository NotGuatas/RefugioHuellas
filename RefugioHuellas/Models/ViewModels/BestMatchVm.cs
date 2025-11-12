namespace RefugioHuellas.Models.ViewModels
{
    public class BestMatchVm
    {
        public int DogId { get; set; }
        public string DogName { get; set; } = "";
        public DateTime IntakeDate { get; set; }

        public int? ApplicationId { get; set; }   // <- ganador
        public string? UserEmail { get; set; }
        public int CompatibilityScore { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
