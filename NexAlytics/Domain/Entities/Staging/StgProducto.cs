namespace NexAlytics.Domain.Entities.Staging;

public class StgProducto
{
    public int Id { get; set; }
    public int ImportJobId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Precio { get; set; } = string.Empty;
    public bool Procesado { get; set; }
    public string? Error { get; set; }
}
