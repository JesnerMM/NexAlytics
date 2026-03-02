namespace NexAlytics.Domain.Entities.DataWarehouse;

public class DimProducto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
}
