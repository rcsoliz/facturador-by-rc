using Facturacion.Application.Commands.EmitirFactura;
using Facturacion.Application.Dtos;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.ValueObjects;
using Facturacion.Infrastructure.Colas;
using Facturacion.Infrastructure.Persistence;
using Facturacion.Infrastructure.Siat.Common;
using Facturacion.Infrastructure.Siat.Fake;
using Facturacion.Infrastructure.Webhooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Facturacion.Tests.Persistence;

/// <summary>
/// Prueba el flujo completo (API → cola → worker → SIN fake → webhook fake) contra
/// Postgres real, sin tocar el SIN — ver restricción "SIN ACCESO AL AMBIENTE PILOTO
/// DEL SIN" en CLAUDE.md. Reemplaza la verificación manual con curl: no hay todavía
/// un endpoint admin para dar de alta Sucursal/PuntoVenta, así que se arman
/// directamente por dominio, igual que ya hace FacturaPersistenciaTests con Factura.
/// </summary>
[Collection("Postgres")]
public class EmisionEndToEndTests
{
    private readonly PostgresFixture _fixture;

    public EmisionEndToEndTests(PostgresFixture fixture) => _fixture = fixture;

    private static async Task<(Guid TenantId, Guid SucursalId, Guid PuntoVentaId)> SembrarTenantAsync(
        FacturacionDbContext db)
    {
        var tenant = new Tenant(
            "Mi Empresa SRL", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea,
            $"hash-fake-{Guid.NewGuid():N}");
        var sucursal = tenant.AgregarSucursal(0, "AV. JORGE LOPEZ #123", "La Paz", "451010");
        var puntoVenta = sucursal.AgregarPuntoVenta(0, "Caja 1", 1);

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        return (tenant.Id, sucursal.Id, puntoVenta.Id);
    }

    private static EmitirFacturaHandler ArmarHandler(FacturacionDbContext db)
    {
        var facturas = new EfFacturaRepository(db);
        var tenants = new EfTenantRepository(db);
        var proveedor = new SiatFakeAdapter(
            tenants, Options.Create(new SiatOptions()), Options.Create(new SiatFakeAdapterOptions()));
        var webhook = new NotificadorWebhookLog(NullLogger<NotificadorWebhookLog>.Instance);
        var procesarEmision = new ProcesarEmisionHandler(facturas, tenants, proveedor, webhook);
        var encolador = new EncoladorEmisionInmediato(procesarEmision);

        return new EmitirFacturaHandler(facturas, encolador);
    }

    private static EmitirFacturaRequest ArmarRequest(
        string referenciaExterna, Guid sucursalId, Guid puntoVentaId) => new(
        ReferenciaExterna: referenciaExterna,
        SucursalId: sucursalId,
        PuntoVentaId: puntoVentaId,
        CodigoDocumentoSector: 1,
        Comprador: new CompradorDto("Cliente Prueba", 1, "5115889", null, null),
        CodigoMoneda: 1,
        TipoCambio: 1,
        CodigoMetodoPago: 1,
        NumeroTarjeta: null,
        Detalles: new[] { new DetalleDto(49111, "JN-1", "Jugo de naranja en vaso", 1, 1, 100m, 0) });

    [Fact]
    public async Task EmitirFactura_FlujoCompleto_TerminaValidadaConCufYXml()
    {
        await using var db = _fixture.NuevoContexto();
        var (tenantId, sucursalId, puntoVentaId) = await SembrarTenantAsync(db);
        var handler = ArmarHandler(db);

        var respuesta = await handler.HandleAsync(
            tenantId, ArmarRequest($"REF-{Guid.NewGuid()}", sucursalId, puntoVentaId));

        var facturaFinal = await db.Facturas.SingleAsync(f => f.Id == respuesta.Id);
        Assert.Equal(EstadoFactura.Validada, facturaFinal.Estado);
        Assert.NotNull(facturaFinal.Cuf);
        Assert.False(string.IsNullOrWhiteSpace(facturaFinal.XmlGenerado));
        Assert.NotNull(facturaFinal.CodigoRecepcionSin);
    }

    [Fact]
    public async Task EmitirFactura_ReferenciaConPrefijoDeRechazo_TerminaRechazada()
    {
        await using var db = _fixture.NuevoContexto();
        var (tenantId, sucursalId, puntoVentaId) = await SembrarTenantAsync(db);
        var handler = ArmarHandler(db);

        var respuesta = await handler.HandleAsync(
            tenantId, ArmarRequest($"RECHAZAR-{Guid.NewGuid()}", sucursalId, puntoVentaId));

        var facturaFinal = await db.Facturas.SingleAsync(f => f.Id == respuesta.Id);
        Assert.Equal(EstadoFactura.Rechazada, facturaFinal.Estado);
        Assert.NotNull(facturaFinal.MotivoRechazo);
    }
}
