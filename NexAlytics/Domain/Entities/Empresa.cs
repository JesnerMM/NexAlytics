namespace NexAlytics.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
