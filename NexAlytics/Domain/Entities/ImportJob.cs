namespace NexAlytics.Domain.Entities;

public class ImportJob
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string ArchivoNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente";
    public int Registros { get; set; }
    public int RegistrosProcesados { get; set; }
    public int RegistrosError { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public Empresa? Empresa { get; set; }
}
