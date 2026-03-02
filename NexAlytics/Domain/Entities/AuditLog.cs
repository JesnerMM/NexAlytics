namespace NexAlytics.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;   // Crear, Actualizar, Eliminar
    public string Entidad { get; set; } = string.Empty;  // Cliente, Producto, Venta, Pago, Usuario
    public int? EntidadId { get; set; }
    public string? Detalle { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
