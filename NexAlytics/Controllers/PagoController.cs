using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexAlytics.Application.DTOs;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

[Authorize]
public class PagoController : Controller
{
    private readonly PagoService _pagoService;
    private readonly VentaService _ventaService;

    public PagoController(PagoService pagoService, VentaService ventaService)
    {
        _pagoService = pagoService;
        _ventaService = ventaService;
    }

    private const int PageSize = 10;

    public async Task<IActionResult> Index(int page = 1)
    {
        page = Math.Max(1, page);
        var result = await _pagoService.GetPagedAsync(GetEmpresaId(), page, PageSize);

        ViewData["Page"] = result.Page;
        ViewData["TotalPages"] = result.TotalPages;
        ViewData["TotalCount"] = result.TotalCount;
        ViewData["PageSize"] = result.PageSize;
        return View(result);
    }

    public async Task<IActionResult> Create(int? ventaId)
    {
        await LoadVentasAsync();
        var dto = new PagoDto { Fecha = DateTime.Today };
        if (ventaId.HasValue) dto.VentaId = ventaId.Value;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PagoDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadVentasAsync();
            return View(dto);
        }
        try
        {
            await _pagoService.CreateAsync(dto, GetEmpresaId());
            TempData["Success"] = "Pago registrado exitosamente";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadVentasAsync();
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _pagoService.GetByIdAsync(id, GetEmpresaId());
        if (dto == null) return NotFound();
        await LoadVentasAsync();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PagoDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadVentasAsync();
            return View(dto);
        }
        var ok = await _pagoService.UpdateAsync(dto, GetEmpresaId());
        if (!ok) return NotFound();
        TempData["Success"] = "Pago actualizado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _pagoService.DeleteAsync(id, GetEmpresaId());
        TempData["Success"] = "Pago eliminado";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadVentasAsync()
    {
        var ventas = await _ventaService.GetAllAsync(GetEmpresaId());
        ViewData["VentaId"] = new SelectList(
            ventas.Select(v => new { v.Id, Display = $"#{v.Id} - {v.ClienteNombre} ({v.Total:C})" }),
            "Id", "Display");
    }

    private int GetEmpresaId()
    {
        var claim = User.FindFirst("EmpresaId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
