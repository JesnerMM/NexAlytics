using Microsoft.EntityFrameworkCore;
using NexAlytics.Domain.Entities;
using NexAlytics.Domain.Entities.Staging;
using NexAlytics.Domain.Entities.DataWarehouse;

namespace NexAlytics.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Operational tables
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Staging tables
    public DbSet<StgCliente> StgClientes => Set<StgCliente>();
    public DbSet<StgVenta> StgVentas => Set<StgVenta>();
    public DbSet<StgProducto> StgProductos => Set<StgProducto>();

    // Data warehouse
    public DbSet<DimFecha> DimFechas => Set<DimFecha>();
    public DbSet<DimCliente> DimClientes => Set<DimCliente>();
    public DbSet<DimProducto> DimProductos => Set<DimProducto>();
    public DbSet<FactVenta> FactVentas => Set<FactVenta>();
    public DbSet<FactPago> FactPagos => Set<FactPago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Empresa
        modelBuilder.Entity<Empresa>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        });

        // Usuario
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.Empresa).WithMany(x => x.Usuarios).HasForeignKey(x => x.EmpresaId);
        });

        // Cliente
        modelBuilder.Entity<Cliente>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200);
            e.HasOne(x => x.Empresa).WithMany(x => x.Clientes).HasForeignKey(x => x.EmpresaId);
        });

        // Producto
        modelBuilder.Entity<Producto>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Precio).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Empresa).WithMany(x => x.Productos).HasForeignKey(x => x.EmpresaId);
        });

        // Venta
        modelBuilder.Entity<Venta>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Total).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Empresa).WithMany(x => x.Ventas).HasForeignKey(x => x.EmpresaId);
            e.HasOne(x => x.Cliente).WithMany(x => x.Ventas).HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        });

        // Pago
        modelBuilder.Entity<Pago>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Monto).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Venta).WithMany(x => x.Pagos).HasForeignKey(x => x.VentaId);
        });

        // ImportJob
        modelBuilder.Entity<ImportJob>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Empresa).WithMany().HasForeignKey(x => x.EmpresaId);
        });

        // PasswordResetToken
        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Token).IsUnique();
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UsuarioNombre).HasMaxLength(200);
            e.Property(x => x.Accion).HasMaxLength(50);
            e.Property(x => x.Entidad).HasMaxLength(100);
            e.Property(x => x.Detalle).HasMaxLength(500);
            e.HasIndex(x => new { x.EmpresaId, x.Fecha });
        });

        // Staging
        modelBuilder.Entity<StgCliente>().HasKey(x => x.Id);
        modelBuilder.Entity<StgVenta>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Total).HasColumnType("decimal(18,2)");
        });
        modelBuilder.Entity<StgProducto>().HasKey(x => x.Id);

        // DW
        modelBuilder.Entity<DimFecha>().HasKey(x => x.Id);
        modelBuilder.Entity<DimCliente>().HasKey(x => x.Id);
        modelBuilder.Entity<DimProducto>().HasKey(x => x.Id);

        modelBuilder.Entity<FactVenta>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Total).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.DimFecha).WithMany().HasForeignKey(x => x.FechaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.DimCliente).WithMany().HasForeignKey(x => x.ClienteDimId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FactPago>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Monto).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.DimFecha).WithMany().HasForeignKey(x => x.FechaId).OnDelete(DeleteBehavior.Restrict);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Empresa>().HasData(new Empresa
        {
            Id = 1,
            Nombre = "Empresa Demo S.A.",
            FechaCreacion = now,
            Activo = true
        });

        // Password: Admin123! — SHA256 hex of "Admin123!"
        modelBuilder.Entity<Usuario>().HasData(new Usuario
        {
            Id = 1,
            EmpresaId = 1,
            Nombre = "Administrador",
            Email = "admin@demo.com",
            PasswordHash = "3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121", // Admin123!
            Rol = "Admin",
            FechaCreacion = now
        });

        // Demo clients
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, EmpresaId = 1, Nombre = "Juan Pérez", Email = "juan@example.com", Telefono = "555-1001", FechaRegistro = now },
            new Cliente { Id = 2, EmpresaId = 1, Nombre = "María García", Email = "maria@example.com", Telefono = "555-1002", FechaRegistro = now },
            new Cliente { Id = 3, EmpresaId = 1, Nombre = "Carlos López", Email = "carlos@example.com", Telefono = "555-1003", FechaRegistro = now }
        );

        // Demo products
        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, EmpresaId = 1, Nombre = "Laptop Pro", Categoria = "Electrónica", Precio = 1299.99m, Activo = true },
            new Producto { Id = 2, EmpresaId = 1, Nombre = "Mouse Inalámbrico", Categoria = "Periféricos", Precio = 29.99m, Activo = true },
            new Producto { Id = 3, EmpresaId = 1, Nombre = "Monitor 27\"", Categoria = "Electrónica", Precio = 399.99m, Activo = true }
        );

        // Demo sales
        modelBuilder.Entity<Venta>().HasData(
            new Venta { Id = 1, EmpresaId = 1, ClienteId = 1, Fecha = now.AddDays(-30), Total = 1329.98m, Estado = "Completada" },
            new Venta { Id = 2, EmpresaId = 1, ClienteId = 2, Fecha = now.AddDays(-20), Total = 399.99m, Estado = "Completada" },
            new Venta { Id = 3, EmpresaId = 1, ClienteId = 3, Fecha = now.AddDays(-10), Total = 1299.99m, Estado = "Pendiente" },
            new Venta { Id = 4, EmpresaId = 1, ClienteId = 1, Fecha = now.AddDays(-5), Total = 29.99m, Estado = "Completada" }
        );

        // Demo payments
        modelBuilder.Entity<Pago>().HasData(
            new Pago { Id = 1, VentaId = 1, Metodo = "Tarjeta", Monto = 1329.98m, Fecha = now.AddDays(-30) },
            new Pago { Id = 2, VentaId = 2, Metodo = "Transferencia", Monto = 399.99m, Fecha = now.AddDays(-20) },
            new Pago { Id = 3, VentaId = 4, Metodo = "Efectivo", Monto = 29.99m, Fecha = now.AddDays(-5) }
        );

        // DW seed
        modelBuilder.Entity<DimFecha>().HasData(
            new DimFecha { Id = 1, Fecha = now.AddDays(-30), Año = 2025, Mes = 12, NombreMes = "Diciembre", Trimestre = 4 },
            new DimFecha { Id = 2, Fecha = now.AddDays(-20), Año = 2025, Mes = 12, NombreMes = "Diciembre", Trimestre = 4 },
            new DimFecha { Id = 3, Fecha = now.AddDays(-10), Año = 2026, Mes = 1, NombreMes = "Enero", Trimestre = 1 },
            new DimFecha { Id = 4, Fecha = now.AddDays(-5), Año = 2026, Mes = 1, NombreMes = "Enero", Trimestre = 1 }
        );

        modelBuilder.Entity<DimCliente>().HasData(
            new DimCliente { Id = 1, EmpresaId = 1, ClienteId = 1, Nombre = "Juan Pérez" },
            new DimCliente { Id = 2, EmpresaId = 1, ClienteId = 2, Nombre = "María García" },
            new DimCliente { Id = 3, EmpresaId = 1, ClienteId = 3, Nombre = "Carlos López" }
        );

        modelBuilder.Entity<DimProducto>().HasData(
            new DimProducto { Id = 1, EmpresaId = 1, ProductoId = 1, Nombre = "Laptop Pro", Categoria = "Electrónica" },
            new DimProducto { Id = 2, EmpresaId = 1, ProductoId = 2, Nombre = "Mouse Inalámbrico", Categoria = "Periféricos" },
            new DimProducto { Id = 3, EmpresaId = 1, ProductoId = 3, Nombre = "Monitor 27\"", Categoria = "Electrónica" }
        );

        modelBuilder.Entity<FactVenta>().HasData(
            new FactVenta { Id = 1, EmpresaId = 1, FechaId = 1, ClienteDimId = 1, VentaId = 1, Total = 1329.98m, Cantidad = 1 },
            new FactVenta { Id = 2, EmpresaId = 1, FechaId = 2, ClienteDimId = 2, VentaId = 2, Total = 399.99m, Cantidad = 1 },
            new FactVenta { Id = 3, EmpresaId = 1, FechaId = 3, ClienteDimId = 3, VentaId = 3, Total = 1299.99m, Cantidad = 1 },
            new FactVenta { Id = 4, EmpresaId = 1, FechaId = 4, ClienteDimId = 1, VentaId = 4, Total = 29.99m, Cantidad = 1 }
        );

        modelBuilder.Entity<FactPago>().HasData(
            new FactPago { Id = 1, EmpresaId = 1, FechaId = 1, PagoId = 1, Monto = 1329.98m },
            new FactPago { Id = 2, EmpresaId = 1, FechaId = 2, PagoId = 2, Monto = 399.99m },
            new FactPago { Id = 3, EmpresaId = 1, FechaId = 4, PagoId = 3, Monto = 29.99m }
        );
    }
}
