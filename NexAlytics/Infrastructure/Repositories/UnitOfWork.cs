using NexAlytics.Domain.Entities;
using NexAlytics.Domain.Entities.Staging;
using NexAlytics.Domain.Entities.DataWarehouse;
using NexAlytics.Domain.Interfaces;
using NexAlytics.Infrastructure.Data;

namespace NexAlytics.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Empresas = new GenericRepository<Empresa>(context);
        Usuarios = new GenericRepository<Usuario>(context);
        Clientes = new GenericRepository<Cliente>(context);
        Productos = new GenericRepository<Producto>(context);
        Ventas = new GenericRepository<Venta>(context);
        Pagos = new GenericRepository<Pago>(context);
        ImportJobs = new GenericRepository<ImportJob>(context);
        StgClientes = new GenericRepository<StgCliente>(context);
        StgVentas = new GenericRepository<StgVenta>(context);
        StgProductos = new GenericRepository<StgProducto>(context);
        DimFechas = new GenericRepository<DimFecha>(context);
        DimClientes = new GenericRepository<DimCliente>(context);
        DimProductos = new GenericRepository<DimProducto>(context);
        FactVentas = new GenericRepository<FactVenta>(context);
        FactPagos = new GenericRepository<FactPago>(context);
    }

    public IRepository<Empresa> Empresas { get; }
    public IRepository<Usuario> Usuarios { get; }
    public IRepository<Cliente> Clientes { get; }
    public IRepository<Producto> Productos { get; }
    public IRepository<Venta> Ventas { get; }
    public IRepository<Pago> Pagos { get; }
    public IRepository<ImportJob> ImportJobs { get; }
    public IRepository<StgCliente> StgClientes { get; }
    public IRepository<StgVenta> StgVentas { get; }
    public IRepository<StgProducto> StgProductos { get; }
    public IRepository<DimFecha> DimFechas { get; }
    public IRepository<DimCliente> DimClientes { get; }
    public IRepository<DimProducto> DimProductos { get; }
    public IRepository<FactVenta> FactVentas { get; }
    public IRepository<FactPago> FactPagos { get; }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
