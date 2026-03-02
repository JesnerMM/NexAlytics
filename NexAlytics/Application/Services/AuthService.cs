using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using NexAlytics.Domain.Interfaces;

namespace NexAlytics.Application.Services;

public class AuthService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork uow, ILogger<AuthService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal?> LoginAsync(string email, string password)
    {
        var hash = ComputeHash(password);
        var users = await _uow.Usuarios.FindAsync(u => u.Email == email && u.PasswordHash == hash);
        var usuario = users.FirstOrDefault();

        if (usuario == null)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", email);
            return null;
        }

        var empresas = await _uow.Empresas.FindAsync(e => e.Id == usuario.EmpresaId && e.Activo);
        if (!empresas.Any())
        {
            _logger.LogWarning("Inactive company for user: {Email}", email);
            return null;
        }

        var empresa = empresas.First();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol),
            new("EmpresaId", usuario.EmpresaId.ToString()),
            new("EmpresaNombre", empresa.Nombre)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    public static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }
}
