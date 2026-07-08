using Facturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Facturacion.Tests.Persistence;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("facturacion_test")
        .WithUsername("facturacion")
        .WithPassword("facturacion")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<FacturacionDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var db = new FacturacionDbContext(options);
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public FacturacionDbContext NuevoContexto() =>
        new(new DbContextOptionsBuilder<FacturacionDbContext>().UseNpgsql(ConnectionString).Options);
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }
