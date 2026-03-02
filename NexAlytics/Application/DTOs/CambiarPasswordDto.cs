using System.ComponentModel.DataAnnotations;

namespace NexAlytics.Application.DTOs;

public class CambiarPasswordDto
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria")]
    [Display(Name = "Contraseña actual")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [Display(Name = "Nueva contraseña")]
    public string NuevaPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la nueva contraseña")]
    [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar nueva contraseña")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}
