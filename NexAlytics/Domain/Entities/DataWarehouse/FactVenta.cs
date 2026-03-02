namespace NexAlytics.Domain.Entities.DataWarehouse;

public class FactVenta
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int FechaId { get; set; }
    public int ClienteDimId { get; set; }
    public int VentaId { get; set; }
    public decimal Total { get; set; }
    public int Cantidad { get; set; }

    public DimFecha? DimFecha { get; set; }
    public DimCliente? DimCliente { get; set; }
}
