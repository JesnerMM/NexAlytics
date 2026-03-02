namespace NexAlytics.Domain.Entities;

public class Venta
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ClienteId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pendiente";

    public Empresa? Empresa { get; set; }
    public Cliente? Cliente { get; set; }
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
