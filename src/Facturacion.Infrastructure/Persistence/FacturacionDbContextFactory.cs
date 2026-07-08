using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Facturacion.Infrastructure.Persistence;

/// <summary>
/// Permite a `dotnet ef` crear el DbContext sin levantar Facturacion.Api.
/// La connection string real de runtime se resuelve en Program.cs (DATABASE_URL
/// o ConnectionStrings:Default); acá solo hace falta algo válido para diseño.
/// </summary>
public class FacturacionDbContextFactory : IDesignTimeDbContextFactory<FacturacionDbContext>
{
    public FacturacionDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Port=5433;Database=facturacion;Username=facturacion;Password=facturacion";

        var optionsBuilder = new DbContextOptionsBuilder<FacturacionDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new FacturacionDbContext(optionsBuilder.Options);
    }
}
