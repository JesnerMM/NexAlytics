namespace NexAlytics.Domain.Entities.DataWarehouse;

public class FactPago
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int FechaId { get; set; }
    public int PagoId { get; set; }
    public decimal Monto { get; set; }

    public DimFecha? DimFecha { get; set; }
}
