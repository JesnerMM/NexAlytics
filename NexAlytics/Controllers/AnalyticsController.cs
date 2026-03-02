using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

[Authorize]
public class AnalyticsController : Controller
{
    private readonly DashboardService _dashboardService;
    private readonly PowerBiService _powerBiService;

    public AnalyticsController(DashboardService dashboardService, PowerBiService powerBiService)
    {
        _dashboardService = dashboardService;
        _powerBiService = powerBiService;
    }

    public async Task<IActionResult> Index()
    {
        var empresaId = GetEmpresaId();
        var pbiEmbed = await _powerBiService.GetEmbedTokenAsync();
        ViewData["PbiEmbed"] = pbiEmbed;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> VentasPorMes(int months = 12)
    {
        var data = await _dashboardService.GetVentasPorMesAsync(GetEmpresaId(), months);
        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> PagosPorMetodo()
    {
        var data = await _dashboardService.GetPagosPorMetodoAsync(GetEmpresaId());
        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> VentasPorCategoria()
    {
        var data = await _dashboardService.GetVentasPorCategoriaAsync(GetEmpresaId());
        return Json(data);
    }

    private int GetEmpresaId()
    {
        var claim = User.FindFirst("EmpresaId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
