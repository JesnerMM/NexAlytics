namespace NexAlytics.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;

    public Empresa? Empresa { get; set; }
}
