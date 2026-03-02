using Microsoft.EntityFrameworkCore;
using NexAlytics.Application.DTOs;
using NexAlytics.Domain.Entities;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Application.Services;

public class ProductoService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductoService> _logger;

    public ProductoService(ApplicationDbContext context, ILogger<ProductoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ProductoDto>> GetAllAsync(int empresaId, string? categoria = null, bool? activo = null)
    {
        var query = _context.Productos.Where(p => p.EmpresaId == empresaId);
        if (!string.IsNullOrEmpty(categoria)) query = query.Where(p => p.Categoria == categoria);
        if (activo.HasValue) query = query.Where(p => p.Activo == activo.Value);

        return await query
            .Select(p => new ProductoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Categoria = p.Categoria,
                Precio = p.Precio,
                Activo = p.Activo
            })
            .OrderBy(p => p.Categoria).ThenBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<PagedResult<ProductoDto>> GetPagedAsync(int empresaId, int page, int pageSize, string? categoria = null, bool? activo = null)
    {
        var query = _context.Productos.Where(p => p.EmpresaId == empresaId);
        if (!string.IsNullOrEmpty(categoria)) query = query.Where(p => p.Categoria == categoria);
        if (activo.HasValue) query = query.Where(p => p.Activo == activo.Value);

        var total = await query.CountAsync();

        var items = await query
            .Select(p => new ProductoDto { Id = p.Id, Nombre = p.Nombre, Categoria = p.Categoria, Precio = p.Precio, Activo = p.Activo })
            .OrderBy(p => p.Categoria).ThenBy(p => p.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductoDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<ProductoDto?> GetByIdAsync(int id, int empresaId)
    {
        return await _context.Productos
            .Where(p => p.Id == id && p.EmpresaId == empresaId)
            .Select(p => new ProductoDto { Id = p.Id, Nombre = p.Nombre, Categoria = p.Categoria, Precio = p.Precio, Activo = p.Activo })
            .FirstOrDefaultAsync();
    }

    public async Task<List<string>> GetCategoriasAsync(int empresaId) =>
        await _context.Productos
            .Where(p => p.EmpresaId == empresaId)
            .Select(p => p.Categoria)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

    public async Task<ProductoDto> CreateAsync(ProductoDto dto, int empresaId)
    {
        var entity = new Producto
        {
            EmpresaId = empresaId,
            Nombre = dto.Nombre,
            Categoria = dto.Categoria,
            Precio = dto.Precio,
            Activo = dto.Activo
        };
        _context.Productos.Add(entity);
        await _context.SaveChangesAsync();

        _context.DimProductos.Add(new Domain.Entities.DataWarehouse.DimProducto
        {
            EmpresaId = empresaId,
            ProductoId = entity.Id,
            Nombre = entity.Nombre,
            Categoria = entity.Categoria
        });
        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(ProductoDto dto, int empresaId)
    {
        var entity = await _context.Productos
            .FirstOrDefaultAsync(p => p.Id == dto.Id && p.EmpresaId == empresaId);
        if (entity == null) return false;

        entity.Nombre = dto.Nombre;
        entity.Categoria = dto.Categoria;
        entity.Precio = dto.Precio;
        entity.Activo = dto.Activo;

        var dim = await _context.DimProductos.FirstOrDefaultAsync(d => d.ProductoId == entity.Id);
        if (dim != null) { dim.Nombre = entity.Nombre; dim.Categoria = entity.Categoria; }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int empresaId)
    {
        var entity = await _context.Productos
            .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);
        if (entity == null) return false;
        _context.Productos.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
