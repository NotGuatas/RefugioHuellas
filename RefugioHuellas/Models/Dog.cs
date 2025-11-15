// Models/Dog.cs Creación de un perro que solo el admin puede ser 
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace RefugioHuellas.Models
{
    public class Dog
    {
        public int Id { get; set; } // Id único del perro 
        public string Name { get; set; } = ""; //Nombre del perrito
        public string? Description { get; set; } // Descripción del perro 
        public string? PhotoUrl { get; set; }   // Ruta pública final que se sube a la carpeta wwrot: /uploads/xxxxx.jpg
        public string? HealthStatus { get; set; } // Estado de saul
        public bool Sterilized { get; set; } // Sale una opción si esta esterilizado con un check
        public DateTime IntakeDate { get; set; } = DateTime.UtcNow; // Sale la fecha local exacta con la que se carga al perro ejem: 10/11/2025
        public string Breed { get; set; } = ""; // raza del perro
        public string Size { get; set; } = "";  //  Tamaño del perro "Pequeño", "Mediano", "Grande"
        public int EnergyLevel { get; set; } = 3; //  Nivel de energia del perro del 1 a 5
        public string IdealEnvironment { get; set; } = ""; // Selección entre "Departamento", "Casa con patio", para perrito

        public int OriginTypeId { get; set; }        // FK obligatoria
        public OriginType? OriginType { get; set; }  // Navegación


        [NotMapped]
        public IFormFile? PhotoFile { get; set; } // Foto que se sube tiene que ser menos -5mb
    }
}
