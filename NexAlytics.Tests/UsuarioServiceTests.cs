using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NexAlytics.Application.Services;
using NexAlytics.Application.DTOs;
using NexAlytics.Domain.Entities;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Tests;

public class UsuarioServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UsuarioService _sut;
    private const int EmpresaId = 1;

    public UsuarioServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _sut = new UsuarioService(_context, NullLogger<UsuarioService>.Instance);
    }

    public void Dispose() => _context.Dispose();

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Usuario> SeedAdminAsync(string email = "admin@test.com")
    {
        var u = new Usuario
        {
            EmpresaId    = EmpresaId,
            Nombre       = "Admin",
            Email        = email,
            PasswordHash = AuthService.ComputeHash("Password1!"),
            Rol          = "Admin",
            FechaCreacion = DateTime.UtcNow
        };
        _context.Usuarios.Add(u);
        await _context.SaveChangesAsync();
        return u;
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyOwnTenant()
    {
        await SeedAdminAsync();
        _context.Usuarios.Add(new Usuario
        {
            EmpresaId    = 99,
            Nombre       = "Other",
            Email        = "other@other.com",
            PasswordHash = AuthService.ComputeHash("x"),
            Rol          = "Admin",
            FechaCreacion = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var list = await _sut.GetAllAsync(EmpresaId);

        Assert.Single(list);
        Assert.Equal("admin@test.com", list[0].Email);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidDto_AddsUser()
    {
        var dto = new UsuarioDto
        {
            Nombre   = "Juan",
            Email    = "juan@test.com",
            Rol      = "Usuario",
            Password = "Password1!"
        };

        var (success, error) = await _sut.CreateAsync(dto, EmpresaId);

        Assert.True(success);
        Assert.Null(error);
        Assert.Equal(1, await _context.Usuarios.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ReturnsError()
    {
        await SeedAdminAsync("dup@test.com");

        var dto = new UsuarioDto
        {
            Nombre   = "Otro",
            Email    = "dup@test.com",
            Rol      = "Usuario",
            Password = "Password1!"
        };

        var (success, error) = await _sut.CreateAsync(dto, EmpresaId);

        Assert.False(success);
        Assert.NotNull(error);
        Assert.Equal(1, await _context.Usuarios.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_StoresHashedPassword()
    {
        var dto = new UsuarioDto
        {
            Nombre   = "Test",
            Email    = "hash@test.com",
            Rol      = "Usuario",
            Password = "Password1!"
        };

        await _sut.CreateAsync(dto, EmpresaId);

        var stored = await _context.Usuarios.FirstAsync();
        Assert.NotEqual("Password1!", stored.PasswordHash);
        Assert.Equal(AuthService.ComputeHash("Password1!"), stored.PasswordHash);
    }

    // ── ChangePasswordAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_CorrectCurrentPassword_Succeeds()
    {
        var admin = await SeedAdminAsync();

        var (ok, error) = await _sut.ChangePasswordAsync(admin.Id, "Password1!", "NewPass1!");

        Assert.True(ok);
        Assert.Null(error);
        var stored = await _context.Usuarios.FindAsync(admin.Id);
        Assert.Equal(AuthService.ComputeHash("NewPass1!"), stored!.PasswordHash);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_Fails()
    {
        var admin = await SeedAdminAsync();

        var (ok, error) = await _sut.ChangePasswordAsync(admin.Id, "WrongPass!", "NewPass1!");

        Assert.False(ok);
        Assert.NotNull(error);
        var stored = await _context.Usuarios.FindAsync(admin.Id);
        Assert.Equal(AuthService.ComputeHash("Password1!"), stored!.PasswordHash);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_LastAdmin_ReturnsError()
    {
        var admin = await SeedAdminAsync();

        var (ok, error) = await _sut.DeleteAsync(admin.Id, EmpresaId, currentUserId: 999);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Equal(1, await _context.Usuarios.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_CurrentUser_ReturnsError()
    {
        var admin = await SeedAdminAsync();
        _context.Usuarios.Add(new Usuario
        {
            EmpresaId    = EmpresaId,
            Nombre       = "Admin2",
            Email        = "admin2@test.com",
            PasswordHash = AuthService.ComputeHash("x"),
            Rol          = "Admin",
            FechaCreacion = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var (ok, error) = await _sut.DeleteAsync(admin.Id, EmpresaId, currentUserId: admin.Id);

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task DeleteAsync_ValidTarget_RemovesUser()
    {
        var admin = await SeedAdminAsync();
        var dto = new UsuarioDto
        {
            Nombre   = "ToDelete",
            Email    = "del@test.com",
            Rol      = "Usuario",
            Password = "Pass1!"
        };
        await _sut.CreateAsync(dto, EmpresaId);
        var toDelete = await _context.Usuarios.FirstAsync(u => u.Email == "del@test.com");

        var (ok, error) = await _sut.DeleteAsync(toDelete.Id, EmpresaId, currentUserId: admin.Id);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(1, await _context.Usuarios.CountAsync());
    }
}
