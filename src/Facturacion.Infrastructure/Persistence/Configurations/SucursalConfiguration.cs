using Facturacion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infrastructure.Persistence.Configurations;

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("Sucursales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ActividadEconomica).HasMaxLength(10);

        builder.HasMany(s => s.PuntosVenta)
            .WithOne()
            .HasForeignKey(p => p.SucursalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.PuntosVenta)
            .HasField("_puntosVenta")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
