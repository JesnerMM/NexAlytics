using Microsoft.EntityFrameworkCore;
using NexAlytics.Application.DTOs;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Application.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashboardDto> GetDashboardAsync(int empresaId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        var dto = new DashboardDto
        {
            VentasTotales = await _context.FactVentas
                .Where(f => f.EmpresaId == empresaId)
                .SumAsync(f => f.Total),

            ClientesActivos = await _context.Clientes
                .Where(c => c.EmpresaId == empresaId)
                .CountAsync(),

            PagosRecibidos = await _context.FactPagos
                .Where(f => f.EmpresaId == empresaId)
                .SumAsync(f => f.Monto),

            ImportacionesTotales = await _context.ImportJobs
                .Where(j => j.EmpresaId == empresaId && j.Estado != "Preview")
                .CountAsync(),

            VentasTotalesMesAnterior = await _context.FactVentas
                .Include(f => f.DimFecha)
                .Where(f => f.EmpresaId == empresaId && f.DimFecha != null
                    && f.DimFecha.Fecha >= startOfLastMonth && f.DimFecha.Fecha < startOfMonth)
                .SumAsync(f => f.Total)
        };

        // Ventas por mes (últimos 12 meses)
        dto.VentasPorMes = (await _context.FactVentas
            .Include(f => f.DimFecha)
            .Where(f => f.EmpresaId == empresaId && f.DimFecha != null
                && f.DimFecha.Fecha >= now.AddMonths(-12))
            .GroupBy(f => new { f.DimFecha!.Año, f.DimFecha.Mes, f.DimFecha.NombreMes })
            .Select(g => new
            {
                g.Key.Año,
                g.Key.Mes,
                g.Key.NombreMes,
                Value = g.Sum(x => x.Total)
            })
            .OrderBy(x => x.Año).ThenBy(x => x.Mes)
            .ToListAsync())
            .Select(x => new ChartDataPoint
            {
                Label = $"{x.NombreMes} {x.Año}",
                Value = x.Value
            }).ToList();

        // Ventas por estado
        dto.VentasPorEstado = await _context.Ventas
            .Where(v => v.EmpresaId == empresaId)
            .GroupBy(v => v.Estado)
            .Select(g => new ChartDataPoint { Label = g.Key, Value = g.Sum(v => v.Total) })
            .ToListAsync();

        // Últimas ventas
        dto.UltimasVentas = await _context.Ventas
            .Include(v => v.Cliente)
            .Where(v => v.EmpresaId == empresaId)
            .OrderByDescending(v => v.Fecha)
            .Take(10)
            .Select(v => new UltimaVentaDto
            {
                Id = v.Id,
                ClienteNombre = v.Cliente != null ? v.Cliente.Nombre : "N/A",
                Fecha = v.Fecha,
                Total = v.Total,
                Estado = v.Estado
            })
            .ToListAsync();

        return dto;
    }

    public async Task<List<ChartDataPoint>> GetVentasPorMesAsync(int empresaId, int months = 12)
    {
        var from = DateTime.UtcNow.AddMonths(-months);
        return (await _context.FactVentas
            .Include(f => f.DimFecha)
            .Where(f => f.EmpresaId == empresaId && f.DimFecha != null && f.DimFecha.Fecha >= from)
            .GroupBy(f => new { f.DimFecha!.Año, f.DimFecha.Mes, f.DimFecha.NombreMes })
            .Select(g => new
            {
                g.Key.Año,
                g.Key.Mes,
                g.Key.NombreMes,
                Value = g.Sum(x => x.Total)
            })
            .OrderBy(x => x.Año).ThenBy(x => x.Mes)
            .ToListAsync())
            .Select(x => new ChartDataPoint
            {
                Label = $"{x.NombreMes} {x.Año}",
                Value = x.Value
            }).ToList();
    }

    public async Task<List<ChartDataPoint>> GetPagosPorMetodoAsync(int empresaId)
    {
        return await _context.Pagos
            .Include(p => p.Venta)
            .Where(p => p.Venta != null && p.Venta.EmpresaId == empresaId)
            .GroupBy(p => p.Metodo)
            .Select(g => new ChartDataPoint { Label = g.Key, Value = g.Sum(p => p.Monto) })
            .ToListAsync();
    }

    public async Task<List<ChartDataPoint>> GetVentasPorCategoriaAsync(int empresaId)
    {
        return await _context.DimProductos
            .Where(d => d.EmpresaId == empresaId)
            .GroupBy(d => d.Categoria)
            .Select(g => new ChartDataPoint { Label = g.Key, Value = g.Count() })
            .ToListAsync();
    }
}
