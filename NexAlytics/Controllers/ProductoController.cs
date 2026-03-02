using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexAlytics.Application.DTOs;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

[Authorize]
public class ProductoController : Controller
{
    private readonly ProductoService _productoService;

    public ProductoController(ProductoService productoService)
    {
        _productoService = productoService;
    }

    private const int PageSize = 10;

    public async Task<IActionResult> Index(string? categoria, bool? activo, int page = 1)
    {
        page = Math.Max(1, page);
        var empresaId = GetEmpresaId();
        var result = await _productoService.GetPagedAsync(empresaId, page, PageSize, categoria, activo);
        var categorias = await _productoService.GetCategoriasAsync(empresaId);

        ViewData["Categorias"] = categorias;
        ViewData["CategoriaFiltro"] = categoria;
        ViewData["ActivoFiltro"] = activo;
        ViewData["Page"] = result.Page;
        ViewData["TotalPages"] = result.TotalPages;
        ViewData["TotalCount"] = result.TotalCount;
        ViewData["PageSize"] = result.PageSize;
        return View(result);
    }

    public IActionResult Create() => View(new ProductoDto { Activo = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductoDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _productoService.CreateAsync(dto, GetEmpresaId());
        TempData["Success"] = "Producto creado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _productoService.GetByIdAsync(id, GetEmpresaId());
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductoDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var ok = await _productoService.UpdateAsync(dto, GetEmpresaId());
        if (!ok) return NotFound();
        TempData["Success"] = "Producto actualizado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var dto = await _productoService.GetByIdAsync(id, GetEmpresaId());
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _productoService.DeleteAsync(id, GetEmpresaId());
        TempData["Success"] = "Producto eliminado";
        return RedirectToAction(nameof(Index));
    }

    private int GetEmpresaId()
    {
        var claim = User.FindFirst("EmpresaId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
