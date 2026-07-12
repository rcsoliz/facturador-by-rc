using Facturacion.Api.Autenticacion;
using Facturacion.Application.Commands.AgregarPuntoVenta;
using Facturacion.Application.Commands.AgregarSucursal;
using Facturacion.Application.Commands.AnularFactura;
using Facturacion.Application.Commands.EmitirFactura;
using Facturacion.Application.Commands.RegistrarCredencialSiat;
using Facturacion.Application.Commands.RegistrarTenant;
using Facturacion.Application.Queries;
using Facturacion.Domain.Ports;
using Facturacion.Infrastructure.Colas;
using Facturacion.Infrastructure.Persistence;
using Facturacion.Infrastructure.Seguridad;
using Facturacion.Infrastructure.Siat.Common;
using Facturacion.Infrastructure.Siat.Fake;
using Facturacion.Infrastructure.Webhooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    const string esquema = "ApiKey";
    options.AddSecurityDefinition(esquema, new OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "API key del tenant (header X-Api-Key).",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = esquema },
            },
            Array.Empty<string>()
        },
    });
});

// ── Autenticación (X-Api-Key → tenant) ──────────────────────────────────────
builder.Services.AddScoped<ICurrentTenant, CurrentTenant>();

var adminApiKey =
    Environment.GetEnvironmentVariable("ADMIN_API_KEY")
    ?? builder.Configuration["Admin:ApiKey"]
    ?? throw new InvalidOperationException(
        "No se configuró el admin API key (variable ADMIN_API_KEY o Admin:ApiKey).");
builder.Services.AddSingleton(new AdminOptions(adminApiKey));

// ── Casos de uso ────────────────────────────────────────────────────────────
builder.Services.AddScoped<EmitirFacturaHandler>();
builder.Services.AddScoped<AnularFacturaHandler>();
builder.Services.AddScoped<ConsultarFacturaHandler>();
builder.Services.AddScoped<ProcesarEmisionHandler>();
builder.Services.AddScoped<RegistrarTenantHandler>();
builder.Services.AddScoped<AgregarSucursalHandler>();
builder.Services.AddScoped<AgregarPuntoVentaHandler>();
builder.Services.AddScoped<RegistrarCredencialSiatHandler>();

// ── Persistencia (EF Core + Npgsql) ─────────────────────────────────────────
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "No se configuró la cadena de conexión (variable DATABASE_URL o ConnectionStrings:Default).");

builder.Services.AddDbContext<FacturacionDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IFacturaRepository, EfFacturaRepository>();
builder.Services.AddScoped<ITenantRepository, EfTenantRepository>();
builder.Services.AddScoped<ICredencialSiatRepository, EfCredencialSiatRepository>();
builder.Services.AddScoped<ICatalogoRepository, EfCatalogoRepository>();

// ── Seguridad (cifrado en reposo) ───────────────────────────────────────────
var claveMaestra =
    Environment.GetEnvironmentVariable("CREDENCIALES_CLAVE_MAESTRA")
    ?? builder.Configuration["Seguridad:ClaveMaestra"]
    ?? throw new InvalidOperationException(
        "No se configuró la clave maestra de cifrado (variable CREDENCIALES_CLAVE_MAESTRA o " +
        "Seguridad:ClaveMaestra) — 32 bytes AES-256 en Base64.");
builder.Services.AddSingleton(new ProteccionDatosOptions(claveMaestra));
builder.Services.AddSingleton<IProteccionDatos, ProteccionDatosAes>();

// ── Adaptadores (Infrastructure) ────────────────────────────────────────────
// TODO(claude-code): selección de IProveedorFiscal por ModalidadFacturacion del
// tenant (factory, cuando exista SiatComputarizadaAdapter real); Hangfire como
// IEncoladorEmision. Por ahora todo corre contra mocks (ver restricción "SIN
// ACCESO AL AMBIENTE PILOTO DEL SIN" en CLAUDE.md).
builder.Services.AddScoped<IEncoladorEmision, EncoladorEmisionInmediato>();
builder.Services.AddScoped<ISinCredencialesClient, CredencialesClienteFake>();
builder.Services.AddScoped<CredencialesService>();
builder.Services.AddScoped<IGestorCredencialesSiat>(sp => sp.GetRequiredService<CredencialesService>());
builder.Services.AddScoped<ISinCatalogosClient, CatalogosClienteFake>();
builder.Services.AddScoped<CatalogosService>();
builder.Services.AddScoped<IProveedorFiscal, SiatFakeAdapter>();
builder.Services.AddScoped<INotificadorWebhook, NotificadorWebhookLog>();
builder.Services.Configure<SiatOptions>(builder.Configuration.GetSection(SiatOptions.SeccionConfiguracion));
builder.Services.Configure<SiatFakeAdapterOptions>(
    builder.Configuration.GetSection(SiatFakeAdapterOptions.SeccionConfiguracion));

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

app.UseMiddleware<AdminApiKeyMiddleware>();
app.UseMiddleware<ApiKeyAuthMiddleware>();

app.MapControllers();

app.Run();
