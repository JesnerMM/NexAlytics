using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexAlytics.Application.DTOs;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

[Authorize]
public class VentaController : Controller
{
    private readonly VentaService _ventaService;
    private readonly ClienteService _clienteService;
    private readonly ExportService _exportService;

    public VentaController(VentaService ventaService, ClienteService clienteService, ExportService exportService)
    {
        _ventaService = ventaService;
        _clienteService = clienteService;
        _exportService = exportService;
    }

    private const int PageSize = 10;

    public async Task<IActionResult> Index(int page = 1)
    {
        page = Math.Max(1, page);
        var result = await _ventaService.GetPagedAsync(GetEmpresaId(), page, PageSize);

        ViewData["Page"] = result.Page;
        ViewData["TotalPages"] = result.TotalPages;
        ViewData["TotalCount"] = result.TotalCount;
        ViewData["PageSize"] = result.PageSize;
        return View(result);
    }

    public async Task<IActionResult> Create()
    {
        await LoadClientesAsync();
        return View(new VentaDto { Fecha = DateTime.Today, Estado = "Pendiente" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VentaDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadClientesAsync();
            return View(dto);
        }
        await _ventaService.CreateAsync(dto, GetEmpresaId());
        TempData["Success"] = "Venta creada exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _ventaService.GetByIdAsync(id, GetEmpresaId());
        if (dto == null) return NotFound();
        await LoadClientesAsync();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VentaDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadClientesAsync();
            return View(dto);
        }
        var ok = await _ventaService.UpdateAsync(dto, GetEmpresaId());
        if (!ok) return NotFound();
        TempData["Success"] = "Venta actualizada exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var dto = await _ventaService.GetByIdAsync(id, GetEmpresaId());
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _ventaService.DeleteAsync(id, GetEmpresaId());
        TempData["Success"] = "Venta eliminada";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var data = await _ventaService.GetAllAsync(GetEmpresaId());
        var bytes = _exportService.ExportVentas(data);
        var fileName = $"Ventas_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private async Task LoadClientesAsync()
    {
        var clientes = await _clienteService.GetAllAsync(GetEmpresaId());
        ViewData["ClienteId"] = new SelectList(clientes, "Id", "Nombre");
    }

    private int GetEmpresaId()
    {
        var claim = User.FindFirst("EmpresaId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
