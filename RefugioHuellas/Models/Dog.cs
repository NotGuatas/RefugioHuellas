// Models/Dog.cs
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace RefugioHuellas.Models
{
    public class Dog
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? PhotoUrl { get; set; }   // Ruta pública final: /uploads/xxxxx.jpg
        public string? HealthStatus { get; set; }
        public bool Sterilized { get; set; }
        public DateTime IntakeDate { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public IFormFile? PhotoFile { get; set; } // <-- archivo que sube el admin
    }
}
