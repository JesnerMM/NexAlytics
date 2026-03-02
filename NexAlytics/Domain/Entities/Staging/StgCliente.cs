namespace NexAlytics.Domain.Entities.Staging;

public class StgCliente
{
    public int Id { get; set; }
    public int ImportJobId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Procesado { get; set; }
    public string? Error { get; set; }
}
