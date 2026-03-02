using System.ComponentModel.DataAnnotations;

namespace NexAlytics.Application.DTOs;

public class ClienteDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Telefono { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }
    public int VentasCount { get; set; }
    public decimal TotalVentas { get; set; }
}
