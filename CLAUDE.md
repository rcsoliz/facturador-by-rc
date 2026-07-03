# Facturacion API — Servicio de Facturación Electrónica (SIAT Bolivia)

## Qué es este proyecto

Servicio **standalone multi-tenant** de facturación electrónica para Bolivia (SIAT/SIN),
consumible vía API REST por cualquier sistema (propio o de terceros). Es un producto
vendible: los clientes pagan por factura emitida o suscripción por NIT.

**Principio rector:** los sistemas cliente NO conocen CUFD, CUIS, XML, SOAP ni nada del SIN.
Solo consumen REST: emitir → 202 → webhook con el resultado (o polling).

## Stack (decidido, no cambiar sin consultar a Roberto)

- .NET 8 / ASP.NET Core (Controllers) + Swashbuckle
- Clean Architecture estricta: Domain ← Application ← Infrastructure / Api / Workers
- PostgreSQL (Npgsql + EF Core) — multi-tenant por `TenantId` en cada tabla
- MediatR para CQRS (migración pendiente: hoy los handlers son clases planas)
- Hangfire para colas/jobs (implementar `IEncoladorEmision`)
- Polly para resiliencia hacia el SIN (reintentos + circuit breaker)
- `dotnet-svcutil` para generar clientes SOAP desde los WSDL del SIN
- QuestPDF + QRCoder para la representación gráfica
- xUnit para tests

## Reglas de arquitectura (estrictas)

1. `Facturacion.Domain` no referencia NADA externo. Cero paquetes NuGet.
2. `Facturacion.Application` solo referencia Domain (+ MediatR cuando se migre).
3. Todo acceso al SIN pasa por el puerto `Domain/Ports/IProveedorFiscal`.
   - v1: `SiatComputarizadaAdapter` (hash SHA256, sin firma XMLDSig)
   - v1.5: `SiatElectronicaAdapter` (XMLDSig con `SignedXml`, certificados por tenant)
   - La `ModalidadFacturacion` del tenant decide qué adaptador inyectar (factory en DI).
4. Toda transición de estado de `Factura` pasa por sus métodos (máquina de estados).
   Nunca setear `Estado` directamente. Tests en `FacturaStateMachineTests`.
5. Emisión asíncrona: la API acepta (202) y encola; el worker procesa
   (`ProcesarEmisionHandler`) y notifica por webhook.
6. Secretos (tokens delegados SIAT, futuros certificados) SIEMPRE cifrados en reposo.
   Clave maestra por variable de entorno. Nunca en texto plano ni en el repo.

## Roadmap

### v1 — Computarizada en Línea (en curso)
- [x] Esqueleto de solución + Domain + máquina de estados + puertos
- [x] Contrato REST público (emitir/consultar/anular/pdf)
- [ ] Middleware de autenticación por `X-Api-Key` → resuelve tenant (quitar `TenantDev`)
- [ ] EF Core + Npgsql: DbContext, configuraciones, migraciones, secuencia de correlativos
      por punto de venta (¡atómica! usar secuencias de Postgres, no MAX+1)
- [ ] Clientes SOAP del SIN (ambiente piloto) con dotnet-svcutil
- [ ] Siat/Common: CufCalculator (módulo 11 + base 16), XmlFacturaBuilder + validación XSD,
      CredencialesService (CUIS anual / CUFD 24h), CatalogosService (sync diaria)
- [ ] Hangfire: cola de emisión, job de renovación CUFD, job de sync de catálogos,
      job de envío de paquetes de contingencia
- [ ] Webhooks firmados (HMAC) con reintentos
- [ ] Representación gráfica PDF + QR
- [ ] Registro/onboarding de tenants (endpoint admin)

### v1.5 — Electrónica en Línea
- [ ] `SiatElectronicaAdapter`: firma XMLDSig, gestión de certificados por tenant

## Contexto SIAT (referencia rápida)

- CUIS: código de inicio de sistema, vigencia ~1 año
- CUFD: código único de facturación diaria, vigencia 24h — renovar ANTES de vencer
- CUF: código único de factura, se calcula por factura (incluye código de control del CUFD)
- Envíos: individual, paquete de contingencia, masivo — todos XML gzip + hash SHA256
- Documento sector: define el XSD del XML (1 = compra-venta estándar)
- Catálogos/paramétricas: sincronización diaria obligatoria
- Ambientes: piloto (pruebas) y producción — configurar por `SiatOptions`

## Convenciones

- Código y comentarios en español (dominio boliviano); nombres de clases en español
- Commits: convencionales (`feat:`, `fix:`, `refactor:`, `test:`)
- Cambios mínimos y dirigidos; no refactorizar sin pedido explícito
- NO instalar nada global en la máquina sin preguntar
- Tests para toda lógica de dominio nueva

## Comandos

```bash
dotnet build                                   # compilar todo
dotnet test                                    # correr tests
dotnet run --project src/Facturacion.Api       # levantar la API (Swagger en /swagger)
```
