using Facturacion.Application.Commands.EmitirFactura;
using Facturacion.Application.Dtos;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.ValueObjects;
using Facturacion.Infrastructure.Colas;
using Facturacion.Infrastructure.Persistence;
using Facturacion.Infrastructure.Seguridad;
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
    // AES-256 de prueba (32 bytes en Base64) — nunca usar en producción.
    private const string ClaveMaestraPrueba = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=";

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

        var proteccion = new ProteccionDatosAes(new ProteccionDatosOptions(ClaveMaestraPrueba));
        db.CredencialesSiat.Add(new CredencialSiat(
            tenant.Id, sucursal.Id, puntoVenta.Id, proteccion.Cifrar("token-delegado-prueba")));
        await db.SaveChangesAsync();

        return (tenant.Id, sucursal.Id, puntoVenta.Id);
    }

    private static EmitirFacturaHandler ArmarHandler(FacturacionDbContext db)
    {
        var facturas = new EfFacturaRepository(db);
        var tenants = new EfTenantRepository(db);
        var credenciales = new CredencialesService(
            new EfCredencialSiatRepository(db),
            new CredencialesClienteFake(),
            new ProteccionDatosAes(new ProteccionDatosOptions(ClaveMaestraPrueba)));
        var proveedor = new SiatFakeAdapter(
            tenants, credenciales, Options.Create(new SiatOptions()), Options.Create(new SiatFakeAdapterOptions()));
        var webhook = new NotificadorWebhookHttp(
            new HttpClientFactoryNuncaLlamado(), new ProteccionDatosAes(new ProteccionDatosOptions(ClaveMaestraPrueba)),
            NullLogger<NotificadorWebhookHttp>.Instance);
        var procesarEmision = new ProcesarEmisionHandler(facturas, tenants, proveedor, webhook);
        var encolador = new EncoladorEmisionSincronoFake(procesarEmision);

        return new EmitirFacturaHandler(facturas, encolador);
    }

    /// <summary>
    /// En producción <see cref="Facturacion.Infrastructure.Colas.EncoladorEmisionHangfire"/>
    /// solo encola (fire-and-forget) — acá se necesita que el procesamiento
    /// corra sincrónicamente para poder aserir el estado final justo después
    /// de <c>handler.HandleAsync</c>, sin levantar un servidor Hangfire real.
    /// </summary>
    private sealed class EncoladorEmisionSincronoFake : IEncoladorEmision
    {
        private readonly ProcesarEmisionHandler _procesarEmision;
        public EncoladorEmisionSincronoFake(ProcesarEmisionHandler procesarEmision) => _procesarEmision = procesarEmision;

        public Task EncolarEmisionAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default) =>
            _procesarEmision.HandleAsync(tenantId, facturaId, ct);

        public Task EncolarAnulacionAsync(Guid tenantId, Guid facturaId, int codigoMotivo, CancellationToken ct = default) =>
            throw new NotSupportedException();
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

    /// <summary>
    /// Los tenants sembrados acá nunca configuran webhook (WebhookUrl null),
    /// así que NotificadorWebhookHttp debe omitir el envío sin tocar HTTP —
    /// si algún día lo llamara, este fake lo hace fallar fuerte en vez de
    /// intentar una conexión real durante el test.
    /// </summary>
    private sealed class HttpClientFactoryNuncaLlamado : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) =>
            throw new InvalidOperationException(
                "No se esperaba una llamada HTTP: el tenant de prueba no tiene webhook configurado.");
    }
}
