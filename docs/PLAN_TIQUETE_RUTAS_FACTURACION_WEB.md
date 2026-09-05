# Plan sitio web: Tiquete y rutas de facturación/cobro

Fecha: 2026-09-05. Estado: W0 cerrado; API A1–A7 code-complete; **W1–W3 completas, W4/W6 con base, W5/W7 pendientes**.

## Estado por fase

| Fase | Estado | Qué |
|---|---|---|
| W1 | ✅ | `TipoFacturaFiscalDTO.EsTiquete` (serializa `esTiquete`, compatible con el API y con clientes viejos — `TipoFacturaTiqueteContratoTests`). `TiposFacturaFiscal.razor`: switch "Tiquete" (solo Uso Facturación, sección 3), columna/badge "Modalidad" y filtro. Al cambiar el uso se apaga `EsTiquete`. |
| W2 | ✅ | `SerieFacturacionFiscalDTO.EsTiquete` + `Naturaleza` y `SerieCatalogoTipoFacturaFiscalDTO.EsTiquete` (solo lectura, los llena el API). `SeriesFacturacionFiscal.razor`: badge "Tiquete" en la columna de tipo, badge de naturaleza junto al documento electrónico, y en el modal una nota "Modalidad: Tiquete / Venta con cobro diferido" derivada del tipo. La matriz tipo×serie×condición ya la valida el API (A1.5). |
| W3 | ✅ | `PoliticaRutaFacturacion` (servicio puro: `EntradaSerie` → `GuardarPreventaContado` / `ConfirmarCredito` / `CobrarTiqueteElectronico` / `CobrarTiqueteInterno` / `ConfiguracionInvalida`, 12 casos). Contratos a mano + `FacturaDTO.IdSerie` (partial); proxy `IComandosFacturacion` (`api/facturacion/*` + `api/cobros/estado-cuenta`) registrado. `PerfilEmisionElegibleWebDTO` gana `EsTiquete`/`EsInterna`/`Naturaleza`. **`Facturacion.razor` cableado:** la acción primaria, el bloque de pagos y la llamada salen de la ruta, no de `!esCredito`. No-tiquete contado → "Guardar preventa" (sin captura de pagos) → `CrearPreventaContado`. No-tiquete crédito → "Facturar a crédito" → `ConfirmarVentaCredito`. Tiquete → "Cobrar" + captura de pagos 100% → `CobrarVentaTiquete`. Una sola llamada de comando por confirmación; `IdSerie` explícito; clave de idempotencia por intento (se conserva ante error, se renueva tras éxito/limpiar). Apertura de caja exigida también para la preventa de contado. Resultado recuperable (`MostrarResultadoOperacion`): distingue preventa / crédito / tiquete y estado fiscal (en proceso / interno / NoAplica), nunca "Factura emitida". Consignación y prefactura conservan el flujo legado. Tests: `PoliticaRutaFacturacionTests`, `ComandosFacturacionContratoTests`, `PerfilEmisionRutaTests`. |

| W6 | 🟡 base | Pantalla `EstadoCuenta.razor` (`/sales/account-statement[/{id}]`, gated `VENTAS.ABONO_COBRAR`, sin nodo de menú nuevo): busca cliente, fecha de corte, tiles de límite/saldo/crédito a favor/disponible, tramos de antigüedad (por vencer / 1-30 / 31-60 / 61-90 / 91+) y detalle de facturas con saldo. Fuente única `api/cobros/estado-cuenta/{id}`. Enlace "Estado de cuenta" desde `CuentasPorCobrar`. Falta: PDF/exportación con el mismo corte y protecciones VER/EXPORTAR/IMPRIMIR separadas. |
| W4 | 🟡 base | `PreparacionPagoVenta` (servicio puro §4): `Calcular(total, líneas)` → reparto con recibido/aplicado/vuelto/faltante por forma; invariante `recibido − vuelto = aplicado`; vuelto solo de efectivo; sobrepago no-efectivo y referencia faltante son error. 8 casos en `PreparacionPagoVentaTests` (incluye el escenario E02). `Facturacion.razor` ya deriva `TotalPagado`/`Saldo`/`Vuelto`/`Cubre100` de este servicio. Falta el componente Blazor compartido `PanelPagoVenta` y la captura de referencia por forma; `Cobrar`/`CuentasPorCobrar` aún tienen su reparto propio. |

| W5 | 🟡 base | API: `ServicioFacturarPreventaContado` + `POST api/venta-orquestada/facturar-preventa-contado` — una llamada idempotente que cobra (Cobro/FormasPago/Aplicación + MovimientoCaja), marca `Cobrado` y factura la preventa (inventario + estado); rechaza crédito / anulada / sin apertura / pago < 100%; pulsa la señal fiscal. 11 tests. Web: contratos `FacturarPreventaContadoComandoDTO`/`ResultadoDTO`, `IComandosFacturacion.FacturarPreventaContado`, y `CuentasPorCobrar.CobrarYFacturar` ya no encadena `Cobrar → FacturarPreventa → Emitir`: reparte lo recibido por preventa y hace **una** llamada por documento con clave de idempotencia por lote (reintentar el lote no re-cobra). Falta: componente/servicio 100% compartido con `Cobrar` (`/initial/charge`) y resolver `CAJA.COBRAR` vs `VENTAS.ABONO_COBRAR`. |
| W7 | 🟡 núcleo hecho | La distinción interno/electrónico ya la resuelve el API (A3: título, sin clave, fuera de la cola fiscal) y la web la refleja en `MostrarResultadoOperacion` (W3): "Documento interno, sin espera de Hacienda" vs "Comprobante electrónico en proceso"; nunca XML ni "pendiente de Hacienda" para `NoAplica`. Falta: visor PDF integrado / enlace de descarga recuperable, botón "Reintentar impresión" sin re-cobrar, y estados correo pendiente/enviado/fallido separados en la bandeja. |

> Nota sobre `tools/actualizar-contratos.sh`: regenerar el cliente completo contra el API
> local rompe ~150 archivos por colisión de nombres (el generado histórico se hizo contra
> `devapi` y el API divergió mucho). Se mantiene la convención del repo: **contratos a mano**
> en `DTOs/Fiscal` + partials sobre el `FacturaDTO` generado. Los esquemas a mano se
> validaron contra el swagger del API en ejecución.

---


## W0 — decisiones cerradas (2026-09-05)

Idénticas a las de `DevSuvesaPosWeb/docs/PLAN_TIQUETE_RUTAS_FACTURACION_API.md` §A0:

1. Agente: se mantiene apertura obligatoria para crear preventa/crédito.
2. Tiquete + crédito: **bloqueado** en API y UI.
3. No tiquete + serie no electrónica: **existe** como "venta interna diferida" (preventa
   contado o venta a crédito → documento interno sin Hacienda, fiscal `NoAplica`). Dos rutas
   más en la matriz.
4. Preventa contado: pago **total en una operación**; no se emite hasta saldo 0.
5. Impresión: se permite representación **provisional con estado "pendiente"**.
6. Correo de crédito: **al aceptar Hacienda**, sin esperar el cobro.
7. Recibo: registro único automático tras el pago (también contado); impresión adicional
   configurable.
8. Series duplicadas por ámbito: **no** (se mantiene el bloqueo actual).
9. Entrega de mercadería / duración de preventa: pendiente de afinar antes de W4.
10. Estado de cuenta (corte/antigüedad/moneda/exportación): pendiente de afinar antes de W6.

### Matriz de rutas (UI)

| Modalidad | Acción principal | Antes de cobrar | Al confirmar |
|---|---|---|---|
| No tiquete + contado (electrónico o interno) | "Guardar preventa" | Preventa visible sin recibo | Cobro 100% desde Cobrar → factura/documento + recibo |
| No tiquete + crédito (electrónico) | "Facturar a crédito" | Ninguno | Factura + plazo + saldo CxC; emisión pendiente/aceptada |
| No tiquete + crédito (interno) | "Facturar a crédito" | Ninguno | Venta a crédito con documento interno (sin Hacienda) |
| Tiquete + electrónico | "Cobrar" | Borrador de UI, sin preventa | Modal pagos → 100% → venta/cobro → emisión + impresión pendiente |
| Tiquete + interno | "Cobrar" | Borrador de UI, sin preventa | Modal pagos → 100% → venta/cobro → impresión interna |
| Tiquete + crédito | — | — | **Bloqueado** en la UI |

---



Repositorio: `SuvesaPosSitieWebNew`. Rama conservada: `feature/ola-0-cimientos`. HEAD observado: `72e97be5912c22a514412f1449a2741617845b38`. No se modificó código, no se regeneraron contratos, no se ejecutaron operaciones de venta ni se cambiaron ramas.

Plan API complementario: `DevSuvesaPosWeb/docs/PLAN_TIQUETE_RUTAS_FACTURACION_API.md`. Los comandos y estados nuevos de este documento son propuestas; no deben consumirse hasta que el API implemente y publique sus contratos.

## 1. Resultado funcional esperado

La pantalla conserva el orden cliente → condición → serie → bodega → artículos. El agente puede usar tablet. La modalidad depende de **Tiquete en el tipo de facturación**, mientras la serie conserva condición, emisor/ámbito y requisito electrónico.

| Modalidad | Acción principal | Documento antes de cobrar | Resultado de confirmar |
|---|---|---|---|
| No tiquete + contado | Guardar preventa / Enviar a cobro | Preventa, visible sin recibo | No factura ni cobro en esta pantalla; el cajero confirma pago total y factura desde Cobrar |
| No tiquete + crédito | Facturar a crédito | Ningún recibo necesario | Factura, plazo y saldo CxC; emisión electrónica pendiente/aceptada; visible en Cobrar |
| Tiquete + electrónico | Cobrar | Borrador de interfaz, NO preventa persistida | Modal de pagos → 100% → venta/cobro → emisión y presentación de impresión |
| Tiquete + no electrónico | Cobrar | Borrador de interfaz, NO preventa persistida | Modal de pagos → 100% → venta/cobro → impresión interna sin Hacienda |

La instrucción general de generar preventa para no tiquete tiene una excepción explícita para crédito: debe confirmarse la factura inmediatamente, sin esperar conversión por caja. La elección de contado/crédito no puede cambiar silenciosamente al seleccionar una serie.

No deducir Tiquete de `CodigoFe == 04`, del título de la serie ni del formato 80 mm. Mostrar “Tiquete” como modalidad de atención y “Electrónico / Interno” como naturaleza del documento. Ser tiquete no significa necesariamente ser electrónico.

## 2. Diagnóstico de las pantallas y contratos actuales

Rutas de archivo relativas al repositorio; líneas orientativas.

| Pieza actual | Evidencia | Brecha para este requerimiento |
|---|---|---|
| Mantenimiento de tipos | `Views/Parametros/TiposFacturaFiscal.razor` y `.razor.cs`; `DTOs/Fiscal/TipoFacturaFiscalDTO.cs` | Solo Id/Código/Descripción/Uso/Activo; falta EsTiquete en alta, edición, copia y consulta |
| Tipo consumido por venta | `DTOs/Fiscal/TipoFactura.TipoDocumento.cs`; `DTOs/Generated/SeePosDtos.cs`; `ApiConexion/ProxyClass/Facturacion.cs` | Existe otra representación del tipo además del DTO manual: deben converger en el nuevo contrato |
| Mantenimiento de series | `Views/Parametros/SeriesFacturacionFiscal.razor(.cs)` | Ya administra condición y RequiereDocumentoElectronico/CodigoFE; no debe duplicarse el switch Tiquete en la serie como dato independiente |
| Captura de venta | `Views/Ventas/Facturacion.razor:75-116` | El orden condición/serie/bodega existe; aprovecharlo, sin rehacer todo el formulario |
| Pago inmediato para cualquier contado | `Facturacion.razor:589`, `MostrarCobroContado` | Decide por !crédito y !consignación, no por Tiquete; impide la preventa ordinaria nueva |
| No tiquete contado no genera preventa | `Facturacion.razor:1578` | `Preventa = EsPrefacturaConsignacion`; para venta ordinaria envía false |
| Serie elegida no viaja por Id | `Facturacion.razor:1577-1578`; `ProxyClass/Facturacion.cs` | Se manda Tipo y terminal. API vuelve a resolver; contrato debe enviar IdSerie exacto y recibirlo confirmado |
| Internas no elegibles | `Facturacion.razor:681-704` y `Class/Cobros/PerfilesEmisionManager.cs` del API | El sitio depende de elegibilidad que rechaza sin emisión 4.4. Cambiar contrato antes que habilitar opciones a mano |
| Crédito valida disponible, pero no captura/envía plazo | `Facturacion.razor:1554-1578` | Falta mostrar plazo configurado, vencimiento y enviar selección permitida; API actual copia IdPlazo y mapper lo exige |
| Éxito comercial se anuncia como fiscal | `Facturacion.razor:1591` | Muestra “Factura emitida correctamente” y limpia; no distingue cola/aceptación ni conserva acceso inmediato al PDF |
| Cobrar antiguo | `Views/Ventas/Cobrar.razor`, ruta `/initial/charge` | Cobra y después convierte preventa con otra llamada; sin impresión. Es ruta visible del menú, no código muerto |
| Cobrar ampliado | `Views/Ventas/CuentasPorCobrar.razor(.cs)`, `/sales/collect` | Ya incluye preventas, crédito, recibos y operaciones fallidas; no es solo lectura, aunque AGENTS conserva esa descripción histórica |
| Cobro de preventas orquestado desde la vista | `CuentasPorCobrar.razor.cs:236-354` | Por documento llama Cobrar → FacturarPreventa → EmitirFactura/Tiquete; se puede perder conexión entre pasos |
| Crédito y recibo tienen base aprovechable | `CuentasPorCobrar.razor.cs:70-113`; `ProxyClass/CobrosCredito.cs` | Reutilizar, con validación transaccional en API y recuperación de operación |
| Boleta ya implementada | `Views/Ventas/TramiteCobro.razor(.cs)`; `ProxyClass/TramitesCobro.cs`; ruta `/sales/collection-process` | Integrar selección del cliente/facturas y saldo; no crear otro módulo paralelo |
| Impresión ya tiene infraestructura | `ProxyClass/ImpresionDocumentos.cs`; `Views/Parametros/PlantillasImpresion.razor(.cs)` | Reutilizar PDF servido por sitio; agregar tiquete interno y respuesta rápida con estado fiscal correcto |
| Pantalla declarada de escritorio | `Facturacion.razor:37` | Auditar restricciones de AppPantalla y adaptar facturación/cobro a 768 px y orientación vertical; no basta que Bootstrap acomode columnas |
| Cantidad mínima incorrecta para fracciones | `Facturacion.razor:1499`, `linea.Cantidad = Math.Max(1, linea.Cantidad)` | Al recalcular 0,375 kg se eleva a 1; debe corregirse en implementación con precisión/unidad del artículo y prueba de regresión |
| Pruebas de operación con transporte simulado | `tests/...Tests/ContratosOperacionDiariaTests.cs` | Verifican rutas/cuerpos con respuestas prefabricadas; no prueban dinero, stock ni aceptación fiscal |

No se levantaron las aplicaciones ni se confirmó su estado en producción. No se realizaron pruebas de venta con datos reales. Las limitaciones aquí descritas se desprenden del código local, no de una medición de tiempos o una reproducción en Hacienda.

## 3. Plan de pantallas y componentes

### W1. Tipos de facturación

- Agregar switch accesible “Tiquete”, enlazado a `EsTiquete`; ayuda: “Cobra y confirma la venta en esta pantalla, sin crear preventa. La serie define si requiere documento electrónico”.
- Mostrarlo solo en Uso Facturación. No permitir guardarlo activo en Compra/Devolución/Consignación; presentar el error del API sin normalización silenciosa.
- Incluir columna/badge y filtro en consulta; preservar valor al editar/clonar. No deducirlo del texto “Tiquete electrónico”.
- Respetar permisos CREAR/EDITAR y cambios de uso; mostrar impacto si el tipo ya tiene series. Modificar un tipo usado no debe reinterpretar documentos guardados.
- Revisar todos los DTOs/mapeos. El endpoint de mantenimiento y la lista consumida por Facturación deben exponer el mismo valor.
- Usar AppModal/controles comunes, no crear otra abstracción de formularios. Si se conserva el modal legado durante un primer lote, documentar su migración sin expandir a todas las pantallas.

### W2. Series

- Mostrar “Modalidad: Tiquete / Venta con cobro diferido”, derivada del tipo, como dato informativo.
- Conservar condición Contado/Crédito, Requiere documento electrónico y selección fiscal. La matriz del servidor debe guiar las combinaciones permitidas.
- Propuesta: tipo Tiquete obliga contado; si es electrónico, 04. Tipo no Tiquete electrónico de venta usa 01. NC queda fuera de facturación ordinaria.
- No hacer que desactivar “Emisión 4.4 habilitada” convierta una serie que requiere electrónico en interna. Explicar configuración pendiente y bloquear emisión si corresponde.
- Elegibilidad debe incluir internas válidas, y motivos para tipos inactivos, ámbito no autorizado, falta de configuración o condición incompatible.
- Enviar IdSerie exacto; no sustituir por primera serie ni derivar por terminal. Mostrar empresa, sucursal y terminal de la serie, diferenciándola de la caja donde se recibe dinero.
- Versionar selección: si la configuración cambia mientras hay líneas o pagos preparados, pedir revalidación; no continuar con información vieja.

### W3. Captura compartida cliente → condición → serie → bodega → artículos

Mantener la ruta real `/initial/billing` y las pestañas de ventas. No abrir otra pantalla en una ruta desconectada del menú ni duplicar Facturación para tablet.

1. Cliente: datos fiscales, sucursal del cliente, correo, crédito, moneda/precio aplicable, orden de compra obligatoria y condiciones comerciales.
2. Condición: contado o crédito. Al cambiar, invalidar serie incompatible, plazo y pagos preparados. Conservar líneas solo si el usuario confirma y se recalculan precios/impuestos afectados.
3. Serie: catálogo elegible por emisor/sucursal/condición, con etiqueta de modalidad y naturaleza. La respuesta del API es la autoridad; esconder una opción no sustituye validar al guardar.
4. Bodega: permitida al usuario/sucursal, stock por bodega/lote. Al cambiar con líneas, confirmar y reconsultar disponibilidad/lotes; no mover los artículos silenciosamente.
5. Artículos: preservar búsqueda, imágenes, lotes, bonificaciones, descuentos, exoneración/CABYS y actividades fiscales. Cantidades decimales según unidad, incluidos pesos menores de 1 kg; unidades enteras cuando el artículo lo exija.
6. Resumen: total, condición, modalidad, documento a emitir, plazo/vencimiento cuando corresponda. Acción primaria calculada desde la matriz, no desde un bool “es crédito” solamente.

Extraer la política de presentación a un servicio puro y testeable (p. ej. `PoliticaRutaFacturacion`), con resultado `GuardarPreventa`, `ConfirmarCredito`, `CobrarTiqueteElectronico`, `CobrarTiqueteInterno` o `ConfiguracionInvalida`. La validación financiera definitiva permanece en API.

#### No Tiquete / contado

- Acción “Guardar preventa” y texto “Quedará pendiente en Cobrar; todavía no se emite la factura electrónica”. No mostrar captura de pagos obligatoria aquí.
- Enviar comando CrearPreventaContado; sin recibo previo, sin llamada fiscal posterior desde la vista.
- Al guardar mostrar número operativo, cliente, total, estado y enlace a Cobrar con filtro del documento. No decir “Factura emitida”.
- Preservar ficha si sigue siendo necesaria para caja; buscar también por número/cliente, no depender de una ficha no asignada.
- Validar la política de apertura de captura que se acuerde para el agente. Hasta que se autorice excepción, conservar el requerimiento previo de apertura para preventas.

#### No Tiquete / crédito

- Mostrar habilitación, límite, saldo y disponible como orientación. La verificación final debe realizarse en API bajo concurrencia.
- Mostrar plazo del cliente y vencimiento; si solo tiene un plazo, usarlo sin entrada libre. Si tiene varios autorizados, seleccionar uno; no ofrecer todo el catálogo indiscriminadamente.
- Acción “Facturar a crédito”. No pedir monto ni crear recibo para que aparezca en Cobrar.
- Respuesta: venta confirmada, saldo y vencimiento, estado fiscal separado, PDF cuando esté listo, enlaces a estado de cuenta y trámite de cobro.
- Nunca crear una preventa adicional para que otro usuario complete manualmente el crédito. Si el API reutiliza internamente estructuras, la operación visible debe quedar confirmada en un único comando recuperable.

#### Tiquete

- Acción “Cobrar” abre un componente compartido de pagos. El borrador en la interfaz no consume serie, stock ni genera preventa.
- Cobertura del 100% del total aplicado; cancelar el modal vuelve al borrador sin operación confirmada. Validar que la serie siga vigente antes de enviar.
- Enviar CobrarVentaTiquete como un solo comando. Desactivar doble envío visualmente y usar la misma clave idempotente ante reintento real.
- Confirmado: conservar IdOperacion/IdVenta/IdCobro y ofrecer impresión en pantalla de inmediato cuando esté disponible. No limpiar la venta antes de mostrar un resultado recuperable.
- Electrónico: “Cobrado. Comprobante en proceso” hasta que exista estado posterior; actualizar mediante consulta acotada o notificación. No prometer aceptación instantánea.
- Interno: “Cobrado. Documento interno” y PDF sin espera fiscal. Nunca mostrar XML o “pendiente de Hacienda” para NoAplica.
- Reimprimir nunca invoca el comando de guardar/cobrar otra vez.

### W4. Componente compartido de pagos

Propuesta `PanelPagoVenta`/`DialogoCobro`, reutilizado por Tiquete y Cobrar, con lógica de preparación en Services. No replicar las tres implementaciones actuales de reparto.

- Métodos configurados por API, activos y permitidos; no asumir que efectivo siempre tiene código interno 01. Respetar semántica EsEfectivo/RequiereReferencia y moneda.
- Resumen: total, aplicado, recibido, vuelto y faltante; mostrar apertura/cajero. Tarjeta/transferencia deben conservar referencia obligatoria cuando corresponda.
- Validar 100% aplicado, no 100% recibido exactamente: recibir efectivo mayor es válido si el vuelto sale de efectivo. No permitir sobrepago no efectivo como vuelto ficticio.
- Dinero en decimal en UI; conversiones al DTO viejo no deben perpetuarse en contratos nuevos.
- Pago parcial solo para crédito por defecto propuesto. Si se autoriza parcial en preventa contado, mostrar pendiente y NO pedir emisión hasta saldo cero.
- Para varias facturas, mostrar cómo se aplicará el pago y saldos restantes. No repartir entre monedas/emisores distintos sin política explícita del API.
- No interpretar respuesta vacía/HTTP 200 como cobro confirmado. Usar el resultado de negocio y errores por campo.
- Cambio de caja, sesión caducada o apertura cerrada mientras el modal está abierto: bloquear y refrescar, sin perder el borrador ni generar una segunda operación.

### W5. Cobrar y Abono Cobrar: una sola experiencia, sin quitar permisos

Hoy `/initial/charge` y `/sales/collect` siguen siendo rutas distintas del menú. La segunda está más avanzada, pero la primera todavía permite un flujo diferente. Deben compartir componente/servicio y comandos centrales.

Propuesta: mantener ambas rutas por compatibilidad, con una interfaz común de pendientes de contado, facturas de crédito, recibos y errores fiscales. Resolver explícitamente las diferencias `CAJA.COBRAR` / `VENTAS.ABONO_COBRAR`; una redirección no debe conceder permisos que el usuario no tiene.

- Pendientes contado: preventas por estado, no por recibo. Filtros cliente/nombre/cédula/número/ficha/fecha/agente/sucursal; lista paginada y refresco controlado para que las capturas de ruta aparezcan sin recargar toda la app.
- Crédito: facturas con saldo aunque no haya recibo; vencimiento, días, estado fiscal, original, abonos, NC y saldo. Acciones de pago parcial/total permitidas.
- Recibos: creados después del dinero confirmado, con aplicaciones y apertura; imprimir/reimprimir existente.
- Operaciones fallidas: separar pago fallido, resultado desconocido y emisión fiscal fallida después de cobro. Solo esta última ofrece recuperación fiscal, no “volver a cobrar”.
- Reemplazar el ciclo actual Cobrar → FacturarPreventa → Emitir en la vista por un comando del API. Si se permite cobrar varias preventas, acordar atomicidad por lote o resultado por documento; nunca ocultar éxitos parciales.
- El API devuelve el reparto final. La pantalla no debe alterar saldos localmente como si fueran definitivos ni usar Total original como saldo cuando existen abonos.
- Consultar resultados tras timeout antes de habilitar un nuevo cobro. Idempotencia persiste por operación/pestaña; una recarga no debe generar una nueva clave para un pago en estado desconocido.

### W6. Estados de cuenta y boleta de trámite

- Reutilizar Trámite de cobro existente en `/sales/collection-process`; completar navegación desde cliente/Cobrar y selección de pendientes, sin emitir recibos ni tocar apertura por crear la boleta.
- Conservar regla implementada: monto comprometido = saldo completo vigente; factura puede aparecer en varias boletas. Fecha prometida no reemplaza vencimiento fiscal/comercial.
- Separar monto histórico de boleta de saldo actual; opcional seguimiento Pendiente/Cumplida/Vencida consultando aplicaciones reales.
- Estado de cuenta: no basta imprimir la pestaña de pendientes. Requiere API con saldo inicial al corte, movimientos, pagos/NC/anulaciones, saldo final, antigüedad y moneda. Consulta, PDF y exportación deben usar el mismo filtro/corte.
- Crear reporte/pantalla solo después de verificar reportes legados disponibles; lo observado no certifica que un reporte externo no exista.
- Protecciones de VER/EXPORTAR/IMPRIMIR y ámbito cliente/sucursal; si hay envío por correo, mostrar destinatario y resultado sin exponer credenciales.

### W7. Impresión y bandeja fiscal

- Reutilizar `/documentos/{tipo}/{id}/pdf` y proxies de impresión del servidor; JWT permanece fuera del navegador y de las URLs.
- Agregar tipo/plantilla para tiquete interno, selección A4/80 mm y preferencia por emisor/serie/usuario cuando se acuerde. No confundir formato térmico con naturaleza electrónica.
- Presentar visor integrado o enlace de descarga recuperable. `window.open` después de varios await puede requerir gesto adicional: probar bloqueadores de ventanas en tablet; no depender de que siempre se abra una pestaña.
- La solicitud pide mostrar impresión. Impresión silenciosa en impresora Bluetooth/USB, gaveta y báscula no quedan incluidas automáticamente: serían capacidades distintas que necesitan decisión y prueba de dispositivo.
- Mostrar estado actual de comprobante, acceso a PDF disponible, XML firmado/respuesta solo cuando existan. PDF provisional debe indicar su condición; no simular aceptación.
- Si PDF falla con pago confirmado, conservar recibo/venta y botón Reintentar impresión. Cancelar el diálogo de impresión tampoco anula la venta.
- Informar correo pendiente/enviado/fallido separadamente. Mantener paquete fiscal con tres adjuntos y política de crédito definida por API.

## 4. Arquitectura web y archivos a intervenir al implementar

Conservar Blazor Server, Bootstrap/Havit y las capas del repositorio; no introducir una SPA o stack de UI nuevo.

| Capa | Trabajo |
|---|---|
| DTOs/Fiscal y DTOs/Ventas/Cobros | Consumir EsTiquete, IdSerie explícito, plazo, estados, importes aplicados y resultado operativo; resolver duplicación manual/generada |
| API generado | Regenerar con `tools/actualizar-contratos.sh` contra el API actualizado; nunca editar Generated a mano |
| ProxyInterface / ProxyClass | Exponer comandos y consultas atómicas. Mantener ProxyBase/contexto de token, traducción de envelopes y manejo de excepciones |
| Services | Política de rutas, estado de cobro recuperable, preparación de pagos; la autorización y contabilidad definitivas son del API |
| Models | Borrador y resultado por pestaña; separar venta en edición de operación confirmada |
| Views/Parametros | Switch, series compatibles y ayuda contextual |
| Views/Ventas | Facturacion, Cobrar y CuentasPorCobrar comparten flujo, no copias de lógica financiera |
| Views/Consignacion/Prefactura | Adaptador hacia política de cobro/emisión, conservando conteo/aprobación y tipo de stock |
| Impresión/bandeja | Documento interno y estados de emisión/recuperación; no llamadas fiscales encadenadas desde la vista |
| Menú/seguridad | Mantener rutas accesibles y acciones correctas; coordinar códigos con catálogo del API solo si se agregan funciones |

Usar IServicioDialogos, IManejadorRespuestas, AppModal, AppRejilla/AppFiltros y campos comunes. No llamar directamente a HttpClient desde Views ni hacer que una View vea ApiException. Mantener la sesión del API en servidor; no usar localStorage para tokens o comprobantes sensibles.

Compatibilidad: no habilitar el switch contra un API que no lo persiste. El contrato debe distinguir campo ausente de false al actualizar desde clientes antiguos, o versionar la actualización para no apagar tipos existentes. Exponer versión/capacidad del flujo antes de habilitar comandos nuevos; no hacer fallback silencioso al cobro legado tras un timeout.

## 5. Tablet, conectividad y usabilidad de almacén

- Facturación hoy tiene nivel Escritorio; incluir tablet como objetivo verificado en 768×1024 y 1024×768, teclado táctil visible, modal de pagos y listas largas.
- Controles táctiles, foco y lectura de código de barras; resumen y acción primaria siempre localizables sin tapar líneas. Probar navegar entre artículos sin perder cantidad/lote.
- `onchange` en captura rápida de cantidades/precios, sin viaje de servidor por cada tecla; búsqueda con debounce acotado y cancelación de resultados viejos. No rediseñar conectividad como parte de un ajuste visual.
- Preservar 0,375 kg y 1,250 kg: validación por unidad, separador decimal regional y precisión permitida. Ni UI ni PDF deben convertirlo a 1 unidad.
- Blazor Server necesita conexión activa. “Usar tablet en ruta” no equivale a modo offline. Propuesta inicial: sin confirmar cobros/ventas offline; guardar borrador seguro y recuperar operación al reconectar. Offline real requiere diseño separado de sincronización, numeración, stock, seguridad y conflictos.
- Tras perder conexión durante cobro, estado “Resultado por confirmar”; consultar API por operación/idempotencia. No invitar a cobrar otra vez sin resolverla.
- Evitar perder una venta confirmada por Limpiar, recarga o cierre de pestaña; conservar tarjeta de resultado y acceso en historial. Para borradores no confirmados, confirmar descarte.
- Una sesión con varias ventas abiertas necesita clave/estado por pestaña, no un único pago compartido que mezcle clientes.
- Rendimiento: medir catálogo, búsqueda, cálculo, confirmación local, PDF y espera fiscal por separado en red de ruta; fijar objetivos después de medir, no prometer tiempos de Hacienda.

## 6. Integraciones que deben incluirse en la regresión

1. Caja: apertura efectiva al cobrar, arqueo por moneda/medio, cierre que congela importes, devolución posterior en apertura autorizada. Vendedor, cajero y terminal fiscal no son el mismo dato.
2. Consignación: la facturación del consumo debe aplicar las mismas reglas de pago/crédito sin perder prefactura/aprobación/liquidación. No enviar toda existencia remanente a venta; reporte final sigue independiente.
3. Bonificación: preservar grupos, artículos regalados, límites de descuento y tributación; evitar una limpieza de formulario que pierda sus vínculos.
4. Carne/producción: lotes, caducidad, cantidades de Kg y saldos por bodega; no consumir materia prima al cobrar producto ya producido.
5. Compras/órdenes/recibos de pago/traslados/tomas: sus catálogos y plantillas no deben mostrar Tiquete ni recibir la nueva condición por error.
6. Devoluciones/NC: distinguir reversión comercial, fiscal y monetaria; no ofrecer borrar cobros confirmados o reabrir caja cerrada para ajustar una impresión.
7. Proformas, albaranes y rutas antiguas: sus conversiones a factura deben usar la política vigente y la serie seleccionada, no una emisión inmediata no autorizada.
8. Seguridad: login real, Bearer hacia API, PDF autorizado, consulta de saldos por ámbito, permisos para agente/cajero/supervisor. Desbloquear con clave interna no sustituye permisos de negocio.

## 7. Fases del sitio y dependencias del API

| Fase web | Entrega | Requisito API / verificación |
|---|---|---|
| W0 | Cerrar matriz y UX, inventario de rutas/permisos | Decisiones §9; identificar reporte de estado de cuenta existente si lo hay |
| W1 | Contratos + switch + series | API A1 publicado; round-trip y compatibilidad de DTOs |
| W2 | Política de rutas y captura tablet | Elegibilidad incluye internas y serie exacta; cuatro acciones correctas |
| W3 | Pagos compartidos y resultado recuperable | API A2/A3; idempotencia, 100%, vuelto, apertura; sin encadenar escrituras |
| W4 | Crédito/plazo y Cobrar común | API A4; deuda visible sin recibo, cobros parciales y vencimiento |
| W5 | Impresión/bandeja rápida | API A5; estados y PDFs disponibles, interno sin Hacienda |
| W6 | Estado de cuenta y enlace a boletas | Consulta por corte y saldo consistente; reutilizar boleta existente |
| W7 | Consignación, legado y regresión | API A6/A7; contratos finales, pruebas conjuntas y piloto controlado |

No activar una pantalla adelantada al contrato del API ni esconder errores devolviendo éxito local. Desplegar por ámbito/caja piloto después de verificar las cuatro rutas y reconciliación. Si hay rollback funcional, las operaciones nuevas siguen consultables/recuperables, no se vuelven a cobrar por la pantalla vieja. No se realizó ningún despliegue en esta tarea.

## 8. Plan de pruebas: no basta con unitarias

Las pruebas actuales de cálculo, contratos, permisos, impresión y E2E son una base. No se ejecutaron ni se añadieron pruebas en esta tarea documental. Antes de implementar hay que comprobar qué suites ejercitan realmente el servidor y cuáles usan datos simulados.

### Unitarias

- Política de las cuatro rutas y todas las combinaciones inválidas; tipo ausente/inactivo o respuesta de catálogo vieja.
- Alta/edición de EsTiquete conservando valor; serialización/mapeo del DTO manual y generado.
- Preparación de pagos: insuficiente, exacto, efectivo con vuelto, tarjeta sin vuelto, multimedios, métodos inactivos, referencias y separación de monedas.
- Cantidades 0,375 y 1,250 kg; precios, redondeos, bonificaciones, máximos descuentos e impuestos configurados.
- Cambio de cliente/condición/serie/bodega invalida solo dependencias correctas; no reutiliza pago o plazo de otro cliente.
- Estado por pestaña, clave idempotente estable, timeout vs error definitivo, recuperación sin segundo submit.
- Mensajes y acciones según estado: confirmado ≠ enviado ≠ aceptado; impresión fallida no borra venta; fiscal NoAplica no consulta Hacienda.

### Componentes y contratos

- Switch accesible, permisos de edición, mensajes de validación del API y combos compatibles.
- Acción Guardar preventa/Facturar a crédito/Cobrar según matriz; pagos no visibles obligatoriamente en no tiquete contado.
- Modal cancelado no invoca guardar; doble clic no envía dos comandos; apertura inválida mantiene formulario.
- Una sola llamada de comando por confirmación; no Cobrar+FacturarPreventa+Emitir desde la vista.
- 401 sesión vencida y 403 permiso insuficiente tratados de forma distinta; no exponer token/XML sensible en consola.
- Contratos reales generados, enums/nombres JSON, errores HTTP y envelopes; no dar por bueno solo un mock que devuelve HTTP 200.

### E2E representativo con API + SQL aislados

| Caso | Flujo y aserción |
|---|---|
| E01 | Agente crea no tiquete contado de 10.000: queda en Cobrar sin recibo ni clave fiscal; otro cajero la encuentra |
| E02 | Cobra tarjeta 4.000 + efectivo 8.000, vuelto 2.000: una factura/recibo; XML aplicado 4.000+6.000 y efectivo neto 6.000 |
| E03 | Crédito 25.000, límite 100.000, deuda previa 70.000, plazo 15 días: saldo total 95.000, disponible 5.000, sin recibo inicial |
| E04 | Abono efectivo 10.000 a esa factura: saldo 15.000, disponible 15.000; recibo único e impresión; no reemitir factura original |
| E05 | Tiquete electrónico de carne 0,375 kg y 1,250 kg: modal → pago 100% → cero preventa → stock correcto → impresión recuperable |
| E06 | Tiquete interno: cero envío/reserva fiscal, sin XML ni estado pendiente; título interno correcto |
| E07 | Sin apertura, apertura de otro ámbito o cierre durante el modal: API rechaza sin dinero/stock parcial |
| E08 | Dos cajas intentan cobrar la misma preventa, o red cae después del commit: resultado único y recuperación sin duplicados |
| E09 | Hacienda lenta/rechazada, PDF falla o usuario cancela impresión: el pago continúa confirmado; solo reintentar la etapa pendiente |
| E10 | Boleta desde pendientes y posterior abono: constancia histórica intacta, saldo actual cambia; no movimiento de caja por generar boleta |
| E11 | Consignación 10 bebidas, conteo 4, factura 6, reposición 5: reporte 9; sin descuento doble de bodega |
| E12 | Fondo 50.000 + efectivo neto 6.000 + abono 10.000: arqueo efectivo 66.000; tarjeta 4.000 separada; cierre y reimpresión sin mutaciones |
| E13 | Dos pestañas/clientes, cambio de sucursal, sesión expirada, reconexión tablet: no cruzar líneas, pagos, series o cajeros |
| E14 | Regresión de compras, órdenes, producción, devoluciones, bonificaciones y plantillas: el nuevo switch no altera esos usos |

Ejecutar responsive con teclado táctil, orientación y visor PDF. Integración SQL debe afirmar saldos/stock/recibos persistidos; los E2E no pueden limitarse a que aparezca un toast. Hacienda/SMTP/pasarela simulados para fallos reproducibles y ambiente de pruebas autorizado para integración externa. No utilizar clientes o correos de producción.

## 9. Decisiones que faltan y recomendaciones

| Decisión | Propuesta, pendiente de aprobación |
|---|---|
| Apertura para agente de ruta | Conservar regla previa hasta definir caja de ruta/excepción. Caja de captura y caja del cobro deben distinguirse |
| Tiquete a crédito | Bloquear: el requerimiento exige 100% al confirmar |
| No tiquete no electrónico | No habilitar automáticamente; definir si existe esta operación |
| Contado con pagos parciales/anticipos | Primera entrega con pago total; si se necesitan parciales, modelar saldo y no emitir hasta 100% |
| Impresión antes de respuesta Hacienda | Permitir representación con estado pendiente si se aprueba; no presentar aceptación inexistente |
| Correo de crédito | Enviar tras aceptación, no esperar cobro; confirmar frente al requisito previo de correo después de pagar |
| Recibo además de tiquete | Registro único automático tras cobro; impresión adicional configurable |
| Impresora física/offline | Fuera del simple visor PDF; alcance independiente si se requiere Bluetooth, impresión silenciosa o venta sin red |
| Entrega de mercadería | Acordar cuándo se reserva/descuenta/entrega y cuándo expira una preventa |
| Estado de cuenta | Corte, antigüedad, monedas y exportación; buscar reporte existente antes de construir |

La posibilidad de documento interno no equivale a permiso tributario para omitir emisión. Antes de ponerlo en producción, el responsable fiscal debe validar qué operaciones pueden usarlo. Este documento no es una auditoría normativa.

## 10. Cierre de aceptación

El trabajo futuro estará terminado cuando los cuatro flujos funcionen desde el menú real en escritorio/tablet, las preventas y créditos aparezcan sin recibo previo, todo pago quede conciliado en su apertura, la serie seleccionada se conserve, no haya cobros/emisiones duplicados por reintentos, crédito tenga plazo y saldo correcto, el tiquete interno no espere a Hacienda, las impresiones indiquen su naturaleza real y las pruebas de transacciones y operación diaria estén ejecutadas con evidencia. No declarar terminado solo por añadir el switch o compilar las pantallas.
