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

    private const int PageSize = 10;

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        page = Math.Max(1, page);
        var result = await _clienteService.GetPagedAsync(GetEmpresaId(), page, PageSize, search);

        ViewData["Search"] = search;
        ViewData["Page"] = result.Page;
        ViewData["TotalPages"] = result.TotalPages;
        ViewData["TotalCount"] = result.TotalCount;
        ViewData["PageSize"] = result.PageSize;
        return View(result);
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
