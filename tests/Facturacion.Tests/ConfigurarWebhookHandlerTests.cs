using Facturacion.Application.Commands.ConfigurarWebhook;
using Facturacion.Application.Dtos;
using Facturacion.Domain.Common;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;
using Facturacion.Domain.ValueObjects;

namespace Facturacion.Tests;

public class ConfigurarWebhookHandlerTests
{
    [Fact]
    public async Task HandleAsync_UrlYSecretoValidos_ConfiguraElWebhookCifrandoElSecreto()
    {
        var tenant = new Tenant("Empresa Prueba", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");
        var repo = new TenantRepositoryFake(tenant);
        var proteccion = new ProteccionDatosFake();
        var handler = new ConfigurarWebhookHandler(repo, proteccion);

        await handler.HandleAsync(
            tenant.Id, new ConfigurarWebhookRequest("https://cliente.example.com/webhooks", "mi-secreto"));

        Assert.Equal("https://cliente.example.com/webhooks", tenant.WebhookUrl);
        Assert.Equal(proteccion.Cifrar("mi-secreto"), tenant.WebhookSecretCifrado);
        Assert.NotEqual("mi-secreto", tenant.WebhookSecretCifrado); // nunca en texto plano
        Assert.True(repo.GuardadoLlamado);
    }

    [Theory]
    [InlineData("http://cliente.example.com/webhooks")] // no HTTPS
    [InlineData("no-es-una-url")]
    [InlineData("")]
    public async Task HandleAsync_UrlInvalida_LanzaDomainException(string url)
    {
        var tenant = new Tenant("Empresa Prueba", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");
        var handler = new ConfigurarWebhookHandler(new TenantRepositoryFake(tenant), new ProteccionDatosFake());

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(tenant.Id, new ConfigurarWebhookRequest(url, "mi-secreto")));

        Assert.Equal("WEBHOOK_URL_INVALIDA", ex.Codigo);
    }

    [Fact]
    public async Task HandleAsync_SecretoVacio_LanzaDomainException()
    {
        var tenant = new Tenant("Empresa Prueba", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");
        var handler = new ConfigurarWebhookHandler(new TenantRepositoryFake(tenant), new ProteccionDatosFake());

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(tenant.Id, new ConfigurarWebhookRequest("https://cliente.example.com/webhooks", " ")));

        Assert.Equal("WEBHOOK_SECRETO_REQUERIDO", ex.Codigo);
    }

    private sealed class TenantRepositoryFake : ITenantRepository
    {
        private readonly Tenant _tenant;
        public bool GuardadoLlamado { get; private set; }

        public TenantRepositoryFake(Tenant tenant) => _tenant = tenant;

        public Task<Tenant?> ObtenerAsync(Guid tenantId, CancellationToken ct = default) =>
            Task.FromResult(tenantId == _tenant.Id ? _tenant : null);

        public Task<Tenant?> ObtenerPorApiKeyHashAsync(string apiKeyHash, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AgregarAsync(Tenant tenant, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AgregarSucursalAsync(Sucursal sucursal, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AgregarPuntoVentaAsync(PuntoVenta puntoVenta, CancellationToken ct = default) => throw new NotSupportedException();

        public Task GuardarCambiosAsync(CancellationToken ct = default)
        {
            GuardadoLlamado = true;
            return Task.CompletedTask;
        }
    }

    private sealed class ProteccionDatosFake : IProteccionDatos
    {
        public string Cifrar(string textoPlano) => $"CIFRADO:{textoPlano}";
        public string Descifrar(string textoCifrado) => textoCifrado.Replace("CIFRADO:", "");
    }
}
