namespace NexAlytics.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public Empresa? Empresa { get; set; }
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
