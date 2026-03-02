namespace NexAlytics.Domain.Entities.DataWarehouse;

public class DimFecha
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int Año { get; set; }
    public int Mes { get; set; }
    public string NombreMes { get; set; } = string.Empty;
    public int Trimestre { get; set; }
}
