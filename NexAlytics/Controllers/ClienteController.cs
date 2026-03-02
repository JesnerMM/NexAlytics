using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexAlytics.Application.DTOs;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

[Authorize]
public class ClienteController : Controller
{
    private readonly ClienteService _clienteService;
    private readonly ExportService _exportService;
    private readonly AuditService _audit;

    public ClienteController(ClienteService clienteService, ExportService exportService, AuditService audit)
    {
        _clienteService = clienteService;
        _exportService = exportService;
        _audit = audit;
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
        var created = await _clienteService.CreateAsync(dto, GetEmpresaId());
        await _audit.LogAsync(GetEmpresaId(), "Crear", "Cliente", created.Id, created.Nombre);
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
        await _audit.LogAsync(GetEmpresaId(), "Actualizar", "Cliente", dto.Id, dto.Nombre);
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
        await _audit.LogAsync(GetEmpresaId(), "Eliminar", "Cliente", id);
        TempData["Success"] = "Cliente eliminado";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var data = await _clienteService.GetAllAsync(GetEmpresaId());
        var bytes = _exportService.ExportClientes(data);
        var fileName = $"Clientes_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private int GetEmpresaId()
    {
        var claim = User.FindFirst("EmpresaId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
