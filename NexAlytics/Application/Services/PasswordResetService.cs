using Microsoft.EntityFrameworkCore;
using NexAlytics.Domain.Entities;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Application.Services;

public class PasswordResetService
{
    private readonly ApplicationDbContext _context;

    public PasswordResetService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Generates a password reset token for the given email within the given company.
    /// Returns the token string, or null if the email doesn't exist (caller should not reveal this).
    /// </summary>
    public async Task<(string? Token, string? UserName)> GenerateAsync(string email, int empresaId)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email && u.EmpresaId == empresaId);

        if (usuario == null)
            return (null, null);

        // Remove previous unused tokens for this user
        var old = _context.PasswordResetTokens.Where(t => t.UsuarioId == usuario.Id && !t.Used);
        _context.PasswordResetTokens.RemoveRange(old);

        var token = Guid.NewGuid().ToString("N");
        var entry = new PasswordResetToken
        {
            UsuarioId = usuario.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Used = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(entry);
        await _context.SaveChangesAsync();

        return (token, usuario.Nombre);
    }

    /// <summary>
    /// Validates the token. Returns (Valid, UserId, Email).
    /// </summary>
    public async Task<(bool Valid, int UserId, string Email)> ValidateAsync(string token)
    {
        var entry = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token && !t.Used && t.ExpiresAt > DateTime.UtcNow);

        if (entry == null)
            return (false, 0, string.Empty);

        var usuario = await _context.Usuarios.FindAsync(entry.UsuarioId);
        if (usuario == null)
            return (false, 0, string.Empty);

        return (true, usuario.Id, usuario.Email);
    }

    /// <summary>
    /// Consumes the token and updates the user's password.
    /// </summary>
    public async Task<bool> ConsumeAsync(string token, string newPassword)
    {
        var entry = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token && !t.Used && t.ExpiresAt > DateTime.UtcNow);

        if (entry == null)
            return false;

        var usuario = await _context.Usuarios.FindAsync(entry.UsuarioId);
        if (usuario == null)
            return false;

        entry.Used = true;
        usuario.PasswordHash = AuthService.ComputeHash(newPassword);

        await _context.SaveChangesAsync();
        return true;
    }
}
