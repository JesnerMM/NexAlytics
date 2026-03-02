using Microsoft.EntityFrameworkCore;
using NexAlytics.Application.DTOs;
using NexAlytics.Domain.Entities;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Application.Services;

public class ClienteService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(ApplicationDbContext context, ILogger<ClienteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ClienteDto>> GetAllAsync(int empresaId)
    {
        return await _context.Clientes
            .Where(c => c.EmpresaId == empresaId)
            .Select(c => new ClienteDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Email = c.Email,
                Telefono = c.Telefono,
                FechaRegistro = c.FechaRegistro,
                VentasCount = c.Ventas.Count,
                TotalVentas = c.Ventas.Sum(v => v.Total)
            })
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<ClienteDto?> GetByIdAsync(int id, int empresaId)
    {
        return await _context.Clientes
            .Where(c => c.Id == id && c.EmpresaId == empresaId)
            .Select(c => new ClienteDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Email = c.Email,
                Telefono = c.Telefono,
                FechaRegistro = c.FechaRegistro,
                VentasCount = c.Ventas.Count,
                TotalVentas = c.Ventas.Sum(v => v.Total)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ClienteDto> CreateAsync(ClienteDto dto, int empresaId)
    {
        var entity = new Cliente
        {
            EmpresaId = empresaId,
            Nombre = dto.Nombre,
            Email = dto.Email,
            Telefono = dto.Telefono,
            FechaRegistro = DateTime.UtcNow
        };
        _context.Clientes.Add(entity);
        await _context.SaveChangesAsync();

        _context.DimClientes.Add(new Domain.Entities.DataWarehouse.DimCliente
        {
            EmpresaId = empresaId,
            ClienteId = entity.Id,
            Nombre = entity.Nombre
        });
        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.FechaRegistro = entity.FechaRegistro;
        _logger.LogInformation("Cliente created: {Id} for empresa {EmpresaId}", entity.Id, empresaId);
        return dto;
    }

    public async Task<bool> UpdateAsync(ClienteDto dto, int empresaId)
    {
        var entity = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == dto.Id && c.EmpresaId == empresaId);
        if (entity == null) return false;

        entity.Nombre = dto.Nombre;
        entity.Email = dto.Email;
        entity.Telefono = dto.Telefono;

        var dim = await _context.DimClientes.FirstOrDefaultAsync(d => d.ClienteId == entity.Id);
        if (dim != null) dim.Nombre = entity.Nombre;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int empresaId)
    {
        var entity = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);
        if (entity == null) return false;

        _context.Clientes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
