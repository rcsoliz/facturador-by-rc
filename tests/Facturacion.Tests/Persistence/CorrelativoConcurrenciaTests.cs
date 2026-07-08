using Facturacion.Infrastructure.Persistence;
using Xunit;

namespace Facturacion.Tests.Persistence;

[Collection("Postgres")]
public class CorrelativoConcurrenciaTests
{
    private readonly PostgresFixture _fixture;

    public CorrelativoConcurrenciaTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SiguienteNumeroAsync_LlamadasConcurrentes_DevuelveNumerosUnicosConsecutivos()
    {
        var puntoVentaId = Guid.NewGuid();

        var tareas = Enumerable.Range(0, 20).Select(async _ =>
        {
            // Cada tarea usa su propio DbContext/conexión: un DbContext no es thread-safe.
            await using var db = _fixture.NuevoContexto();
            var repo = new EfFacturaRepository(db);
            return await repo.SiguienteNumeroAsync(puntoVentaId);
        });

        var numeros = await Task.WhenAll(tareas);

        Assert.Equal(20, numeros.Distinct().Count());
        Assert.Equal(Enumerable.Range(1, 20).Select(n => (long)n), numeros.OrderBy(n => n));
    }
}
