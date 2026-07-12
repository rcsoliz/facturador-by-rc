using Facturacion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infrastructure.Persistence.Configurations;

public class ItemCatalogoConfiguration : IEntityTypeConfiguration<ItemCatalogo>
{
    public void Configure(EntityTypeBuilder<ItemCatalogo> builder)
    {
        builder.ToTable("Catalogos");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Descripcion).IsRequired().HasMaxLength(500);

        builder.HasIndex(c => new { c.Tipo, c.Codigo }).IsUnique();
    }
}
