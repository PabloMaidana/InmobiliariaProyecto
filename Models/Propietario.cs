using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models;

public class Propietario
{
    public int Id { get; set; }

    [Required]
    public int Dni { get; set; } = default!;

    [Required]
    public string Nombre { get; set; } = default!;

    [Required]
    public string Apellido { get; set; } = default!;

    [StringLength(100)]
    public string? Telefono { get; set; }

    [EmailAddress, Required]
    public string? Email { get; set; }
}