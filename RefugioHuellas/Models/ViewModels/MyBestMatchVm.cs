namespace RefugioHuellas.Models.ViewModels
{
    public class MyBestMatchVm
    {
        public int DogId { get; set; }
        public string DogName { get; set; } = "";
        public string? PhotoUrl { get; set; }
        public string Size { get; set; } = "";
        public int EnergyLevel { get; set; }
        public DateTime IntakeDate { get; set; }
        public int CompatibilityScore { get; set; }
    }
}
