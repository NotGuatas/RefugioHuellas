namespace RefugioHuellas.Models
{
    public class AdoptionApplication
    {
        public int Id { get; set; }
        public int DogId { get; set; }
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pendiente"; // Pendiente/Aprobado/Rechazado

        // navegación (opcional futuro)
        public Dog? Dog { get; set; }
    }
}
