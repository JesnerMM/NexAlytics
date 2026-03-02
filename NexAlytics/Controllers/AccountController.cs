using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexAlytics.Application.DTOs;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _authService;
    private readonly UsuarioService _usuarioService;

    public AccountController(AuthService authService, UsuarioService usuarioService)
    {
        _authService = authService;
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Email y contraseña son requeridos");
            return View();
        }

        var principal = await _authService.LoginAsync(email, password);
        if (principal == null)
        {
            ModelState.AddModelError(string.Empty, "Credenciales inválidas");
            return View();
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [Authorize]
    [HttpGet]
    public IActionResult CambiarPassword() => View(new CambiarPasswordDto());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var (success, error) = await _usuarioService.ChangePasswordAsync(userId, dto.PasswordActual, dto.NuevaPassword);

        if (!success)
        {
            ModelState.AddModelError(nameof(dto.PasswordActual), error!);
            return View(dto);
        }

        TempData["Success"] = "Contraseña actualizada exitosamente";
        return RedirectToAction("Index", "Dashboard");
    }
}
