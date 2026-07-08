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
    }
}
