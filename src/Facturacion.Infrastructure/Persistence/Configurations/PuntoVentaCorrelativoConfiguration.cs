using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infrastructure.Persistence.Configurations;

public class PuntoVentaCorrelativoConfiguration : IEntityTypeConfiguration<PuntoVentaCorrelativo>
{
    public void Configure(EntityTypeBuilder<PuntoVentaCorrelativo> builder)
    {
        builder.ToTable("PuntoVentaCorrelativos");
        builder.HasKey(x => x.PuntoVentaId);
    }
}
