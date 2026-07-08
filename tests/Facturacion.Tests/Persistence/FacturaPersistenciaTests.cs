using Facturacion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Facturacion.Tests.Persistence;

[Collection("Postgres")]
public class FacturaPersistenciaTests
{
    private readonly PostgresFixture _fixture;

    public FacturaPersistenciaTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GuardarYRecuperarFactura_ConDetalles_PersisteTodosLosCampos()
    {
        var factura = new Factura(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, $"REF-{Guid.NewGuid()}",
            "Cliente Prueba", 1, "1234567", null, null, 1, 1,
            new[] { new DetalleFactura(99100, "P-1", "Servicio", 2, 58, 50m, 5m) });

        await using (var db = _fixture.NuevoContexto())
        {
            await db.Facturas.AddAsync(factura);
            await db.SaveChangesAsync();
        }

        await using var dbLectura = _fixture.NuevoContexto();
        var recuperada = await dbLectura.Facturas
            .Include(f => f.Detalles)
            .SingleAsync(f => f.Id == factura.Id);

        Assert.Single(recuperada.Detalles);
        Assert.Equal(95m, recuperada.Detalles.Single().SubTotal); // calculada, no mapeada
        Assert.Equal(factura.MontoTotal, recuperada.MontoTotal);
        Assert.Equal(factura.ReferenciaExterna, recuperada.ReferenciaExterna);
    }

    [Fact]
    public async Task ReferenciaExternaDuplicada_MismoTenant_ViolaIndiceUnico()
    {
        var tenantId = Guid.NewGuid();
        var referencia = $"REF-{Guid.NewGuid()}";

        Factura NuevaFactura() => new(
            tenantId, Guid.NewGuid(), Guid.NewGuid(), 1, referencia,
            "Cliente Prueba", 1, "1234567", null, null, 1, 1,
            new[] { new DetalleFactura(99100, "P-1", "Servicio", 1, 58, 10m) });

        await using (var db = _fixture.NuevoContexto())
        {
            await db.Facturas.AddAsync(NuevaFactura());
            await db.SaveChangesAsync();
        }

        await using var db2 = _fixture.NuevoContexto();
        await db2.Facturas.AddAsync(NuevaFactura());

        await Assert.ThrowsAsync<DbUpdateException>(() => db2.SaveChangesAsync());
    }
}
