namespace RefugioHuellas.Models.ViewModels
{
    public class CompatibilityAnswerVm
    {
        public int TraitId { get; set; }
        public string Key { get; set; } = "";
        public string Prompt { get; set; } = "";
        public int Value { get; set; } // 1..5 (Sí/No -> 5/1)
    }

    public class CompatibilityFormVm
    {
        public int DogId { get; set; }
        public string DogName { get; set; } = "";
        public List<CompatibilityAnswerVm> Answers { get; set; } = new();
    }
}
