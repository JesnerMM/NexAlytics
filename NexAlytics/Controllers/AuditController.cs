using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

[Authorize(Roles = "Admin")]
public class AuditController : Controller
{
    private readonly AuditService _auditService;
    private const int PageSize = 20;

    public AuditController(AuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task<IActionResult> Index(string? entidad, int page = 1)
    {
        page = Math.Max(1, page);
        var result = await _auditService.GetPagedAsync(GetEmpresaId(), page, PageSize, entidad);

        ViewData["EntidadFiltro"] = entidad;
        ViewData["Page"] = result.Page;
        ViewData["TotalPages"] = result.TotalPages;
        ViewData["TotalCount"] = result.TotalCount;
        ViewData["PageSize"] = result.PageSize;
        return View(result);
    }

    private int GetEmpresaId()
    {
        var claim = User.FindFirst("EmpresaId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
