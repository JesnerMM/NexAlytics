using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexAlytics.Application.DTOs;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

[Authorize]
public class ClienteController : Controller
{
    private readonly ClienteService _clienteService;

    public ClienteController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var empresaId = GetEmpresaId();
        var clientes = await _clienteService.GetAllAsync(empresaId);

        if (!string.IsNullOrWhiteSpace(search))
            clientes = clientes.Where(c =>
                c.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        ViewData["Search"] = search;
        return View(clientes);
    }

    public IActionResult Create() => View(new ClienteDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _clienteService.CreateAsync(dto, GetEmpresaId());
        TempData["Success"] = "Cliente creado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _clienteService.GetByIdAsync(id, GetEmpresaId());
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClienteDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var ok = await _clienteService.UpdateAsync(dto, GetEmpresaId());
        if (!ok) return NotFound();
        TempData["Success"] = "Cliente actualizado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var dto = await _clienteService.GetByIdAsync(id, GetEmpresaId());
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _clienteService.DeleteAsync(id, GetEmpresaId());
        TempData["Success"] = "Cliente eliminado";
        return RedirectToAction(nameof(Index));
    }

    private int GetEmpresaId()
    {
        var claim = User.FindFirst("EmpresaId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
