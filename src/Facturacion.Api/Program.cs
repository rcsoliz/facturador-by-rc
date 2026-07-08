using Facturacion.Application.Commands.AnularFactura;
using Facturacion.Application.Commands.EmitirFactura;
using Facturacion.Application.Queries;
using Facturacion.Domain.Ports;
using Facturacion.Infrastructure.Colas;
using Facturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ProcesarEmisionHandler (worker) depende de IProveedorFiscal/INotificadorWebhook,
// que todavía no tienen implementación (otros ítems del roadmap). Sin esto, el host
// no arranca en Development: valida el árbol de DI completo al hacer Build().
builder.Host.UseDefaultServiceProvider(options => options.ValidateOnBuild = false);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Casos de uso ────────────────────────────────────────────────────────────
builder.Services.AddScoped<EmitirFacturaHandler>();
builder.Services.AddScoped<AnularFacturaHandler>();
builder.Services.AddScoped<ConsultarFacturaHandler>();
builder.Services.AddScoped<ProcesarEmisionHandler>();

// ── Persistencia (EF Core + Npgsql) ─────────────────────────────────────────
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "No se configuró la cadena de conexión (variable DATABASE_URL o ConnectionStrings:Default).");

builder.Services.AddDbContext<FacturacionDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IFacturaRepository, EfFacturaRepository>();
builder.Services.AddScoped<ITenantRepository, EfTenantRepository>();

// ── Adaptadores (Infrastructure) ────────────────────────────────────────────
// TODO(claude-code): selección de IProveedorFiscal por ModalidadFacturacion del
// tenant (factory); Hangfire como IEncoladorEmision.
builder.Services.AddSingleton<IEncoladorEmision, EncoladorEmisionInmediato>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<FacturacionDbContext>();
        db.Database.Migrate();
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
