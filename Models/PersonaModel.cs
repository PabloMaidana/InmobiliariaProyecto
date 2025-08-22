namespace Inmobiliaria.Models;

public class Persona
{
    public int Id { get; set; }
    public int Dni { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string Apellido { get; set; } = default!;
    public string? Telefono { get; set; }
   public string? Email { get; set; }
}