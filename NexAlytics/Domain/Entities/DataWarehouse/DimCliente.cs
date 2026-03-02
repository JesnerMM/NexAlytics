namespace NexAlytics.Domain.Entities.DataWarehouse;

public class DimCliente
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
