using System.ComponentModel.DataAnnotations;

namespace NexAlytics.Application.DTOs;

public class VentaDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El cliente es obligatorio")]
    public int ClienteId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "El total es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El total debe ser mayor a 0")]
    public decimal Total { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio")]
    public string Estado { get; set; } = "Pendiente";

    public List<PagoDto> Pagos { get; set; } = new();
}
