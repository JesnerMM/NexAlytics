using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexAlytics.Application.Services;

namespace NexAlytics.Controllers;

[Authorize]
public class ImportController : Controller
{
    private readonly ImportService _importService;

    public ImportController(ImportService importService)
    {
        _importService = importService;
    }

    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, string tipo)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Seleccione un archivo";
            return RedirectToAction(nameof(Index));
        }

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext is not (".csv" or ".xlsx" or ".xls"))
        {
            TempData["Error"] = "Solo se permiten archivos CSV o Excel";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var preview = await _importService.ParseAndPreviewAsync(file, tipo, GetEmpresaId());
            return View("Preview", preview);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al procesar el archivo: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int importJobId)
    {
        var ok = await _importService.ConfirmImportAsync(importJobId, GetEmpresaId());
        if (ok)
            TempData["Success"] = "Importación confirmada. Se procesará en segundo plano.";
        else
            TempData["Error"] = "No se pudo confirmar la importación";

        return RedirectToAction(nameof(History));
    }

    public async Task<IActionResult> History()
    {
        var history = await _importService.GetHistoryAsync(GetEmpresaId());
        return View(history);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reprocess(int id)
    {
        var ok = await _importService.ReprocessAsync(id, GetEmpresaId());
        TempData[ok ? "Success" : "Error"] = ok ? "Importación reprocesada" : "Error al reprocesar";
        return RedirectToAction(nameof(History));
    }

    private int GetEmpresaId()
    {
        var claim = User.FindFirst("EmpresaId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
