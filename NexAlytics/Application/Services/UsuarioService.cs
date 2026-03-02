using Microsoft.EntityFrameworkCore;
using NexAlytics.Application.DTOs;
using NexAlytics.Domain.Entities;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Application.Services;

public class UsuarioService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(ApplicationDbContext context, ILogger<UsuarioService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UsuarioDto>> GetAllAsync(int empresaId)
    {
        return await _context.Usuarios
            .Where(u => u.EmpresaId == empresaId)
            .Select(u => new UsuarioDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = u.Rol,
                FechaCreacion = u.FechaCreacion
            })
            .OrderBy(u => u.Nombre)
            .ToListAsync();
    }

    public async Task<PagedResult<UsuarioDto>> GetPagedAsync(int empresaId, int page, int pageSize)
    {
        var query = _context.Usuarios.Where(u => u.EmpresaId == empresaId);

        var total = await query.CountAsync();

        var items = await query
            .Select(u => new UsuarioDto { Id = u.Id, Nombre = u.Nombre, Email = u.Email, Rol = u.Rol, FechaCreacion = u.FechaCreacion })
            .OrderBy(u => u.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<UsuarioDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<UsuarioDto?> GetByIdAsync(int id, int empresaId)
    {
        return await _context.Usuarios
            .Where(u => u.Id == id && u.EmpresaId == empresaId)
            .Select(u => new UsuarioDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = u.Rol,
                FechaCreacion = u.FechaCreacion
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(UsuarioDto dto, int empresaId)
    {
        var emailExists = await _context.Usuarios
            .AnyAsync(u => u.Email == dto.Email && u.EmpresaId == empresaId);
        if (emailExists)
            return (false, "Ya existe un usuario con ese email");

        var entity = new Usuario
        {
            EmpresaId = empresaId,
            Nombre = dto.Nombre,
            Email = dto.Email,
            Rol = dto.Rol,
            PasswordHash = AuthService.ComputeHash(dto.Password!),
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(entity);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Usuario created: {Id} for empresa {EmpresaId}", entity.Id, empresaId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UsuarioDto dto, int empresaId)
    {
        var entity = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == dto.Id && u.EmpresaId == empresaId);
        if (entity == null)
            return (false, "Usuario no encontrado");

        var emailExists = await _context.Usuarios
            .AnyAsync(u => u.Email == dto.Email && u.EmpresaId == empresaId && u.Id != dto.Id);
        if (emailExists)
            return (false, "Ya existe un usuario con ese email");

        entity.Nombre = dto.Nombre;
        entity.Email = dto.Email;
        entity.Rol = dto.Rol;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var entity = await _context.Usuarios.FindAsync(userId);
        if (entity == null)
            return (false, "Usuario no encontrado");

        if (entity.PasswordHash != AuthService.ComputeHash(currentPassword))
            return (false, "La contraseña actual es incorrecta");

        entity.PasswordHash = AuthService.ComputeHash(newPassword);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, int empresaId, int currentUserId)
    {
        if (id == currentUserId)
            return (false, "No puedes eliminar tu propio usuario");

        var entity = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id && u.EmpresaId == empresaId);
        if (entity == null)
            return (false, "Usuario no encontrado");

        if (entity.Rol == "Admin")
        {
            var adminCount = await _context.Usuarios
                .CountAsync(u => u.EmpresaId == empresaId && u.Rol == "Admin");
            if (adminCount <= 1)
                return (false, "No puedes eliminar el último administrador");
        }

        _context.Usuarios.Remove(entity);
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
