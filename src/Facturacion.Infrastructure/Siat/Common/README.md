# Siat/Common — compartido entre modalidades

- [x] `CufCalculator.cs` — cálculo del CUF: concatenación de 52 dígitos, dígito
      auto-verificador módulo 11, codificación base 16, + código de control
      del CUFD. Algoritmo verificado contra el ejemplo oficial del SIN
      ("Generación CUF", www.impuestos.gob.bo, 21/11/2018) — ver
      `CufCalculatorTests`. **Ojo**: `CufDatos.Modalidad` usa los códigos SIN
      (1=Electrónica, 2=Computarizada, 3=Manual), que NO coinciden con el
      enum de dominio `ModalidadFacturacion` (ComputarizadaEnLinea=1,
      ElectronicaEnLinea=2) — quien construya `CufDatos` debe mapear
      explícitamente entre ambos, no castear el enum directo.

- [x] `XmlFacturaBuilder.cs` — construcción del XML de `facturaComputarizadaCompraVenta`
      (documento sector 1) + validación contra el XSD oficial del SIN, embebido en
      `Xsd/facturaComputarizadaCompraVenta.xsd` (descargado de siatinfo.impuestos.gob.bo,
      paquete "Factura de Compra y Venta"). Recibe un `FacturaXmlDatos` explícito
      (no lee `Factura` directamente). Fuente de cada campo que el XSD exige y no
      es un dato directo de `Factura` (decidido con Roberto):
        - `codigoMetodoPago` / `numeroTarjeta` → `Factura.CodigoMetodoPago` /
          `Factura.NumeroTarjeta` (nuevos campos de dominio, ver `EmitirFacturaRequest`).
        - `actividadEconomica` (por ítem) → `Sucursal.ActividadEconomica`, un único
          valor por sucursal repetido en todos los `detalle` de la factura (no hay
          override por ítem en v1).
        - `leyenda` / `usuario` → `SiatOptions.LeyendaFacturaDefault` /
          `SiatOptions.UsuarioSistema` (config fija por instalación, no por tenant).
        - `codigoCliente` → se resuelve en el adaptador como
          `Factura.NumeroDocumentoComprador` (sin campo nuevo).
      Ver `XmlFacturaBuilderTests`: 7/7 en verde, incluyendo un caso que reproduce
      byte a byte los valores del ejemplo oficial del SIN y valida contra el XSD real.
- [x] `SiatOptions.cs` — v1: solo `LeyendaFacturaDefault` y `UsuarioSistema` (lo que
      `XmlFacturaBuilder` necesita hoy). Endpoints piloto/producción, timeouts y
      políticas Polly se agregan cuando se implementen los clientes SOAP.
- [x] `FacturaXmlDatosFactory.cs` — arma `FacturaXmlDatos` combinando `Factura` +
      `Tenant` + `Sucursal` + `PuntoVenta` + CUF/CUFD + `SiatOptions`. Compartido
      entre `Siat/Fake/SiatFakeAdapter` (ya lo usa) y el futuro
      `SiatComputarizadaAdapter`, para no duplicar este mapeo.

- [x] `CredencialesService.cs` + `ISinCredencialesClient.cs` — obtención/renovación
      bajo demanda de CUIS (~1 año) y CUFD (24h) por sucursal/punto de venta,
      contra la entidad `CredencialSiat` (persistida vía `ICredencialSiatRepository`).
      Prioriza la credencial del punto de venta puntual y cae a la de sucursal
      (`PuntoVentaId` null) si no existe una específica — el CUIS/CUFD suele
      gestionarse a nivel sucursal. El token delegado se descifra (`IProteccionDatos`,
      ver `Infrastructure/Seguridad/ProteccionDatosAes.cs`, AES-256-GCM, clave
      maestra por variable de entorno) antes de usarse; nunca se persiste en
      texto plano. `ISinCredencialesClient` es la única pieza que hablará SOAP
      con el SIN — hoy solo tiene la implementación fake
      (`Siat/Fake/CredencialesClienteFake.cs`), ver `Siat/Fake/README.md`.
      Registro/rotación del token delegado: `RegistrarTokenDelegadoAsync`,
      expuesto a Application vía el puerto `IGestorCredencialesSiat` (Domain) —
      endpoint `POST /api/v1/sucursales/{id}/credencial-siat`. Ver
      `CredencialesServiceTests` y `ProteccionDatosAesTests`.

- [x] `CatalogosService.cs` + `ISinCatalogosClient.cs` — sincronización de
      catálogos/paramétricas del SIN (hoy: productos y servicios, actividades
      económicas — `TipoCatalogo`, ampliable a países/motivos de
      anulación/eventos significativos). `ReemplazarAsync` (`ICatalogoRepository`)
      hace un reemplazo atómico por tipo (delete + insert en dos `SaveChanges`
      separados, no uno solo — ver comentario en `EfCatalogoRepository`: evita
      un choque contra el índice único `(Tipo, Codigo)` si EF llegara a
      ordenar el INSERT antes que el DELETE del mismo código dentro de un
      mismo batch). Los catálogos son globales, no por tenant. Se invoca bajo
      demanda hoy (sin job programado todavía, ver Pendientes). Única
      implementación de `ISinCatalogosClient`: `Siat/Fake/CatalogosClienteFake.cs`.
      `ExisteYActivoAsync` queda listo para cuando se decida validar
      `Sucursal.ActividadEconomica` / `DetalleFactura.CodigoProductoSin`
      contra el catálogo sincronizado (hoy pasan sin validar, ver
      `CatalogosServiceTests`). No expone puerto Domain todavía — no hay
      ningún caller en Application aún. Se sincroniza vía job recurrente
      diario de Hangfire (`RecurringJob.AddOrUpdate<CatalogosService>` en
      `Program.cs`), registrado directo sin wrapper — ver `Colas/README.md`.

- [x] `JobRenovacionCufd.cs` — job recurrente de Hangfire (cada 6h) que
      renueva CUIS/CUFD de todas las credenciales registradas, iterando
      `ICredencialSiatRepository.ListarTodasAsync()`. No reemplaza la
      renovación lazy que sigue haciendo `CredencialesService` bajo demanda
      en cada emisión — es una capa extra para no pagar esa latencia en el
      camino caliente. Ver `Colas/README.md` para el resto de los jobs
      (envío de paquetes de contingencia, cola de emisión real).

Ver también `Siat/Fake/README.md`: `SiatFakeAdapter`, la implementación de
`IProveedorFiscal` para desarrollo local (sin el SIN) que ya ejercita todo lo de
arriba end-to-end — ver `SiatFakeAdapterTests` y `EmisionEndToEndTests`.

Pendientes (TODO claude-code):

- `SoapClients/` — clientes generados con dotnet-svcutil desde los WSDL del SIN:
  sincronización de catálogos, códigos (CUIS/CUFD), recepción de facturas,
  operaciones (eventos significativos, puntos de venta). **Pospuesto
  explícitamente por el usuario hasta tener acceso real al SIN** (2026-07-12):
  se investigó que el WSDL real solo se sirve en vivo desde el servidor
  piloto (`pilotosiatservicios.impuestos.gob.bo`), no hay un `.wsdl` estático
  descargable en la documentación pública — así que no se puede generar con
  dotnet-svcutil sin violar la restricción "SIN ACCESO AL AMBIENTE PILOTO DEL
  SIN" de CLAUDE.md. Cuando existan, solo hay que reemplazar el registro DI de
  `ISinCredencialesClient`/`ISinCatalogosClient` (hoy los fakes) por el
  cliente real — `CredencialesService`/`CatalogosService` no cambian.
