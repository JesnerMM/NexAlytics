using System.ComponentModel.DataAnnotations;

namespace NexAlytics.Application.DTOs;

public class PagoDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La venta es obligatoria")]
    public int VentaId { get; set; }

    public string VentaInfo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El método de pago es obligatorio")]
    [MaxLength(50)]
    public string Metodo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Today;
}
