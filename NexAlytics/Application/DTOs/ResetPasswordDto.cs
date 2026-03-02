using System.ComponentModel.DataAnnotations;

namespace NexAlytics.Application.DTOs;

public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string NuevaPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmar contraseña es requerido")]
    [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}
