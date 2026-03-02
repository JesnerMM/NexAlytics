using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexAlytics.Application.DTOs;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

[Authorize(Roles = "Admin")]
public class UsuarioController : Controller
{
    private readonly UsuarioService _usuarioService;

    public UsuarioController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _usuarioService.GetAllAsync(GetEmpresaId());
        return View(usuarios);
    }

    public IActionResult Create() => View(new UsuarioDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            ModelState.AddModelError(nameof(dto.Password), "La contraseña es obligatoria");

        if (!ModelState.IsValid) return View(dto);

        var (success, error) = await _usuarioService.CreateAsync(dto, GetEmpresaId());
        if (!success)
        {
            TempData["Error"] = error;
            return View(dto);
        }

        TempData["Success"] = "Usuario creado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _usuarioService.GetByIdAsync(id, GetEmpresaId());
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UsuarioDto dto)
    {
        // Remove password fields from validation on edit
        ModelState.Remove(nameof(dto.Password));
        ModelState.Remove(nameof(dto.ConfirmPassword));

        if (!ModelState.IsValid) return View(dto);

        var (success, error) = await _usuarioService.UpdateAsync(dto, GetEmpresaId());
        if (!success)
        {
            TempData["Error"] = error;
            return View(dto);
        }

        TempData["Success"] = "Usuario actualizado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var dto = await _usuarioService.GetByIdAsync(id, GetEmpresaId());
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _usuarioService.DeleteAsync(id, GetEmpresaId(), GetUsuarioId());
        if (!success)
            TempData["Error"] = error;
        else
            TempData["Success"] = "Usuario eliminado";

        return RedirectToAction(nameof(Index));
    }

    private int GetEmpresaId()
    {
        var claim = User.FindFirst("EmpresaId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }

    private int GetUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
