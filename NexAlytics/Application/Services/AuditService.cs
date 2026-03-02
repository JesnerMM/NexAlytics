using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexAlytics.Application.DTOs;
using NexAlytics.Domain.Entities;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Application.Services;

public class AuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(int empresaId, string accion, string entidad, int? entidadId = null, string? detalle = null)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId);
        var usuarioNombre = user?.Identity?.Name ?? "Sistema";

        _context.AuditLogs.Add(new AuditLog
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            UsuarioNombre = usuarioNombre,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            Detalle = detalle,
            Fecha = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(int empresaId, int page, int pageSize, string? entidad = null)
    {
        var query = _context.AuditLogs.Where(a => a.EmpresaId == empresaId);

        if (!string.IsNullOrEmpty(entidad))
            query = query.Where(a => a.Entidad == entidad);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLog> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }
}
