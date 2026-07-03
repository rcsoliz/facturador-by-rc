using Facturacion.Application.Commands.AnularFactura;
using Facturacion.Application.Commands.EmitirFactura;
using Facturacion.Application.Queries;
using Facturacion.Domain.Ports;
using Facturacion.Infrastructure.Colas;
using Facturacion.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Casos de uso ────────────────────────────────────────────────────────────
builder.Services.AddScoped<EmitirFacturaHandler>();
builder.Services.AddScoped<AnularFacturaHandler>();
builder.Services.AddScoped<ConsultarFacturaHandler>();
builder.Services.AddScoped<ProcesarEmisionHandler>();

// ── Adaptadores (Infrastructure) ────────────────────────────────────────────
// TODO(claude-code): EF Core + Npgsql; selección de IProveedorFiscal por
// ModalidadFacturacion del tenant (factory); Hangfire como IEncoladorEmision.
builder.Services.AddSingleton<IFacturaRepository, InMemoryFacturaRepository>();
builder.Services.AddSingleton<IEncoladorEmision, EncoladorEmisionInmediato>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
