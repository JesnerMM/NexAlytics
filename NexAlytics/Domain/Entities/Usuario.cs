namespace NexAlytics.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "Admin";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Empresa? Empresa { get; set; }
}
