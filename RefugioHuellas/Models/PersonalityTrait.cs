namespace RefugioHuellas.Models
{
    public class PersonalityTrait
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";   // ejemplo. "energia", "actividad", "ninos", "tiempoLibre"
        public string Name { get; set; } = "";  // ejemplo. "Nivel de energía"
        public int Weight { get; set; } = 1;    // peso del rasgo en el cálculo
        public bool Active { get; set; } = true;
        public string? Prompt { get; set; }      // texto de la pregunta del formulario

    }
}
