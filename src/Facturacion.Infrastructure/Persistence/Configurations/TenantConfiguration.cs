using Facturacion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Nit)
            .HasConversion(nit => nit.Valor, valor => new Facturacion.Domain.ValueObjects.Nit(valor));

        builder.HasIndex(t => t.ApiKeyHash).IsUnique();

        builder.Property(t => t.WebhookUrl).HasMaxLength(2048);

        builder.HasMany(t => t.Sucursales)
            .WithOne()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Sucursales)
            .HasField("_sucursales")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
