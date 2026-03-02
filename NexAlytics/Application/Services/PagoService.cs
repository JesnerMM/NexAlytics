using Microsoft.EntityFrameworkCore;
using NexAlytics.Application.DTOs;
using NexAlytics.Domain.Entities;
using NexAlytics.Domain.Entities.DataWarehouse;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Application.Services;

public class PagoService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PagoService> _logger;

    public PagoService(ApplicationDbContext context, ILogger<PagoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PagoDto>> GetAllAsync(int empresaId,
        string? metodo = null, DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.Pagos.Where(p => p.Venta != null && p.Venta.EmpresaId == empresaId);

        if (!string.IsNullOrEmpty(metodo)) query = query.Where(p => p.Metodo == metodo);
        if (desde.HasValue) query = query.Where(p => p.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(p => p.Fecha <= hasta.Value.AddDays(1).AddSeconds(-1));

        return await query
            .Select(p => new PagoDto
            {
                Id = p.Id,
                VentaId = p.VentaId,
                VentaInfo = $"Venta #{p.VentaId}",
                Metodo = p.Metodo,
                Monto = p.Monto,
                Fecha = p.Fecha
            })
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();
    }

    public async Task<PagedResult<PagoDto>> GetPagedAsync(int empresaId, int page, int pageSize,
        string? metodo = null, DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.Pagos.Where(p => p.Venta != null && p.Venta.EmpresaId == empresaId);

        if (!string.IsNullOrEmpty(metodo)) query = query.Where(p => p.Metodo == metodo);
        if (desde.HasValue) query = query.Where(p => p.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(p => p.Fecha <= hasta.Value.AddDays(1).AddSeconds(-1));

        var total = await query.CountAsync();

        var items = await query
            .Select(p => new PagoDto
            {
                Id = p.Id,
                VentaId = p.VentaId,
                VentaInfo = $"Venta #{p.VentaId}",
                Metodo = p.Metodo,
                Monto = p.Monto,
                Fecha = p.Fecha
            })
            .OrderByDescending(p => p.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<PagoDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<PagoDto?> GetByIdAsync(int id, int empresaId)
    {
        return await _context.Pagos
            .Include(p => p.Venta)
            .Where(p => p.Id == id && p.Venta != null && p.Venta.EmpresaId == empresaId)
            .Select(p => new PagoDto
            {
                Id = p.Id,
                VentaId = p.VentaId,
                VentaInfo = $"Venta #{p.VentaId}",
                Metodo = p.Metodo,
                Monto = p.Monto,
                Fecha = p.Fecha
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PagoDto> CreateAsync(PagoDto dto, int empresaId)
    {
        var venta = await _context.Ventas
            .FirstOrDefaultAsync(v => v.Id == dto.VentaId && v.EmpresaId == empresaId);
        if (venta == null) throw new InvalidOperationException("Venta no encontrada");

        var entity = new Pago
        {
            VentaId = dto.VentaId,
            Metodo = dto.Metodo,
            Monto = dto.Monto,
            Fecha = dto.Fecha
        };
        _context.Pagos.Add(entity);
        await _context.SaveChangesAsync();

        // Insert FactPago
        var dateOnly = dto.Fecha.Date;
        var dimFecha = await _context.DimFechas.FirstOrDefaultAsync(d => d.Fecha == dateOnly);
        if (dimFecha == null)
        {
            var mes = dateOnly.Month;
            dimFecha = new DimFecha
            {
                Fecha = dateOnly, Año = dateOnly.Year, Mes = mes,
                NombreMes = dateOnly.ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("es-ES")),
                Trimestre = (mes - 1) / 3 + 1
            };
            _context.DimFechas.Add(dimFecha);
            await _context.SaveChangesAsync();
        }

        _context.FactPagos.Add(new FactPago
        {
            EmpresaId = empresaId,
            FechaId = dimFecha.Id,
            PagoId = entity.Id,
            Monto = entity.Monto
        });
        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(PagoDto dto, int empresaId)
    {
        var entity = await _context.Pagos
            .Include(p => p.Venta)
            .FirstOrDefaultAsync(p => p.Id == dto.Id && p.Venta != null && p.Venta.EmpresaId == empresaId);
        if (entity == null) return false;

        entity.Metodo = dto.Metodo;
        entity.Monto = dto.Monto;
        entity.Fecha = dto.Fecha;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int empresaId)
    {
        var entity = await _context.Pagos
            .Include(p => p.Venta)
            .FirstOrDefaultAsync(p => p.Id == id && p.Venta != null && p.Venta.EmpresaId == empresaId);
        if (entity == null) return false;
        _context.Pagos.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
