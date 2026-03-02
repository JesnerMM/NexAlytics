using System.Drawing;
using NexAlytics.Application.DTOs;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace NexAlytics.Application.Services;

public class ExportService
{
    private static readonly Color HeaderBg = ColorTranslator.FromHtml("#1e40af");
    private static readonly Color HeaderFg = Color.White;
    private static readonly Color AltRowBg = ColorTranslator.FromHtml("#f0f4ff");

    static ExportService()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public byte[] ExportClientes(List<ClienteDto> data)
    {
        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Clientes");

        string[] headers = ["#", "Nombre", "Email", "Teléfono", "Fecha Registro", "Nº Ventas", "Total Ventas"];
        WriteHeaders(ws, headers);

        for (int i = 0; i < data.Count; i++)
        {
            var c = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = c.Id;
            ws.Cells[row, 2].Value = c.Nombre;
            ws.Cells[row, 3].Value = c.Email;
            ws.Cells[row, 4].Value = c.Telefono;
            ws.Cells[row, 5].Value = c.FechaRegistro;
            ws.Cells[row, 5].Style.Numberformat.Format = "dd/mm/yyyy";
            ws.Cells[row, 6].Value = c.VentasCount;
            ws.Cells[row, 7].Value = c.TotalVentas;
            ws.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
            if (i % 2 == 1) ApplyAltRow(ws, row, headers.Length);
        }

        Finalize(ws, headers.Length, data.Count);
        return pkg.GetAsByteArray();
    }

    public byte[] ExportProductos(List<ProductoDto> data)
    {
        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Productos");

        string[] headers = ["#", "Nombre", "Categoría", "Precio", "Estado"];
        WriteHeaders(ws, headers);

        for (int i = 0; i < data.Count; i++)
        {
            var p = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = p.Id;
            ws.Cells[row, 2].Value = p.Nombre;
            ws.Cells[row, 3].Value = p.Categoria;
            ws.Cells[row, 4].Value = p.Precio;
            ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 5].Value = p.Activo ? "Activo" : "Inactivo";
            if (i % 2 == 1) ApplyAltRow(ws, row, headers.Length);
        }

        Finalize(ws, headers.Length, data.Count);
        return pkg.GetAsByteArray();
    }

    public byte[] ExportVentas(List<VentaDto> data)
    {
        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Ventas");

        string[] headers = ["#", "Cliente", "Fecha", "Total", "Estado"];
        WriteHeaders(ws, headers);

        for (int i = 0; i < data.Count; i++)
        {
            var v = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = v.Id;
            ws.Cells[row, 2].Value = v.ClienteNombre;
            ws.Cells[row, 3].Value = v.Fecha;
            ws.Cells[row, 3].Style.Numberformat.Format = "dd/mm/yyyy";
            ws.Cells[row, 4].Value = v.Total;
            ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 5].Value = v.Estado;
            if (i % 2 == 1) ApplyAltRow(ws, row, headers.Length);
        }

        Finalize(ws, headers.Length, data.Count);
        return pkg.GetAsByteArray();
    }

    public byte[] ExportPagos(List<PagoDto> data)
    {
        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Pagos");

        string[] headers = ["#", "Venta", "Método", "Monto", "Fecha"];
        WriteHeaders(ws, headers);

        for (int i = 0; i < data.Count; i++)
        {
            var p = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = p.Id;
            ws.Cells[row, 2].Value = p.VentaInfo;
            ws.Cells[row, 3].Value = p.Metodo;
            ws.Cells[row, 4].Value = p.Monto;
            ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 5].Value = p.Fecha;
            ws.Cells[row, 5].Style.Numberformat.Format = "dd/mm/yyyy";
            if (i % 2 == 1) ApplyAltRow(ws, row, headers.Length);
        }

        Finalize(ws, headers.Length, data.Count);
        return pkg.GetAsByteArray();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static void WriteHeaders(ExcelWorksheet ws, string[] headers)
    {
        for (int col = 1; col <= headers.Length; col++)
        {
            var cell = ws.Cells[1, col];
            cell.Value = headers[col - 1];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(HeaderFg);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(HeaderBg);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
    }

    private static void ApplyAltRow(ExcelWorksheet ws, int row, int cols)
    {
        var range = ws.Cells[row, 1, row, cols];
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(AltRowBg);
    }

    private static void Finalize(ExcelWorksheet ws, int cols, int dataRows)
    {
        // Border on header
        var headerRange = ws.Cells[1, 1, 1, cols];
        headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
        headerRange.Style.Border.Bottom.Color.SetColor(Color.White);

        // Auto-fit columns
        if (dataRows > 0)
            ws.Cells[1, 1, dataRows + 1, cols].AutoFitColumns(10, 50);
        else
            ws.Cells[1, 1, 1, cols].AutoFitColumns(10, 50);

        // Freeze header row
        ws.View.FreezePanes(2, 1);
    }
}
