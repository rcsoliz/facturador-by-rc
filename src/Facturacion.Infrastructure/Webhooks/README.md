# Webhooks — notificación de cambios de estado al sistema cliente

Implementación de `Domain/Ports/INotificadorWebhook`, invocada por `ProcesarEmisionHandler`
tras cada intento de emisión (validada o rechazada) — ver flujo en `EmitirFactura/ProcesarEmisionHandler.cs`.

- `NotificadorWebhookHttp.cs` — entrega HTTP real. Si el tenant no tiene
  `WebhookUrl`/`WebhookSecretCifrado` configurados, omite el envío sin error
  (silencioso, solo loguea) — no todos los tenants necesitan webhooks. Un
  fallo definitivo (tras los reintentos) tampoco relanza excepción: la
  factura ya quedó persistida en su estado final, solo falló la notificación,
  y no debe hacer fallar el job del worker.
- `FirmaWebhook.cs` — HMAC-SHA256 sobre `"{timestamp}.{cuerpoJson}"`, enviado
  en dos headers: `X-Facturacion-Signature: sha256=<hex>` y
  `X-Facturacion-Timestamp: <unix-segundos>`. Incluir el timestamp en el
  mensaje firmado (no solo en el header) permite que un receptor estricto
  rechace reintentos con timestamp viejo (mitigación de replay). El secreto
  se cifra en reposo (`Tenant.WebhookSecretCifrado`, `IProteccionDatos`,
  mismo patrón que el token delegado SIAT) y se descifra solo al momento de firmar.
- `PoliticasWebhook.cs` — reintentos (3, backoff exponencial 2s/4s/8s) +
  circuit breaker (abre tras 5 fallos consecutivos, 30s) con Polly, aplicados
  al HttpClient nombrado `NotificadorWebhookHttp.NombreCliente` (registro en
  `Program.cs`, no en la clase — así los tests unitarios de
  `NotificadorWebhookHttp` no dependen de temporizadores reales).
- `WebhookFacturaPayload.cs` — DTO del body: `facturaId`, `tenantId`,
  `referenciaExterna`, `estado`, `cuf`, `numeroFactura`, `codigoRecepcionSin`,
  `motivoRechazo`, `fechaEmision`.
- Configuración: `PUT /api/v1/tenant/webhook` (autoservicio, X-Api-Key) —
  `TenantController`, `ConfigurarWebhookHandler` (Application). Solo acepta
  HTTPS absoluta y secreto no vacío.

Verificado end-to-end con curl real (API + Postgres reales, sin el SIN):
onboarding → configurar webhook (URL http rechazada, secreto vacío
rechazado, HTTPS válida aceptada) → secreto confirmado cifrado en la base →
emitir factura → log confirma POST firmado + 3 reintentos + fallo definitivo
logueado (el receptor de prueba no existe) sin romper el flujo de emisión
(factura quedó `Validada`).

Pendiente (TODO claude-code): mover la entrega a un job/outbox propio en
Workers en vez de bloquear `ProcesarEmisionHandler` con los reintentos HTTP
síncronos — aceptable como diseño mínimo v1 porque ya corre en el worker,
fuera del request de la API.
