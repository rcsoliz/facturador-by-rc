using Facturacion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infrastructure.Persistence.Configurations;

public class DetalleFacturaConfiguration : IEntityTypeConfiguration<DetalleFactura>
{
    public void Configure(EntityTypeBuilder<DetalleFactura> builder)
    {
        builder.ToTable("DetallesFactura");
        builder.HasKey(d => d.Id);

        builder.Ignore(d => d.SubTotal); // propiedad calculada, no columna

        builder.Property(d => d.Cantidad).HasPrecision(18, 6);
        builder.Property(d => d.PrecioUnitario).HasPrecision(18, 6);
        builder.Property(d => d.MontoDescuento).HasPrecision(18, 6);
    }
}
