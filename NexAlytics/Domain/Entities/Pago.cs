namespace NexAlytics.Domain.Entities;

public class Pago
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public string Metodo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public Venta? Venta { get; set; }
}
