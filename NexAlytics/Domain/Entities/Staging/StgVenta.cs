namespace NexAlytics.Domain.Entities.Staging;

public class StgVenta
{
    public int Id { get; set; }
    public int ImportJobId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Procesado { get; set; }
    public string? Error { get; set; }
}
