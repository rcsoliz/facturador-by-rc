using System.Net;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;
using Facturacion.Domain.ValueObjects;
using Facturacion.Infrastructure.Webhooks;
using Microsoft.Extensions.Logging.Abstractions;

namespace Facturacion.Tests;

public class NotificadorWebhookHttpTests
{
    private static Tenant CrearTenant() =>
        new("Empresa Prueba", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");

    private static Factura CrearFactura() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        1, $"REF-{Guid.NewGuid()}",
        "Cliente Prueba", 1, "5115889", null, null,
        1, 1, 1, null,
        new[] { new DetalleFactura(49111, "JN-1", "Jugo de naranja en vaso", 1, 1, 100m, 0) });

    [Fact]
    public async Task NotificarCambioEstadoAsync_SinWebhookConfigurado_NoLlamaAlCliente()
    {
        var factory = new HttpClientFactoryFake(HttpStatusCode.OK);
        var notificador = new NotificadorWebhookHttp(
            factory, new ProteccionDatosFake(), NullLogger<NotificadorWebhookHttp>.Instance);

        await notificador.NotificarCambioEstadoAsync(CrearTenant(), CrearFactura());

        Assert.Null(factory.Handler.UltimaPeticion);
    }

    [Fact]
    public async Task NotificarCambioEstadoAsync_ConWebhookConfigurado_EnviaPostFirmado()
    {
        var tenant = CrearTenant();
        tenant.ConfigurarWebhook("https://cliente.example.com/webhooks/facturacion", "CIFRADO:mi-secreto");
        var factura = CrearFactura();

        var factory = new HttpClientFactoryFake(HttpStatusCode.OK);
        var proteccion = new ProteccionDatosFake();
        var notificador = new NotificadorWebhookHttp(factory, proteccion, NullLogger<NotificadorWebhookHttp>.Instance);

        await notificador.NotificarCambioEstadoAsync(tenant, factura);

        var peticion = factory.Handler.UltimaPeticion;
        Assert.NotNull(peticion);
        Assert.Equal(HttpMethod.Post, peticion!.Method);
        Assert.Equal("https://cliente.example.com/webhooks/facturacion", peticion.RequestUri!.ToString());

        var timestamp = peticion.Headers.GetValues("X-Facturacion-Timestamp").Single();
        var firmaEsperada = "sha256=" + FirmaWebhook.Calcular("mi-secreto", timestamp, factory.Handler.UltimoCuerpo!);
        Assert.Equal(firmaEsperada, peticion.Headers.GetValues("X-Facturacion-Signature").Single());

        Assert.Contains(factura.ReferenciaExterna, factory.Handler.UltimoCuerpo);
    }

    [Fact]
    public async Task NotificarCambioEstadoAsync_ElClienteResponde500_NoLanzaExcepcion()
    {
        var tenant = CrearTenant();
        tenant.ConfigurarWebhook("https://cliente.example.com/webhooks/facturacion", "CIFRADO:mi-secreto");

        var factory = new HttpClientFactoryFake(HttpStatusCode.InternalServerError);
        var notificador = new NotificadorWebhookHttp(
            factory, new ProteccionDatosFake(), NullLogger<NotificadorWebhookHttp>.Instance);

        await notificador.NotificarCambioEstadoAsync(tenant, CrearFactura());
        // No debe relanzar: un webhook caído no debe romper el flujo de emisión.
    }

    [Fact]
    public async Task NotificarCambioEstadoAsync_ElEnvioLanzaExcepcion_NoSePropaga()
    {
        var tenant = CrearTenant();
        tenant.ConfigurarWebhook("https://cliente.example.com/webhooks/facturacion", "CIFRADO:mi-secreto");

        var factory = new HttpClientFactoryFake(lanzarExcepcion: true);
        var notificador = new NotificadorWebhookHttp(
            factory, new ProteccionDatosFake(), NullLogger<NotificadorWebhookHttp>.Instance);

        await notificador.NotificarCambioEstadoAsync(tenant, CrearFactura());
    }

    private sealed class HttpClientFactoryFake : IHttpClientFactory
    {
        public StubHttpMessageHandler Handler { get; }

        public HttpClientFactoryFake(HttpStatusCode? codigoRespuesta = null, bool lanzarExcepcion = false) =>
            Handler = new StubHttpMessageHandler(codigoRespuesta ?? HttpStatusCode.OK, lanzarExcepcion);

        public HttpClient CreateClient(string name) => new(Handler);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _codigoRespuesta;
        private readonly bool _lanzarExcepcion;

        public HttpRequestMessage? UltimaPeticion { get; private set; }
        public string? UltimoCuerpo { get; private set; }

        public StubHttpMessageHandler(HttpStatusCode codigoRespuesta, bool lanzarExcepcion)
        {
            _codigoRespuesta = codigoRespuesta;
            _lanzarExcepcion = lanzarExcepcion;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            UltimaPeticion = request;
            UltimoCuerpo = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);

            if (_lanzarExcepcion) throw new HttpRequestException("fallo simulado de red");

            return new HttpResponseMessage(_codigoRespuesta);
        }
    }

    private sealed class ProteccionDatosFake : IProteccionDatos
    {
        public string Cifrar(string textoPlano) => $"CIFRADO:{textoPlano}";
        public string Descifrar(string textoCifrado) => textoCifrado.Replace("CIFRADO:", "");
    }
}
