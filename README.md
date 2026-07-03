# Facturacion API

Servicio standalone multi-tenant de **facturación electrónica para Bolivia (SIAT)**,
consumible por API REST. v1: modalidad Computarizada en Línea. v1.5: Electrónica en Línea.

## Arquitectura

```
src/
├── Facturacion.Domain          # Entidades, máquina de estados, puertos. Cero dependencias.
├── Facturacion.Application     # Casos de uso (emitir, anular, consultar, procesar)
├── Facturacion.Infrastructure  # Adaptadores: SIAT (SOAP/XML), persistencia, colas, webhooks
├── Facturacion.Api             # REST público + Swagger
└── Facturacion.Workers         # Jobs: procesamiento, CUFD, catálogos, contingencia
tests/
└── Facturacion.Tests           # xUnit
```

Puerto clave: `Domain/Ports/IProveedorFiscal` — abstrae el ente fiscal.
La modalidad del tenant decide el adaptador. Ver `CLAUDE.md` para el contexto completo.

## Contrato REST (v1)

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/v1/facturas` | Emitir (202, asíncrono, idempotente por `referenciaExterna`) |
| GET | `/api/v1/facturas/{id}` | Consultar estado |
| POST | `/api/v1/facturas/{id}/anulacion` | Anular (202, asíncrono) |
| GET | `/api/v1/facturas/{id}/pdf` | Representación gráfica |

Resultado final de emisión/anulación: **webhook** al sistema cliente (polling como fallback).

## Correr

```bash
dotnet build
dotnet test
dotnet run --project src/Facturacion.Api   # Swagger en /swagger
```
