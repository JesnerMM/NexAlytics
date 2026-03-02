using Microsoft.EntityFrameworkCore;
using NexAlytics.Application.DTOs;
using NexAlytics.Domain.Entities;
using NexAlytics.Domain.Entities.DataWarehouse;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Application.Services;

public class VentaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VentaService> _logger;

    public VentaService(ApplicationDbContext context, ILogger<VentaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<VentaDto>> GetAllAsync(int empresaId,
        string? estado = null, DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.Ventas.Where(v => v.EmpresaId == empresaId);

        if (!string.IsNullOrEmpty(estado)) query = query.Where(v => v.Estado == estado);
        if (desde.HasValue) query = query.Where(v => v.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(v => v.Fecha <= hasta.Value.AddDays(1).AddSeconds(-1));

        return await query
            .Select(v => new VentaDto
            {
                Id = v.Id,
                ClienteId = v.ClienteId,
                ClienteNombre = v.Cliente != null ? v.Cliente.Nombre : string.Empty,
                Fecha = v.Fecha,
                Total = v.Total,
                Estado = v.Estado
            })
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<PagedResult<VentaDto>> GetPagedAsync(int empresaId, int page, int pageSize,
        string? estado = null, DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.Ventas.Where(v => v.EmpresaId == empresaId);

        if (!string.IsNullOrEmpty(estado)) query = query.Where(v => v.Estado == estado);
        if (desde.HasValue) query = query.Where(v => v.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(v => v.Fecha <= hasta.Value.AddDays(1).AddSeconds(-1));

        var total = await query.CountAsync();

        var items = await query
            .Select(v => new VentaDto
            {
                Id = v.Id,
                ClienteId = v.ClienteId,
                ClienteNombre = v.Cliente != null ? v.Cliente.Nombre : string.Empty,
                Fecha = v.Fecha,
                Total = v.Total,
                Estado = v.Estado
            })
            .OrderByDescending(v => v.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<VentaDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<VentaDto?> GetByIdAsync(int id, int empresaId)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Pagos)
            .FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == empresaId);

        if (venta == null) return null;

        return new VentaDto
        {
            Id = venta.Id,
            ClienteId = venta.ClienteId,
            ClienteNombre = venta.Cliente?.Nombre ?? string.Empty,
            Fecha = venta.Fecha,
            Total = venta.Total,
            Estado = venta.Estado,
            Pagos = venta.Pagos.Select(p => new PagoDto
            {
                Id = p.Id,
                VentaId = p.VentaId,
                Metodo = p.Metodo,
                Monto = p.Monto,
                Fecha = p.Fecha
            }).ToList()
        };
    }

    public async Task<VentaDto> CreateAsync(VentaDto dto, int empresaId)
    {
        var entity = new Venta
        {
            EmpresaId = empresaId,
            ClienteId = dto.ClienteId,
            Fecha = dto.Fecha,
            Total = dto.Total,
            Estado = dto.Estado
        };
        _context.Ventas.Add(entity);
        await _context.SaveChangesAsync();

        await InsertFactVentaAsync(entity, empresaId);

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(VentaDto dto, int empresaId)
    {
        var entity = await _context.Ventas
            .FirstOrDefaultAsync(v => v.Id == dto.Id && v.EmpresaId == empresaId);
        if (entity == null) return false;

        entity.ClienteId = dto.ClienteId;
        entity.Fecha = dto.Fecha;
        entity.Total = dto.Total;
        entity.Estado = dto.Estado;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int empresaId)
    {
        var entity = await _context.Ventas
            .FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == empresaId);
        if (entity == null) return false;
        _context.Ventas.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task InsertFactVentaAsync(Venta venta, int empresaId)
    {
        var dateOnly = venta.Fecha.Date;
        var dimFecha = await _context.DimFechas.FirstOrDefaultAsync(d => d.Fecha == dateOnly);
        if (dimFecha == null)
        {
            var mes = dateOnly.Month;
            dimFecha = new DimFecha
            {
                Fecha = dateOnly,
                Año = dateOnly.Year,
                Mes = mes,
                NombreMes = dateOnly.ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("es-ES")),
                Trimestre = (mes - 1) / 3 + 1
            };
            _context.DimFechas.Add(dimFecha);
            await _context.SaveChangesAsync();
        }

        var dimCliente = await _context.DimClientes
            .FirstOrDefaultAsync(d => d.ClienteId == venta.ClienteId);
        if (dimCliente == null) return;

        _context.FactVentas.Add(new FactVenta
        {
            EmpresaId = empresaId,
            FechaId = dimFecha.Id,
            ClienteDimId = dimCliente.Id,
            VentaId = venta.Id,
            Total = venta.Total,
            Cantidad = 1
        });
        await _context.SaveChangesAsync();
    }
}
