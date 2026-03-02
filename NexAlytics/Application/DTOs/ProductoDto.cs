using System.ComponentModel.DataAnnotations;

namespace NexAlytics.Application.DTOs;

public class ProductoDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [MaxLength(100)]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    public bool Activo { get; set; } = true;
}
