using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using NexAlytics.Application.DTOs;
using NexAlytics.Domain.Entities;
using NexAlytics.Domain.Entities.Staging;
using NexAlytics.Infrastructure.Data;
using OfficeOpenXml;
using System.Globalization;

namespace NexAlytics.Application.Services;

public class ImportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ImportService> _logger;

    public ImportService(ApplicationDbContext context, ILogger<ImportService> logger)
    {
        _context = context;
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<ImportPreviewDto> ParseAndPreviewAsync(IFormFile file, string tipo, int empresaId)
    {
        var preview = new ImportPreviewDto
        {
            Tipo = tipo,
            ArchivoNombre = file.FileName
        };

        var rows = await ParseFileAsync(file);
        preview.Columns = rows.Count > 0 ? rows[0].Keys.ToList() : new List<string>();
        preview.TotalRegistros = rows.Count;

        var requiredColumns = GetRequiredColumns(tipo);
        var missingCols = requiredColumns.Except(preview.Columns, StringComparer.OrdinalIgnoreCase).ToList();

        if (missingCols.Any())
        {
            preview.Errors.Add($"Columnas faltantes: {string.Join(", ", missingCols)}");
            return preview;
        }

        foreach (var row in rows)
        {
            var rowErrors = ValidateRow(row, tipo);
            if (rowErrors.Any())
            {
                preview.RegistrosError++;
                preview.Errors.AddRange(rowErrors.Select(e => $"Fila: {e}"));
            }
            else
            {
                preview.RegistrosValidos++;
            }
        }

        // Store in session-like approach: create a temp job
        var job = new ImportJob
        {
            EmpresaId = empresaId,
            Tipo = tipo,
            ArchivoNombre = file.FileName,
            Estado = "Preview",
            Registros = rows.Count
        };
        _context.ImportJobs.Add(job);
        await _context.SaveChangesAsync();

        // Insert staging records
        await InsertStagingAsync(rows, tipo, job.Id);
        await _context.SaveChangesAsync();

        preview.ImportJobId = job.Id;
        preview.Rows = rows.Take(20).ToList();
        return preview;
    }

    public async Task<bool> ConfirmImportAsync(int importJobId, int empresaId)
    {
        var job = await _context.ImportJobs
            .FirstOrDefaultAsync(j => j.Id == importJobId && j.EmpresaId == empresaId);
        if (job == null) return false;

        job.Estado = "Pendiente";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ImportJobDto>> GetHistoryAsync(int empresaId)
    {
        return await _context.ImportJobs
            .Where(j => j.EmpresaId == empresaId && j.Estado != "Preview")
            .OrderByDescending(j => j.Fecha)
            .Select(j => new ImportJobDto
            {
                Id = j.Id,
                Tipo = j.Tipo,
                ArchivoNombre = j.ArchivoNombre,
                Estado = j.Estado,
                Registros = j.Registros,
                RegistrosProcesados = j.RegistrosProcesados,
                RegistrosError = j.RegistrosError,
                Fecha = j.Fecha
            })
            .ToListAsync();
    }

    public async Task<bool> ReprocessAsync(int importJobId, int empresaId)
    {
        var job = await _context.ImportJobs
            .FirstOrDefaultAsync(j => j.Id == importJobId && j.EmpresaId == empresaId);
        if (job == null) return false;

        // Reset staging records for this job
        if (job.Tipo == "Clientes")
        {
            var stg = await _context.StgClientes.Where(s => s.ImportJobId == importJobId).ToListAsync();
            stg.ForEach(s => { s.Procesado = false; s.Error = null; });
        }
        else if (job.Tipo == "Ventas")
        {
            var stg = await _context.StgVentas.Where(s => s.ImportJobId == importJobId).ToListAsync();
            stg.ForEach(s => { s.Procesado = false; s.Error = null; });
        }
        else if (job.Tipo == "Productos")
        {
            var stg = await _context.StgProductos.Where(s => s.ImportJobId == importJobId).ToListAsync();
            stg.ForEach(s => { s.Procesado = false; s.Error = null; });
        }

        job.Estado = "Pendiente";
        job.RegistrosProcesados = 0;
        job.RegistrosError = 0;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<List<Dictionary<string, string>>> ParseFileAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext == ".csv") return await ParseCsvAsync(file);
        if (ext is ".xlsx" or ".xls") return await ParseExcelAsync(file);
        throw new NotSupportedException($"Formato no soportado: {ext}");
    }

    private static async Task<List<Dictionary<string, string>>> ParseCsvAsync(IFormFile file)
    {
        var rows = new List<Dictionary<string, string>>();
        using var reader = new StreamReader(file.OpenReadStream());
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null };
        using var csv = new CsvReader(reader, config);

        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? Array.Empty<string>();

        while (await csv.ReadAsync())
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var h in headers)
                row[h] = csv.GetField(h) ?? string.Empty;
            rows.Add(row);
        }
        return rows;
    }

    private static async Task<List<Dictionary<string, string>>> ParseExcelAsync(IFormFile file)
    {
        var rows = new List<Dictionary<string, string>>();
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var pkg = new ExcelPackage(stream);
        var ws = pkg.Workbook.Worksheets[0];
        if (ws.Dimension == null) return rows;

        var headers = new List<string>();
        for (int c = 1; c <= ws.Dimension.Columns; c++)
            headers.Add(ws.Cells[1, c].Text.Trim());

        for (int r = 2; r <= ws.Dimension.Rows; r++)
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < headers.Count; c++)
                row[headers[c]] = ws.Cells[r, c + 1].Text.Trim();
            rows.Add(row);
        }
        return rows;
    }

    private static List<string> GetRequiredColumns(string tipo) => tipo switch
    {
        "Clientes" => new List<string> { "Nombre", "Email", "Telefono" },
        "Ventas" => new List<string> { "ClienteNombre", "Fecha", "Total", "Estado" },
        "Productos" => new List<string> { "Nombre", "Categoria", "Precio" },
        _ => new List<string>()
    };

    private static List<string> ValidateRow(Dictionary<string, string> row, string tipo)
    {
        var errors = new List<string>();
        if (tipo == "Clientes")
        {
            if (string.IsNullOrWhiteSpace(row.GetValueOrDefault("Nombre"))) errors.Add("Nombre requerido");
        }
        else if (tipo == "Ventas")
        {
            if (!DateTime.TryParse(row.GetValueOrDefault("Fecha"), out _)) errors.Add("Fecha inválida");
            if (!decimal.TryParse(row.GetValueOrDefault("Total"), NumberStyles.Any, CultureInfo.InvariantCulture, out _)) errors.Add("Total inválido");
        }
        else if (tipo == "Productos")
        {
            if (string.IsNullOrWhiteSpace(row.GetValueOrDefault("Nombre"))) errors.Add("Nombre requerido");
            if (!decimal.TryParse(row.GetValueOrDefault("Precio"), NumberStyles.Any, CultureInfo.InvariantCulture, out _)) errors.Add("Precio inválido");
        }
        return errors;
    }

    private async Task InsertStagingAsync(List<Dictionary<string, string>> rows, string tipo, int jobId)
    {
        if (tipo == "Clientes")
        {
            foreach (var row in rows)
            {
                _context.StgClientes.Add(new StgCliente
                {
                    ImportJobId = jobId,
                    Nombre = row.GetValueOrDefault("Nombre", string.Empty),
                    Email = row.GetValueOrDefault("Email", string.Empty),
                    Telefono = row.GetValueOrDefault("Telefono", string.Empty)
                });
            }
        }
        else if (tipo == "Ventas")
        {
            foreach (var row in rows)
            {
                decimal.TryParse(row.GetValueOrDefault("Total", "0"), NumberStyles.Any, CultureInfo.InvariantCulture, out var total);
                _context.StgVentas.Add(new StgVenta
                {
                    ImportJobId = jobId,
                    ClienteNombre = row.GetValueOrDefault("ClienteNombre", string.Empty),
                    Fecha = row.GetValueOrDefault("Fecha", string.Empty),
                    Total = total,
                    Estado = row.GetValueOrDefault("Estado", "Pendiente")
                });
            }
        }
        else if (tipo == "Productos")
        {
            foreach (var row in rows)
            {
                _context.StgProductos.Add(new StgProducto
                {
                    ImportJobId = jobId,
                    Nombre = row.GetValueOrDefault("Nombre", string.Empty),
                    Categoria = row.GetValueOrDefault("Categoria", string.Empty),
                    Precio = row.GetValueOrDefault("Precio", "0")
                });
            }
        }
        await Task.CompletedTask;
    }
}
