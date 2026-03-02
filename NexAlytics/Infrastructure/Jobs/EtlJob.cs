using Microsoft.EntityFrameworkCore;
using NexAlytics.Domain.Entities;
using NexAlytics.Domain.Entities.DataWarehouse;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Infrastructure.Jobs;

public class EtlJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EtlJob> _logger;

    public EtlJob(ApplicationDbContext context, ILogger<EtlJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("ETL Job started at {Time}", DateTime.UtcNow);

        await ProcessStgClientesAsync();
        await ProcessStgProductosAsync();
        await ProcessStgVentasAsync();

        _logger.LogInformation("ETL Job completed at {Time}", DateTime.UtcNow);
    }

    private async Task ProcessStgClientesAsync()
    {
        var pending = await _context.StgClientes
            .Where(x => !x.Procesado)
            .ToListAsync();

        foreach (var stg in pending)
        {
            try
            {
                var job = await _context.ImportJobs.FindAsync(stg.ImportJobId);
                if (job == null) continue;

                var existingCliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Email == stg.Email && c.EmpresaId == job.EmpresaId);

                if (existingCliente == null)
                {
                    var cliente = new Cliente
                    {
                        EmpresaId = job.EmpresaId,
                        Nombre = stg.Nombre,
                        Email = stg.Email,
                        Telefono = stg.Telefono,
                        FechaRegistro = DateTime.UtcNow
                    };
                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();

                    _context.DimClientes.Add(new DimCliente
                    {
                        EmpresaId = job.EmpresaId,
                        ClienteId = cliente.Id,
                        Nombre = cliente.Nombre
                    });
                }
                else
                {
                    existingCliente.Nombre = stg.Nombre;
                    existingCliente.Telefono = stg.Telefono;

                    var dimCliente = await _context.DimClientes
                        .FirstOrDefaultAsync(d => d.ClienteId == existingCliente.Id);
                    if (dimCliente != null) dimCliente.Nombre = stg.Nombre;
                }

                stg.Procesado = true;

                if (job != null)
                {
                    job.RegistrosProcesados++;
                    if (job.RegistrosProcesados + job.RegistrosError >= job.Registros)
                        job.Estado = "Completado";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing StgCliente {Id}", stg.Id);
                stg.Error = ex.Message;
                stg.Procesado = true;

                var job = await _context.ImportJobs.FindAsync(stg.ImportJobId);
                if (job != null) job.RegistrosError++;

                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task ProcessStgProductosAsync()
    {
        var pending = await _context.StgProductos
            .Where(x => !x.Procesado)
            .ToListAsync();

        foreach (var stg in pending)
        {
            try
            {
                var job = await _context.ImportJobs.FindAsync(stg.ImportJobId);
                if (job == null) continue;

                if (!decimal.TryParse(stg.Precio, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var precio))
                {
                    stg.Error = $"Precio inválido: {stg.Precio}";
                    stg.Procesado = true;
                    job.RegistrosError++;
                    await _context.SaveChangesAsync();
                    continue;
                }

                var existing = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Nombre == stg.Nombre && p.EmpresaId == job.EmpresaId);

                if (existing == null)
                {
                    var producto = new Producto
                    {
                        EmpresaId = job.EmpresaId,
                        Nombre = stg.Nombre,
                        Categoria = stg.Categoria,
                        Precio = precio,
                        Activo = true
                    };
                    _context.Productos.Add(producto);
                    await _context.SaveChangesAsync();

                    _context.DimProductos.Add(new DimProducto
                    {
                        EmpresaId = job.EmpresaId,
                        ProductoId = producto.Id,
                        Nombre = producto.Nombre,
                        Categoria = producto.Categoria
                    });
                }
                else
                {
                    existing.Precio = precio;
                    existing.Categoria = stg.Categoria;
                }

                stg.Procesado = true;
                job.RegistrosProcesados++;
                if (job.RegistrosProcesados + job.RegistrosError >= job.Registros)
                    job.Estado = "Completado";

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing StgProducto {Id}", stg.Id);
                stg.Error = ex.Message;
                stg.Procesado = true;

                var job = await _context.ImportJobs.FindAsync(stg.ImportJobId);
                if (job != null) job.RegistrosError++;

                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task ProcessStgVentasAsync()
    {
        var pending = await _context.StgVentas
            .Where(x => !x.Procesado)
            .ToListAsync();

        foreach (var stg in pending)
        {
            try
            {
                var job = await _context.ImportJobs.FindAsync(stg.ImportJobId);
                if (job == null) continue;

                if (!DateTime.TryParse(stg.Fecha, out var fecha))
                {
                    stg.Error = $"Fecha inválida: {stg.Fecha}";
                    stg.Procesado = true;
                    job.RegistrosError++;
                    await _context.SaveChangesAsync();
                    continue;
                }

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Nombre == stg.ClienteNombre && c.EmpresaId == job.EmpresaId);

                if (cliente == null)
                {
                    stg.Error = $"Cliente no encontrado: {stg.ClienteNombre}";
                    stg.Procesado = true;
                    job.RegistrosError++;
                    await _context.SaveChangesAsync();
                    continue;
                }

                var venta = new Venta
                {
                    EmpresaId = job.EmpresaId,
                    ClienteId = cliente.Id,
                    Fecha = fecha,
                    Total = stg.Total,
                    Estado = stg.Estado
                };
                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                // DW
                var dimFecha = await EnsureDimFechaAsync(fecha);
                var dimCliente = await _context.DimClientes
                    .FirstOrDefaultAsync(d => d.ClienteId == cliente.Id);

                if (dimCliente != null)
                {
                    _context.FactVentas.Add(new FactVenta
                    {
                        EmpresaId = job.EmpresaId,
                        FechaId = dimFecha.Id,
                        ClienteDimId = dimCliente.Id,
                        VentaId = venta.Id,
                        Total = venta.Total,
                        Cantidad = 1
                    });
                }

                stg.Procesado = true;
                job.RegistrosProcesados++;
                if (job.RegistrosProcesados + job.RegistrosError >= job.Registros)
                    job.Estado = "Completado";

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing StgVenta {Id}", stg.Id);
                stg.Error = ex.Message;
                stg.Procesado = true;

                var job = await _context.ImportJobs.FindAsync(stg.ImportJobId);
                if (job != null) job.RegistrosError++;

                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task<DimFecha> EnsureDimFechaAsync(DateTime fecha)
    {
        var dateOnly = fecha.Date;
        var existing = await _context.DimFechas.FirstOrDefaultAsync(d => d.Fecha == dateOnly);
        if (existing != null) return existing;

        var mes = dateOnly.Month;
        var trimestre = (mes - 1) / 3 + 1;
        var nombreMes = dateOnly.ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("es-ES"));

        var dim = new DimFecha
        {
            Fecha = dateOnly,
            Año = dateOnly.Year,
            Mes = mes,
            NombreMes = char.ToUpper(nombreMes[0]) + nombreMes[1..],
            Trimestre = trimestre
        };
        _context.DimFechas.Add(dim);
        await _context.SaveChangesAsync();
        return dim;
    }
}
