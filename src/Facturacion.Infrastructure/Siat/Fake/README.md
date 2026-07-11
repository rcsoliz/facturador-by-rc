# SiatFakeAdapter — desarrollo local sin el SIN

Implementación de `IProveedorFiscal` solo para desarrollo local. Ver la restricción
"SIN ACCESO AL AMBIENTE PILOTO DEL SIN" en `CLAUDE.md`: mientras no haya NIT ni
credenciales SIAT, todo el desarrollo se prueba contra esto, nunca contra el SIN real.

- Usa `CufCalculator` + `XmlFacturaBuilder` + `FacturaXmlDatosFactory` reales (mismo
  código que usará `SiatComputarizadaAdapter`) para generar un CUF y un XML válidos
  contra el XSD oficial — solo la respuesta de *envío* al SIN es simulada.
- CUFD/código de control **fijos y falsos** (`SiatFakeAdapter.CufdFake`): no hay
  `CredencialesService` todavía. No representan un CUFD real ni deben tratarse como tal.
- Comportamiento configurable vía `SiatFakeAdapterOptions`:
  - `SimularSinIndisponible` — fuerza `ComunicacionDisponibleAsync() == false` (contingencia).
  - `PrefijoParaRechazo` (default `"RECHAZAR-"`) — si `Factura.ReferenciaExterna`
    empieza con ese prefijo, `EnviarAsync` simula un rechazo. Permite disparar el
    escenario de rechazo desde el propio request REST sin tocar configuración.
- Registrado en DI (`Program.cs`) como `IProveedorFiscal` mientras no exista
  `SiatComputarizadaAdapter` real. Cuando exista, la selección deberá ser por
  `ModalidadFacturacion`/ambiente (factory), no un reemplazo directo — este adapter
  debería seguir disponible para desarrollo local incluso después.

Ver `Siat/Common/README.md` para el resto de las piezas de Siat/Common.
