using Facturacion.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infrastructure.Persistence;

public class FacturacionDbContext : DbContext
{
    public FacturacionDbContext(DbContextOptions<FacturacionDbContext> options) : base(options) { }

    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<DetalleFactura> DetallesFactura => Set<DetalleFactura>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<PuntoVenta> PuntosVenta => Set<PuntoVenta>();
    public DbSet<CredencialSiat> CredencialesSiat => Set<CredencialSiat>();
    public DbSet<PuntoVentaCorrelativo> PuntoVentaCorrelativos => Set<PuntoVentaCorrelativo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FacturacionDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
