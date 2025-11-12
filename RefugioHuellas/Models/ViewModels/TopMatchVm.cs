namespace RefugioHuellas.Models.ViewModels
{
    public class TopMatchCandidateVm
    {
        public int ApplicationId { get; set; }
        public string? UserEmail { get; set; }
        public int CompatibilityScore { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class TopMatchVm
    {
        public int DogId { get; set; }
        public string DogName { get; set; } = "";
        public DateTime IntakeDate { get; set; }

        // Top 3 candidatos
        public List<TopMatchCandidateVm> TopCandidates { get; set; } = new();
    }
}
