using Facturacion.Domain.Entities;
using Facturacion.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Facturacion.Infrastructure.Persistence.Configurations;

public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("Facturas");
        builder.HasKey(f => f.Id);

        var cufConverter = new ValueConverter<Cuf?, string?>(
            cuf => cuf == null ? null : cuf.Valor,
            valor => valor == null ? null : new Cuf(valor));
        builder.Property(f => f.Cuf).HasConversion(cufConverter);

        builder.Property(f => f.MontoTotal).HasPrecision(18, 2);
        builder.Property(f => f.MontoTotalSujetoIva).HasPrecision(18, 2);
        builder.Property(f => f.TipoCambio).HasPrecision(18, 6);

        builder.HasIndex(f => new { f.TenantId, f.ReferenciaExterna }).IsUnique();
        builder.HasIndex(f => new { f.TenantId, f.Estado });

        builder.HasMany(f => f.Detalles)
            .WithOne()
            .HasForeignKey(d => d.FacturaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.Detalles)
            .HasField("_detalles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
