using System.ComponentModel.DataAnnotations;

namespace NexAlytics.Application.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(200)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(200)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = "Usuario";

    public DateTime FechaCreacion { get; set; }

    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [Display(Name = "Contraseña")]
    public string? Password { get; set; }

    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar Contraseña")]
    public string? ConfirmPassword { get; set; }
}
