using Facturacion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infrastructure.Persistence.Configurations;

public class CredencialSiatConfiguration : IEntityTypeConfiguration<CredencialSiat>
{
    public void Configure(EntityTypeBuilder<CredencialSiat> builder)
    {
        builder.ToTable("CredencialesSiat");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TokenDelegadoCifrado).IsRequired();
        builder.Property(c => c.Cuis).HasMaxLength(50);
        builder.Property(c => c.Cufd).HasMaxLength(50);
        builder.Property(c => c.CufdCodigoControl).HasMaxLength(50);

        // Una credencial por punto de venta, o por sucursal cuando PuntoVentaId es null.
        builder.HasIndex(c => new { c.TenantId, c.SucursalId, c.PuntoVentaId }).IsUnique();
    }
}
