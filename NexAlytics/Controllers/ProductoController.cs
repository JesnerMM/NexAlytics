using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexAlytics.Application.DTOs;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

[Authorize]
public class ProductoController : Controller
{
    private readonly ProductoService _productoService;
    private readonly ExportService _exportService;
    private readonly AuditService _audit;

    public ProductoController(ProductoService productoService, ExportService exportService, AuditService audit)
    {
        _productoService = productoService;
        _exportService = exportService;
        _audit = audit;
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
        var created = await _productoService.CreateAsync(dto, GetEmpresaId());
        await _audit.LogAsync(GetEmpresaId(), "Crear", "Producto", created.Id, created.Nombre);
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
        await _audit.LogAsync(GetEmpresaId(), "Actualizar", "Producto", dto.Id, dto.Nombre);
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
        await _audit.LogAsync(GetEmpresaId(), "Eliminar", "Producto", id);
        TempData["Success"] = "Producto eliminado";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel(string? categoria, bool? activo)
    {
        var data = await _productoService.GetAllAsync(GetEmpresaId(), categoria, activo);
        var bytes = _exportService.ExportProductos(data);
        var fileName = $"Productos_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private int GetEmpresaId()
    {
        var claim = User.FindFirst("EmpresaId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
