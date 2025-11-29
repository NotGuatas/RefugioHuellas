using System.Collections.Generic;

namespace RefugioHuellas.Models
{
    public class OriginType
    {
        public int Id { get; set; }          // PK
        public string Name { get; set; } = string.Empty; // Calle, Rescate policial, etc.

        // Relación
        public ICollection<Dog> Dogs { get; set; } = new List<Dog>();
    }
}
