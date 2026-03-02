using NexAlytics.Domain.Entities;
using NexAlytics.Domain.Entities.Staging;
using NexAlytics.Domain.Entities.DataWarehouse;

namespace NexAlytics.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Empresa> Empresas { get; }
    IRepository<Usuario> Usuarios { get; }
    IRepository<Cliente> Clientes { get; }
    IRepository<Producto> Productos { get; }
    IRepository<Venta> Ventas { get; }
    IRepository<Pago> Pagos { get; }
    IRepository<ImportJob> ImportJobs { get; }
    IRepository<StgCliente> StgClientes { get; }
    IRepository<StgVenta> StgVentas { get; }
    IRepository<StgProducto> StgProductos { get; }
    IRepository<DimFecha> DimFechas { get; }
    IRepository<DimCliente> DimClientes { get; }
    IRepository<DimProducto> DimProductos { get; }
    IRepository<FactVenta> FactVentas { get; }
    IRepository<FactPago> FactPagos { get; }

    Task<int> SaveChangesAsync();
}
