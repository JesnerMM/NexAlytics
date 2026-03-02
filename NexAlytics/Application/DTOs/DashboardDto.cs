namespace NexAlytics.Application.DTOs;

public class DashboardDto
{
    public decimal VentasTotales { get; set; }
    public int ClientesActivos { get; set; }
    public decimal PagosRecibidos { get; set; }
    public int ImportacionesTotales { get; set; }

    public decimal VentasTotalesMesAnterior { get; set; }
    public int ClientesMesAnterior { get; set; }

    public List<ChartDataPoint> VentasPorMes { get; set; } = new();
    public List<ChartDataPoint> VentasPorEstado { get; set; } = new();
    public List<UltimaVentaDto> UltimasVentas { get; set; } = new();
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class UltimaVentaDto
{
    public int Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
}
