namespace RefugioHuellas.Models.ViewModels
{
    public class AdminAdoptionVm
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string? UserEmail { get; set; }
        public string DogName { get; set; } = "";
        public int CompatibilityScore { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class AdminAdoptionDetailVm
    {
        public int Id { get; set; }
        public string? UserEmail { get; set; }
        public string DogName { get; set; } = "";
        public int CompatibilityScore { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public List<(string Prompt, int Value)> Answers { get; set; } = new();
    }
}
