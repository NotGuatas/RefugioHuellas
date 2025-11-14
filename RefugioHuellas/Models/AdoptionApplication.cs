using System.ComponentModel.DataAnnotations;

namespace RefugioHuellas.Models
{
    public class AdoptionApplication
    {
        public int Id { get; set; }  // Id único de adopción
        public int DogId { get; set; } // El id del perrito ejemplo 1,2,3,4
        public string UserId { get; set; } = string.Empty; // Id del usuario 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Fecha exacta en la que se registro 
        public string Status { get; set; } = "Pendiente";  // Estado en el que se encuentra la petición de adopción  Pendiente/Aprobado/Rechazado

        public Dog? Dog { get; set; } 
        public int CompatibilityScore { get; set; } = 0; // 0–100

        [Required(ErrorMessage = "El teléfono de contacto es obligatorio.")]
        [RegularExpression(@"^09\d{8}$",
        ErrorMessage = "El teléfono debe iniciar con 09 y tener 10 dígitos.")]
        [Display(Name = "Teléfono de contacto")]
        public string Phone { get; set; } = string.Empty;

    }
}
