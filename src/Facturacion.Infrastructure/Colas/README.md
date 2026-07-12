# Colas — Hangfire

Cola de emisión real (roadmap v1). Reemplaza `EncoladorEmisionInmediato`
(síncrono, provisional, encolaba "en el acto" dentro del mismo request HTTP —
contradecía el diseño documentado de "202 Aceptada + procesamiento asíncrono").

- `EncoladorEmisionHangfire.cs` — implementa `IEncoladorEmision` (Application)
  con `IBackgroundJobClient.Enqueue<T>(...)`: solo persiste el job y devuelve,
  el procesamiento corre en el servidor de Hangfire. Usa
  `ProcesarEmisionHandler`/`ProcesarAnulacionHandler` como destino del job.
- `JobEnvioPaquetesContingencia.cs` — job recurrente (cada 15 min): si el SIN
  está disponible (`IProveedorFiscal.ComunicacionDisponibleAsync`), agrupa las
  facturas `EnContingencia` de **todos** los tenants por `TenantId` (un
  paquete de contingencia es una operación por NIT, nunca se mezclan
  tenants) y llama `EnviarPaqueteContingenciaAsync` por grupo. Requirió
  ampliar `Factura.MarcarRechazada` para aceptar también el estado
  `EnContingencia` como origen (antes solo aceptaba `Enviada` — una factura
  en contingencia nunca pasó por `Enviada`, así que el paquete no podía
  rechazarse sin este cambio). Ver `FacturaStateMachineTests`.
- `JobRenovacionCufd.cs` vive en `Siat/Common/` (no acá): renueva CUIS/CUFD de
  **todas** las credenciales registradas cada 6h, iterando
  `ICredencialSiatRepository.ListarTodasAsync()` (nuevo método del puerto) y
  llamando `CredencialesService.ObtenerCufdVigenteAsync` por cada una — no
  reemplaza la renovación lazy bajo demanda que ya hacía `CredencialesService`
  en cada emisión, es una capa extra para reducir la latencia agregada por
  renovación en el camino caliente.
- `CatalogosService.SincronizarAsync` se registra directo como job recurrente
  diario (no necesitó wrapper, ya es una operación global sin iteración por tenant).

**Servidor de Hangfire**: embebido en `Facturacion.Api` (`Program.cs`), no en
`Facturacion.Workers` — un solo proceso para v1, más simple, permite exponer
el dashboard (`/hangfire`) desde el mismo host. `Facturacion.Workers` queda
sin usar hasta que haga falta escalar el procesamiento en un proceso aparte.
Storage: mismo Postgres del servicio (`Hangfire.PostgreSql`), schema propio
`hangfire` (se crea solo en el primer arranque).

**Dashboard** (`/hangfire`): solo se expone si `IsDevelopment()`, sin filtro
de autorización propio — el filtro por defecto de Hangfire (solo IPs locales)
daba 401 falsos en esta máquina por un mismatch de
`Connection.LocalIpAddress`/`RemoteIpAddress` en loopback dual-stack de
Windows. **Gotcha real encontrado verificando con curl**: el 401 inicial NO
era de Hangfire — era `ApiKeyAuthMiddleware` interceptando `/hangfire` antes
de que la request llegara al dashboard (mismo problema que ya resolvía
`/api/v1/admin`). Se agregó la excepción de path correspondiente. TODO:
autenticación real del dashboard (`IDashboardAuthorizationFilter`) antes de
exponerlo fuera de Development.

**Verificado end-to-end con curl real**: `POST /api/v1/facturas` devuelve
202 en ~0.5s con la factura en estado `Pendiente` (antes, con el encolador
síncrono, ya devolvía `Validada` porque todo el ciclo corría dentro del
request) — confirma que ahora es realmente asíncrono. Unos segundos después,
`GET` muestra `Validada` con CUF real. Anulación probada de punta a punta:
`POST .../anulacion` (202) → unos segundos después `GET` muestra `Anulada`.
Los 3 jobs recurrentes quedaron registrados en `hangfire.set` tras el arranque.

Ver también `Application/Commands/AnularFactura/ProcesarAnulacionHandler.cs`:
antes de este trabajo, `EncolarAnulacionAsync` era un no-op explícito — la
anulación nunca se procesaba de verdad.
