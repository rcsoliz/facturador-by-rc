using Facturacion.Application.Commands.AnularFactura;
using Facturacion.Application.Commands.EmitirFactura;
using Facturacion.Infrastructure.Colas;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace Facturacion.Tests;

public class EncoladorEmisionHangfireTests
{
    [Fact]
    public async Task EncolarEmisionAsync_CreaUnJobParaProcesarEmisionHandler()
    {
        var cliente = new BackgroundJobClientFake();
        var encolador = new EncoladorEmisionHangfire(cliente);
        var tenantId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();

        await encolador.EncolarEmisionAsync(tenantId, facturaId);

        Assert.NotNull(cliente.UltimoJob);
        Assert.Equal(typeof(ProcesarEmisionHandler), cliente.UltimoJob!.Type);
        Assert.Equal(nameof(ProcesarEmisionHandler.HandleAsync), cliente.UltimoJob.Method.Name);
        Assert.Equal(tenantId, cliente.UltimoJob.Args[0]);
        Assert.Equal(facturaId, cliente.UltimoJob.Args[1]);
        Assert.IsType<EnqueuedState>(cliente.UltimoEstado);
    }

    [Fact]
    public async Task EncolarAnulacionAsync_CreaUnJobParaProcesarAnulacionHandler()
    {
        var cliente = new BackgroundJobClientFake();
        var encolador = new EncoladorEmisionHangfire(cliente);
        var tenantId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();

        await encolador.EncolarAnulacionAsync(tenantId, facturaId, 3);

        Assert.NotNull(cliente.UltimoJob);
        Assert.Equal(typeof(ProcesarAnulacionHandler), cliente.UltimoJob!.Type);
        Assert.Equal(nameof(ProcesarAnulacionHandler.HandleAsync), cliente.UltimoJob.Method.Name);
        Assert.Equal(tenantId, cliente.UltimoJob.Args[0]);
        Assert.Equal(facturaId, cliente.UltimoJob.Args[1]);
        Assert.Equal(3, cliente.UltimoJob.Args[2]);
    }

    private sealed class BackgroundJobClientFake : IBackgroundJobClient
    {
        public Job? UltimoJob { get; private set; }
        public IState? UltimoEstado { get; private set; }

        public string Create(Job job, IState state)
        {
            UltimoJob = job;
            UltimoEstado = state;
            return Guid.NewGuid().ToString();
        }

        public bool ChangeState(string jobId, IState state, string? expectedState) => true;
    }
}
