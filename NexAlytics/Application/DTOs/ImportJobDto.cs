namespace NexAlytics.Application.DTOs;

public class ImportJobDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string ArchivoNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int Registros { get; set; }
    public int RegistrosProcesados { get; set; }
    public int RegistrosError { get; set; }
    public DateTime Fecha { get; set; }
}

public class ImportPreviewDto
{
    public int ImportJobId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string ArchivoNombre { get; set; } = string.Empty;
    public int TotalRegistros { get; set; }
    public int RegistrosValidos { get; set; }
    public int RegistrosError { get; set; }
    public List<Dictionary<string, string>> Rows { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Columns { get; set; } = new();
}
