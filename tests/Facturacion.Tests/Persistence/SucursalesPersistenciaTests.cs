using Facturacion.Application.Commands.AgregarPuntoVenta;
using Facturacion.Application.Commands.AgregarSucursal;
using Facturacion.Application.Dtos;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.ValueObjects;
using Facturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Facturacion.Tests.Persistence;

/// <summary>
/// A diferencia de EmisionEndToEndTests (que arma Tenant+Sucursal+PuntoVenta en un
/// solo grafo nuevo y los persiste juntos), acá el Tenant se persiste primero y se
/// vuelve a cargar en un DbContext separado antes de agregarle una Sucursal — el
/// mismo patrón que sigue un request HTTP real (ApiKeyAuthMiddleware ya cargó el
/// Tenant antes de que el handler lo mute). Así se reprodujo el bug real: EF Core
/// generaba un UPDATE (no INSERT) para la Sucursal nueva porque su Id (Guid
/// generado en el dominio) hacía que, al descubrirla solo por el grafo de
/// navegación de un Tenant ya trackeado, la infiriera como existente.
/// </summary>
[Collection("Postgres")]
public class SucursalesPersistenciaTests
{
    private readonly PostgresFixture _fixture;

    public SucursalesPersistenciaTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task AgregarSucursalYPuntoVenta_SobreTenantYaPersistido_PersisteCorrectamente()
    {
        Guid tenantId;
        await using (var dbAlta = _fixture.NuevoContexto())
        {
            var tenant = new Tenant(
                "Empresa Prueba", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea,
                $"hash-{Guid.NewGuid():N}");
            dbAlta.Tenants.Add(tenant);
            await dbAlta.SaveChangesAsync();
            tenantId = tenant.Id;
        }

        await using var db = _fixture.NuevoContexto();
        var tenants = new EfTenantRepository(db);
        var sucursalHandler = new AgregarSucursalHandler(tenants);
        var puntoVentaHandler = new AgregarPuntoVentaHandler(tenants);

        var sucursalResp = await sucursalHandler.HandleAsync(
            tenantId, new AgregarSucursalRequest(0, "AV. JORGE LOPEZ #123", "La Paz", "451010"));

        var puntoVentaResp = await puntoVentaHandler.HandleAsync(
            tenantId, sucursalResp.Id, new AgregarPuntoVentaRequest(0, "Caja 1", 1));

        Assert.NotNull(puntoVentaResp);

        await using var dbVerificacion = _fixture.NuevoContexto();
        var sucursalPersistida = await dbVerificacion.Sucursales
            .Include(s => s.PuntosVenta)
            .SingleAsync(s => s.Id == sucursalResp.Id);

        Assert.Equal("451010", sucursalPersistida.ActividadEconomica);
        Assert.Single(sucursalPersistida.PuntosVenta);
        Assert.Equal("Caja 1", sucursalPersistida.PuntosVenta.Single().Nombre);
    }

    [Fact]
    public async Task AgregarPuntoVenta_SucursalInexistente_DevuelveNull()
    {
        Guid tenantId;
        await using (var dbAlta = _fixture.NuevoContexto())
        {
            var tenant = new Tenant(
                "Empresa Prueba 2", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea,
                $"hash-{Guid.NewGuid():N}");
            dbAlta.Tenants.Add(tenant);
            await dbAlta.SaveChangesAsync();
            tenantId = tenant.Id;
        }

        await using var db = _fixture.NuevoContexto();
        var handler = new AgregarPuntoVentaHandler(new EfTenantRepository(db));

        var respuesta = await handler.HandleAsync(
            tenantId, Guid.NewGuid(), new AgregarPuntoVentaRequest(0, "Caja 1", 1));

        Assert.Null(respuesta);
    }
}
