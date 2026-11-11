# UAT en vivo — hallazgos y mejoras

Documento de trabajo de la rama `feature/modulo-contabilidad`. La misma copia vive en:

- `DevSuvesaPosWeb/docs/UAT_CONTABILIDAD_EN_VIVO.md`
- `SuvesaPosSitieWebNew/docs/UAT_CONTABILIDAD_EN_VIVO.md`

Se actualiza al cerrar cada prueba. Lo que ya está en «Cerrado en esta pasada» no se vuelve a ejecutar. Lo abierto se sigue desde «Mejoras pendientes».

**Fecha de la pasada inicial:** 2026-11-05 (caja 2026-11-06)  
**Última re-prueba:** 2026-11-11 (noche, re-validación post-#26)  
**Entorno:** Blazor `http://localhost:5199`, API `http://localhost:5000`  
**Libro:** emisor Rafael Alberto Martinez Quesada, sucursal CostaPets, contabilidad encendida  
**Usuario de la prueba:** AdminCosta26  
**Caja abierta al cierre de la re-prueba:** #21 (fondo ₡10.000) — #20 cerrada en esta pasada

El diseño de activación por sucursal y de libro por emisor sigue en `docs/MEJORAS_CONTABILIDAD_API_Y_WEB.md`. Este archivo es la evidencia de lo que el sistema hizo con documentos reales y lo que hay que corregir.

## Re-prueba 2026-11-11 noche (post-#26)

Prefijo `UAT-*-1111N`. Navegador integrado a **1440×900** (sin eso, nivel C oculta el contenido y la bandeja parece vacía).

| Flujo | Documento | Evento | Resultado |
|---|---|---|---|
| Apertura caja | #21 (ya abierta) | Aperturas #21 `AperturaCaja` | Contabilizado |
| Venta crédito | FE Marco 002/Test1 | Ventas **#70** y **#71** `VentaEmitida` | Contabilizado |
| Venta contado | Preventa **#29** Factura Contado Marco 002/Test1 ₡2.425,32 → Cobrar efectivo | Ventas **#72** `VentaEmitida` | Contabilizado (apareció tras FE en proceso) |
| Anulación NC | (pasada tarde) | DevolucionesVenta #19 | Contabilizado (no re-emitida) |
| Compra | (pasada tarde) `UAT-CR-1111R` | Compras #17 | Contabilizado |
| Depósito confirmado | (pasada tarde) `UAT-DEP-1111R` | Instrumentos #11 | Contabilizado |
| Toma física | (pasada tarde) 00000008 | Inventario #8 | Contabilizado |
| Presupuesto | (pasada tarde) | — | Sin evento (esperado) |
| Póliza planilla | Manual D `6.1.1.1` / C `2.1.2.1` | AsientoManual Contabilidad #3078… | Contabilizado — **#26 resuelto** |
| Póliza fiscal | D `6.5.1` / C `1.1.2.1` | — | No re-ejecutada en esta noche |
| Comisiones | Liquidada 05/11 (4 mov.) | Comisiones #2 `ComisionLiquidada` | Sigue Contabilizado |
| Consignación | Kardex/ledger | — | Pantalla OK; sin movimiento nuevo esta noche |
| Pre-cierre 09/2026 | Período Abierto | Botón «Ejecutar pre-cierre» disponible | **Cierre efectivo sigue bloqueado** (docs sin evento + diffs auxiliares) |
| Abrir mes | 09/10/11 Abierto | — | «Abrir período» habilitado |

**Nota operativa:** el navegador integrado arranca bajo 1280px y muestra solo el aviso «Pantalla de escritorio»; hay que forzar viewport ≥1280 o la UAT parece fallar cuando en realidad el grid está oculto.

## Re-prueba 2026-11-11 tarde (post-mejoras)

Casos nuevos tras sync de catálogo y activación de `AperturaCaja` / `DevolucionFondoCaja`. Prefijo `UAT-*-1111R`.

| Flujo | Documento | Evento | Resultado |
|---|---|---|---|
| Sync catálogo | Plantillas nuevas + cuenta `4.2.1.2` | — | `AperturaCaja` y `DevolucionFondoCaja` creadas como borrador; varias plantillas con versión borrador nueva |
| Activar fondo de caja | v1 desde 2026-09-01 | — | Ambas plantillas **Activa** (simuladas con `{"Fondo":40000}`) |
| Cierre caja #20 | Arqueo ₡11.425,00 vs esperado ₡11.425,32 | Cajas #8 `DiferenciaCaja` | Contabilizado (faltante ₡0,32 a gasto) |
| Devolución fondo #20 | Cierre de apertura previa a la plantilla | `DevolucionFondoCaja` | **No** (esperado: cajas abiertas antes de activar no se regularizan) |
| Apertura caja #21 | Fondo ₡10.000, Caja 1 | Aperturas #21 `AperturaCaja` | Contabilizado |
| Venta crédito | FE #10, Marco, 002/Test1, ₡2.425,32 | Ventas #68 `VentaEmitida` | Contabilizado |
| Venta contado | Tiquete #21, efectivo ₡2.425,32 | Ventas #69 + Cobros #27 | Contabilizado |
| Anulación NC | NC #19 sobre tiquete #21 | DevolucionesVenta #19 `NotaCreditoEmitida` | Contabilizado |
| Compra crédito | `UAT-CR-1111R`, proveedor TEST, 002/Test1, ₡1.697,72 | Compras #17 `CompraRegistrada` | Contabilizado |
| Depósito caja confirmado | Pre-dep ₡1.000 → `UAT-DEP-1111R` → Confirmado | Instrumentos #11 `DepositoConfirmado` | Contabilizado (**#24 corregido**) |
| Depósito caja no confirmado | Pre-dep ₡500 → `UAT-DEP-1111B` (Pendiente / resolución No confirmado) | Ninguno | Correcto: sin `DepositoConfirmado` |
| Toma física faltante | Pretoma #5 → toma **00000008** parcial (−1 und. Test1, ₡1.502,41) | Inventario #8 `AjusteInventario` | Contabilizado |
| Presupuesto / proforma | Proforma nueva Marco + 002, total ₡2.425,32 | — | Guardada; **sin evento** (esperado) |
| Póliza manual planilla | D `6.1.1.1` / C `2.1.2.1` ₡50.000, concepto `UAT planilla 1111R` | — | **Falla API** FK `FK_AsientosContables_EventosContables_IdEventoContable` (ver #26) |
| Comisiones (re-verificación) | Corte previo #2 | Comisiones #2 `ComisionLiquidada` | Sigue Contabilizado |
| Consignación (re-verificación) | Casos previos Marco / conteo | Ventas/Cobros de consignación | Siguen Contabilizado; hallazgos #2/#5 de salida/traslado intactos |
| Pre-cierre 09/2026 | Período Abierto | Checklist | **No se puede cerrar:** 0 eventos bloqueantes; **70** docs sin evento; **15** diferencias de conciliación (CxC/CxP/Inventario vs mayor en 0 — saldos iniciales) |
| Pre-cierre / abrir mes | 09/2026, 10/2026, 11/2026 | — | Los tres **Abierto**; «Abrir período» listo. Cierre efectivo de septiembre **bloqueado** hasta regularizar docs/saldos |
| Compras #14 | Error viejo | `CompraRegistrada` | Sigue Error: falta campo `ImpuestoAcreditable` (hace falta Regenerar / plantilla nueva) |

**Caja abierta al cierre de esta re-prueba:** #21. **Pendiente real:** cierre efectivo de septiembre (limpiar 70 docs sin evento + 15 diffs), regenerar/reintentar Compras #14, arreglar póliza manual (#26), y opcionalmente un corte nuevo de comisiones / consignación facturada si se quiere re-ejercer esos flujos end-to-end.

## Re-prueba 2026-11-11 (mañana)

Casos simulados nuevos del día. La caja #20 ya estaba abierta; no se abrió otra.

| Flujo | Documento | Evento | Resultado |
|---|---|---|---|
| Crédito en Marco (antes sin crédito) | Se activó crédito + ruta UAT Comisiones + límite ₡500.000 | — | Listo para facturar a crédito |
| Venta crédito | Factura electrónica #9, Marco, art. 002 lote Test1, ₡2.425,32 | Ventas #66 `VentaEmitida` | Contabilizado |
| Venta contado (tiquete) | Tiquete #20, mismo cliente/artículo, efectivo ₡2.425,32 | Ventas #67 `VentaEmitida` + Cobros #26 `CobroAplicado` | Contabilizado |
| Anulación vía devolución | NC #18 sobre tiquete #20 (completa), RecibidoPorHacienda | DevolucionesVenta #18 `NotaCreditoEmitida` | Contabilizado (tras activar v2 vigente desde 2026-11-01 y Reintentar) |
| Pre-depósito + depósito caja | Pre-depósito ₡1.000 apertura #20 → depósito `UAT-DEP-1111A` → Confirmado | Esperado `DepositoConfirmado` | UI Confirmado; **no aparece evento** en bandeja (nov) |
| Depósito no confirmado | `UAT-DEP-NO-0511` (pasada anterior) | Ninguno | Sigue No confirmado (correcto para cobro de factura; este es de caja) |
| Pre-cierre mes 11/2026 | Período Abierto | Checklist | **No se puede cerrar:** 2 eventos bloqueantes, 15 diferencias de conciliación |
| Apertura de mes | 09/2026, 10/2026, 11/2026 | — | Los tres en estado Abierto; «Abrir período» disponible para crear otros |

No se re-ejecutaron en esta sesión (siguen vigentes las filas de «Cerrado en esta pasada»): compra #15/#16, abonos, toma 00000007, consignación, comisiones corte #2, escenarios de caja #15–#19. Presupuesto/proforma: sin evento contable esperado (solo preventa). Póliza manual (planilla/fiscal): el modal «Nueva póliza manual» abre y lista cuentas; el bind de montos vía automatización CDP no actualizó el estado Blazor — falta confirmar a mano D `6.1.1.1` / C `2.1.2.1` y D `6.5.1` / C `1.1.2.1`.

### Hallazgos nuevos 2026-11-11

**23. `NotaCreditoEmitida` sin plantilla vigente — corregido 2026-11-11.**  
La v1 quedó Activa solo desde 2026-12-01 (corte de inventario del catálogo). Se creó, simuló y activó la **v2** con vigencia desde **2026-11-01**; v1 pasó a Reemplazada. Tras Reintentar, DevolucionesVenta #18 quedó **Contabilizado**.

**24. Depósito de caja confirmado no publica `DepositoConfirmado` — corregido en re-prueba tarde.**  
`UAT-DEP-1111A` (mañana) no dejó evento. En la tarde, `UAT-DEP-1111R` confirmado publicó **Instrumentos #11 `DepositoConfirmado` Contabilizado**. El caso no confirmado (`UAT-DEP-1111B`) no genera evento (correcto).

**25. Pre-cierre 11/2026 bloqueado.**  
Tras corregir NC #18 queda al menos Compras #14 en error de configuración + documentos sin evento + 15 diferencias de conciliación (auxiliar CxC vs mayor desfasado). No cerrar el mes hasta limpiar eso. Septiembre sigue igual: 70 docs sin evento + 15 diffs.

**26. Póliza manual no inserta asiento (FK a evento) — causa hallada y corregida 2026-11-11.**  
Desde Bandeja → Pólizas → «Nueva póliza manual» (CostaPets, D `6.1.1.1` / C `2.1.2.1` ₡50.000) el API respondía: `The INSERT statement conflicted with the FOREIGN KEY constraint "FK_AsientosContables_EventosContables_IdEventoContable"` (tabla `dbo.EventosContables`, columna `Id`).  
Causa: `RegistrarEventoAsync` solo hace `Add` y `GeneralLedgerService.ContabilizarManualAsync` copiaba `evento.Id` al asiento antes de guardar el evento; el INSERT del asiento iba contra un evento inexistente. El mismo patrón estaba en `ReversarAsync`. No hacía falta un «evento sintético»: el método ya crea el suyo.  
Arreglo: `SaveChangesAsync` del evento dentro de la transacción, antes de armar el asiento (manual y reverso). Las pruebas de humo usan InMemory, que no valida llaves foráneas, y por eso no lo veían. **Pendiente: reiniciar el API y repetir la póliza manual y el asiento fiscal D `6.5.1` / C `1.1.2.1`.**

## Cerrado en pasadas anteriores

Estos flujos generaron el documento operativo y la póliza. No repetirlos salvo re-prueba puntual.

| Flujo | Documento | Evento | Póliza |
|---|---|---|---|
| Factura de crédito | Factura 8, ₡28.630,73 | Ventas #61 | Contabilizado |
| Compra a crédito | UAT-CR-0511, artículo 002, proveedor 174, ₡11.300,00 | Compras #15 | D `1.1.6.1.4` 10.000 + D `6.1.6.12` 1.300 / C `2.1.1.1` 11.300 |
| Compra de contado | UAT-CO-0511, mismos datos | Compras #16 | Igual que la de crédito |
| Abono a proveedor | UAT-AB-0511, ₡11.300,00 | Compras #1 | D `2.1.1.1` 11.300 / C `1.1.2.1` 11.300 |
| Toma con faltante | N.º 00000007, −1 und., costo ₡1.502,41 | Inventario #7 | D `6.1.6.8` / C `1.1.6.1.3` 1.502,41 |
| Depósito confirmado de factura | Recibo 21, ₡1.000,00 sobre factura de crédito N.º 5 | Cobros #21 `CobroAplicado` | D `1.1.2.1` / C `1.1.4.1` 1.000 |
| Depósito no confirmado | Recibo 22, ₡1.000,00 sobre factura N.º 7 | Ninguno | Correcto: no se reconoce el cobro |
| Comisión agente, SAC y dual | Corte #2, 4 movimientos, ₡559,78 | Comisiones #2 `ComisionLiquidada` | D `6.1.1.2` / C `2.1.2.5` 559,78 |
| Caja, faltante bajo el tope | Apertura #7, abierta desde el 4/9/2026. Esperado ₡148.966,80, contado ₡148.460,21 | Cajas #1 `DiferenciaCaja` | D `6.1.6.9` 506,59 / C `1.1.1.2` 506,59 |
| Caja, sobrante | Apertura #14, fondo ₡10.000, contado ₡10.200 | Cajas #2 `DiferenciaCaja` | D `1.1.1.2` 200 / C `4.2.1.1` 200 |
| Caja, cuadre (2026-11-06) | Apertura #15, fondo y conteo ₡10.000 | Ninguno. Cierre #3 | Correcto: diferencia 0 no publica póliza |
| Caja, faltante bajo el tope (2026-11-06) | Apertura #16, esperado ₡10.000, contado ₡9.500 | Cajas #4 `DiferenciaCaja` | D `6.1.6.9` 500 / C `1.1.1.2` 500 |
| Caja, sobrante (2026-11-06) | Apertura #17, contado ₡10.200 | Cajas #5 `DiferenciaCaja` | D `1.1.1.2` 200 / C `4.2.1.1` 200 |
| Caja, faltante sobre el tope y rebajo | Apertura #18, contado ₡4.000, faltante ₡6.000. Rebajo por planilla | Cajas #6 `DiferenciaCaja` y `FaltanteRebajadoPlanilla` | D `1.1.5.1` / C `1.1.1.2` 6.000, luego D `2.1.2.1` / C `1.1.5.1` 6.000 |
| Caja, faltante sobre el tope y castigo | Apertura #19, contado ₡3.000, faltante ₡7.000. Castigo | Cajas #7 `DiferenciaCaja` y `FaltanteCastigado` | D `1.1.5.1` / C `1.1.1.2` 7.000, luego D `6.1.6.9` / C `1.1.5.1` 7.000 |

La plantilla `AjusteInventario` del libro quedó en versión 2, Activa, vigencia 2026-11-01. La versión 1 regía desde 2026-12-01 y no cubría documentos de noviembre.

### Comisiones — cómo se probaron los tres papeles

No existe un tercer tipo de beneficiario «dual». Una persona con las dos marcas cobra una vez:

- En su propia ruta, solo como **Agente**.
- En una ruta que no tiene asignada, como **Servicio al cliente**, y el agente titular de esa ruta cobra la suya.

Para emitir con la caja ya abierta se marcaron temporalmente «Es agente» y «Servicio al cliente» en el perfil Super AdminCostaPets. Esas dos marcas ya se quitaron. El perfil volvió a: Super administración, Gestiona usuarios, Costa Pets, Acepta consignación, Permite existencia negativa.

| Tiquete | Ruta | Líneas |
|---|---|---|
| 00100150040000000018 | UAT Comisiones (creada en la prueba, id 4, 5 % agente) | Costa PetsAdmin agente 5 % = ₡107,32. Sin línea SAC |
| 00100150040000000019 | Guanacaste (3 % agente, 4 % SAC) | Alberto agente 3 % = ₡64,39 y Costa PetsAdmin servicio al cliente 4 % = ₡85,85 |

Base de ambos tiquetes: ₡2.146,30 (artículo 002, sin impuesto), cliente Marco, contado. El devengo contable es al liquidar el corte, no al emitir la venta.

El corte también arrastró un movimiento que ya estaba pendiente: tiquete 00100151010000000005, cliente Rafael, Alberto agente 3 % de ₡10.074,07 = ₡302,22. Por eso Alberto suma ₡366,61 en dos movimientos. El usuario SacFacturacion no facturó: no tiene caja abierta y su clave interna no se conoce. La línea de servicio al cliente sí se generó con el perfil dual.

### Consignación — el operativo cerró, la contabilidad no

Primera consignación de Marco (cédula 112940871), bodega del cliente, artículo 002 lote Test1.

1. Boleta de ingreso #4: 2 unidades, costo ₡3.004,82, referencia UAT-CON-0511.
2. La bodega central y la del cliente son de la misma sucursal. `PolizaTrasladoIntrasucursal.Registrar` sale sin evento cuando origen y destino son la misma sucursal.
3. Conteo #5: consignado 2, físico 1, vendido 1, clasificación «Unidad vendida en el punto», aprobado.
4. Prefactura #63, facturada de contado en efectivo ₡2.425,32 (₡2.146,30 + IVA ₡279,02), apertura de caja #7.
5. Liquidación #1 quedó Facturada. La bandeja del 2026-11-05 no recibió un evento nuevo por esa factura.

### Caja — apertura, arqueo, conciliación y cierre

La apertura no genera evento contable. El fondo queda en el operativo y no entra a `1.1.1.2`. El asiento nace solo al cerrar, y solo por la diferencia (contado − esperado). Cero diferencia no publica nada.

La #7 llevaba abierta desde el 4/9/2026, fondo ₡40.000, efectivo neto ₡31.356,59 y tarjetas ₡77.610,21. El depósito de ₡1.000 de la prueba anterior salió como línea informativa y no entró al esperado ni a la diferencia. El arqueo no muestra el efectivo del sistema, solo las tarjetas: el primer conteo dejó las tarjetas iguales al sistema y el efectivo en cero, y la conciliación marcó faltante de ₡71.356,59. Se corrigió el arqueo #4 a ₡148.460,21 y el cierre quedó en faltante de ₡506,59. Como está bajo el tope de ₡5.000, fue a gasto (`6.1.6.9`) y no al cajero.

La #14 se abrió con ₡10.000 en esta prueba (la pantalla avisó «Apertura #0»; el número real es 14) y se cerró con sobrante de ₡200. Ese asiento debita la caja y acredita `4.2.1.1`. El fondo de ₡10.000 no quedó en el libro.

Esa pasada dejó abierta la #15. La segunda pasada del 2026-11-06 la cerró y recorrió el resto de escenarios.

### Caja — segunda pasada, 2026-11-06

Cada caja nueva salió con fondo ₡10.000 (un billete de 10.000), caja 1, cajero Costa PetsAdmin. El aviso volvió a decir «Apertura #0»; el número real se leyó en el arqueo. Ninguna tenía movimientos, así que el esperado fue el fondo. Cheques y depósitos siguieron fuera de la fórmula.

| Apertura | Conteo | Cierre | Póliza, todas Contabilizado |
|---|---|---|---|
| #15 | ₡10.000 | #3, diferencia 0 | Sin evento |
| #16 | ₡9.500 | #4, faltante ₡500 | Cajas #4: D `6.1.6.9` Faltantes de caja / C `1.1.1.2` Caja cobro ruta |
| #17 | ₡10.200 | #5, sobrante ₡200 | Cajas #5: D `1.1.1.2` / C `4.2.1.1` Sobrantes de caja y valores |
| #18 | ₡4.000 | #6, faltante ₡6.000, luego rebajo por planilla | Cajas #6 `DiferenciaCaja`: D `1.1.5.1` Cuentas por cobrar a empleados / C `1.1.1.2`. `FaltanteRebajadoPlanilla`: D `2.1.2.1` Salarios por pagar / C `1.1.5.1` |
| #19 | ₡3.000 | #7, faltante ₡7.000, luego castigo | Cajas #7 `DiferenciaCaja`: D `1.1.5.1` / C `1.1.1.2`. `FaltanteCastigado`: D `6.1.6.9` / C `1.1.5.1` |

Quedó abierta la **#20**, caja 1, fondo ₡10.000, observaciones «Fondo despues del UAT de caja», sin arqueo, para poder seguir facturando.

## Correcciones ya en el código de trabajo

Están en el árbol del API y en el proceso que escucha en el puerto 5000. Todavía no hay commit.

| Cambio | Dónde | Por qué |
|---|---|---|
| `Aprobar` registra `AjusteInventario` | `TomaFisicaManager.Aprobar` | Solo `Guardar` llamaba a `RegistrarEventoAjusteInventario`. La pantalla aprueba, así que la toma movía existencia y no dejaba evento |
| `Aprobar` pulsa el worker | `TomaFisicaController.Aprobar` | El evento quedaba encolado hasta el barrido periódico |
| Nombre de cajero | `ServicioFacturarPreventaContado` | Si el nombre enviado no coincide con el de la apertura, reintenta con `c.Usuario` |
| Descripción de línea vacía | `ContabilidadManager` | `Descripcion` nula ya no revienta el asiento; se guarda en blanco |

`Anular` de la toma ya reversa con `AjusteInventarioAnulado`. Eso no se ejerció en vivo.

## Mejoras pendientes

El plan, con el criterio de cierre de cada punto, está en `docs/PLAN_MEJORAS_UAT_CONTABILIDAD.md` (la misma copia en los dos repos). Lo de abajo es el hallazgo.

### API — contabilidad

**1. Sobrante de toma física no tiene asiento.**  
Toma N.º 00000006 (conteo 40 contra sistema 39, ganancia ₡1.502,41) creó Inventario #6 Contabilizado con póliza de 0 líneas. `RegistrarEventoAjusteInventario` solo reparte líneas con `CostoDiferencia < 0`, y la plantilla `AjusteInventario` solo debita `CostoPerdidas` contra inventario.  
Mejora: mandar también el costo de las unidades ganadas y agregar en la plantilla el par inventario / sobrante (o reversión de merma). Una toma mixta debe partir faltante y sobrante en la misma póliza.

**2. La salida de inventario de la consignación facturada no se contabiliza.**  
Poner la mercadería en la bodega del cliente (boleta) no es una venta: sigue siendo inventario de la empresa. `ContabilizarConsignacion` solo arma póliza si la central y el cliente están en sucursales distintas; si son la misma, `PolizaTrasladoIntrasucursal` sale sin evento porque la cuenta y la sucursal no cambian.  
La salida que sí debe ir al libro es la unidad facturada. `FacturarPrefactura` descuenta la bodega y, en contado, solo crea el recibo de caja: no llama a `RegistrarEventoVentaEmitida` ni a `CobroAplicado`. Por eso la prefactura #63 no apareció en la bandeja. En crédito sí llama al evento, pero el payload trae solo total, subtotal, impuesto y descuento. Le faltan `Costo`, `SubTotalGravado`, `SubTotalExento` y el reparto `Inv*` que sí manda `VentasManager`. La fórmula rechaza un campo ausente, así que esa póliza tampoco acreditaría el inventario. La prefactura no pasa por `VentasManager.addEnvoiceEntry`: muta la fila de la preventa.  
Mejora: al facturar, publicar el mismo `VentaEmitida` que una venta normal (con costo y tipo de inventario) y, si es contado, el `CobroAplicado`.

**3. El banco del cobro no es el banco del asiento.**  
El depósito confirmado se pagó con BANCO NACIONAL y la póliza debitó `1.1.2.1` BAC colones, que es la cuenta fija de la plantilla `CobroAplicado` para `MontoBanco` (`UsosCuentaContable.BancoColones`). Lo mismo en los abonos a proveedor.  
Mejora: el payload debe llevar la cuenta bancaria elegida en el pago, y la plantilla debe usar esa cuenta cuando exista. Si no hay mapeo, el evento tiene que quedar en error de configuración y no en la cuenta de otro banco.

**4. El abono se puede confirmar dos veces.**  
UAT-AB-0511 quedó en dos recibos idénticos, Compras #1 y Compras #2, ambos Contabilizado. CxP quedó debitada ₡22.600 contra una sola factura de ₡11.300. UAT-CO-0511 sigue pendiente de pago.  
Mejora: el alta del abono debe ser idempotente por referencia y factura, o la pantalla debe impedir un segundo guardado del mismo formulario. El recibo duplicado hay que anularlo en el entorno; no reintentarlo.

**5. El traslado a la bodega de consignación de la misma sucursal no deja rastro contable.**  
Es el comportamiento actual de `PolizaTrasladoIntrasucursal` (sale si las sucursales son iguales o el costo es 0). El inventario se movió; el libro no.  
Mejora a decidir con el contador: o se documenta que el traslado entre bodegas de la misma sucursal es solo operativo, o se agrega dimensión de bodega para ver central contra consignado sin cambiar el saldo de la cuenta.

**6. El artículo 002 se fue a producto terminado.**  
`InventarioPorTipo` manda el tipo de artículo 3 a `1.1.6.1.3` Producto terminado. El faltante de la toma y el costo de estas ventas cayeron ahí, no en mercadería `1.1.6.1.4`.  
Mejora: revisar el tipo del artículo 002 (y del resto del catálogo Costa Pets) antes de cerrar un período. Si el tipo está bien, el asiento está bien.

**7. Compra del 2026-11-11 sigue en error.**  
Compras #14, `CompraRegistrada`, fecha posterior a hoy (2026-11-05). No reintentar ni volver a registrar esa compra. Cuando el reloj del sistema pase esa fecha, reintentar desde la bandeja.

**8. `EsDeducible` no se ve en el catálogo.**  
La marca existe en la semilla y en el API. El modal de editar cuenta no la muestra. Hace falta verla, y editarla con permiso, para que el reporte de deducibilidad se pueda contrastar en pantalla.

**9. La cuenta `1.1.6.1` admite movimiento y tiene subcuentas.**  
El sync del catálogo ya lo avisó. Solo las hojas (`1.1.6.1.1` a `1.1.6.1.4`) deben moverse.

### API — consignación y comisiones

**10. La liquidación muestra facturado 0.**  
El conteo #5 vendió 1 unidad y la prefactura se facturó, pero el estado muestra factura 0 y facturado 0,000. `Facturado` solo sube si la reserva de la preventa coincide en artículo y lote con la línea del conteo (`ConsignacionInventarioManager`, alrededor del descuento de bodega). La línea de la prefactura guardó artículo «4» y lote «0», así que no casó con 002 / Test1. La existencia de la bodega del cliente sí bajó.  
Mejora: guardar código de artículo y id real del lote en la prefactura, y acumular `Facturado` con esa misma llave. El saldo final del estado tiene que restar lo facturado.

**11. El corte de comisiones liquida todo el rango.**  
Crear corte toma todo lo pendiente, cobrado y sin corte de la sucursal entre las dos fechas. El corte #2 se llevó también la comisión de Rafael de las 17:28.  
Mejora: el diálogo «Si, continuar» debe decir cuántos movimientos, de qué beneficiarios y por qué importe entran, antes de confirmar. Un segundo corte con el mismo rango y estado Confirmado ya se rechaza; eso está bien.

**12. Ruido de clave duplicada en comisiones.**  
El log del API repite `IX_ComisionMovimientos_IdVentaDetalle_TipoBeneficiario` al tomar la foto de la venta. No bloqueó el guardado. Conviene dejar de registrarlo como error cuando la fila ya existe.

### Web — presentación

**13. La fecha del estado de consignación se imprime como texto de código.**  
`Views/Consignacion/Estados.razor`, la celda de fecha cierra el `@()` antes de `.ToString(...)`. En pantalla salió `5/11/2026 5:40:16 p. m..ToString("dd/MM/yyyy")`.  
Mejora: `@((x.FechaLiquidacion ?? x.FechaConteo).ToString("dd/MM/yyyy"))`.

**14. El descuento y el impuesto de compra muestran el formato.**  
En `Views/Compras/Compra.razor` el formato `:N2%` quedó fuera de la expresión Razor (línea de la rejilla y el aviso de impuesto del XML). Se ve `0:N2%`. El mismo patrón está en la fecha de vencimiento del lote de esa rejilla si el `ToString` queda fuera del `@()`.  
Mejora: un solo helper de porcentaje, el mismo que ya usa el resto de importes.

**15. Las rejillas muestran el id interno del artículo.**  
Comisiones y la prefactura de consignación pintan «4» o «1 — …» en vez del código 002 y el lote Test1.  
Mejora: el DTO que alimenta la grilla debe traer `CodArticulo` y el número de lote, y la columna debe usar esos campos.

**16. El depósito de una factura no es el depósito de caja.**  
Un pago con forma DEPOSITO sobre una factura de crédito contabiliza como `CobroAplicado` solo si queda confirmado. `DepositoConfirmado` es el traslado de efectivo de la caja al banco. El estado negativo del depósito de factura es «No confirmado»; «Rechazado» es el del cheque. Conviene decirlo en la pantalla de depósitos para que no se busque el evento equivocado en la bandeja.

**17. La conciliación no lista la caja abierta.**  
`ObtenerAperturaDeCajaSinCierre` filtra estado `M`. Una apertura en `A` no aparece hasta que se arquea, y la pantalla dice «solo aperturas sin cerrar». Con la #7 abierta la lista salió vacía.  
Mejora: listar `A` y `M`, y dejar el botón de cerrar deshabilitado mientras no haya arqueo, con el motivo en la misma fila.

**18. El arqueo no muestra el efectivo que el sistema espera.**  
Solo enseña tarjetas y pre-depósitos. El cajero no ve el efectivo neto ni el fondo mientras cuenta.  
Mejora: mostrar, como referencia, fondo + efectivo neto al lado del total declarado.

**19. El cierre no dice la diferencia que va a contabilizar.**  
El diálogo confirma el total de sistema (₡148.966,80 en la #7) y no el faltante de ₡506,59 que publica `DiferenciaCaja`.  
Mejora: el texto debe incluir contado, esperado y diferencia, y si cae a gasto o al cajero.

**20. La apertura responde número 0.**  
`CrearAperturaCaja` guarda el id real y devuelve el `request`, que nunca recibió `Napertura`. El aviso dijo «Apertura #0» para la #14, la #15 y, en la segunda pasada, para la #16, #17, #18, #19 y #20.  
Mejora: devolver el id asignado.

**21. El fondo de la caja no entra al libro.**  
Abrir no publica evento. Al cerrar solo se contabiliza la diferencia. Los ₡40.000 de la #7 y los ₡10.000 de la #14 no están en `1.1.1.2`.  
Mejora a decidir con el contador: asentar el fondo al abrir (caja contra el origen del fondo) o dejarlo fuera y documentarlo. Hoy el libro solo ve cobros, depósitos de caja y la diferencia del cierre.

**22. Faltante sobre ₡5.000, rebajo y castigo — ejecutados el 2026-11-06.**  
La #18 (faltante ₡6.000) cargó `1.1.5.1` y el rebajo por planilla la pasó a `2.1.2.1`. La #19 (faltante ₡7.000) hizo el mismo cargo al cajero y el castigo la pasó a `6.1.6.9`. Las dos cuentas por cobrar quedaron saldadas en el mismo día. El cuadre de la #15 confirmó que diferencia 0 no publica evento.

## Datos dejados en el entorno

No borrarlos desde aquí y no volver a registrarlos.

- Ruta **UAT Comisiones** (id 4), activa, agente por defecto Costa PetsAdmin.
- Corte de comisiones **#2** confirmado. No se puede repetir el mismo rango.
- Recibos de abono **Compras #1 y #2** (UAT-AB-0511). El segundo está de más.
- Compra **UAT-CO-0511** sigue con saldo. **UAT-CR-0511** quedó saldada de más por el abono duplicado.
- **Compras #14** en error por fecha 2026-11-11 (reintentar desde bandeja; el reloj local ya es 2026-11-11).
- Toma **00000006** (sobrante, póliza vacía) y **00000007** (faltante, contabilizada). Lote Test1: no usar **12312321**.
- Depósitos: `UAT-DEP-1111A` Confirmado (2026-11-11); `UAT-DEP-NO-0511` No confirmado; `UAT-DEP-OK-0511` Confirmado. Otros (5675676, …) no se tocaron.
- **DevolucionesVenta #18** / NC #18 y **#19** / NC #19: Contabilizado.
- Ventas 2026-11-11 tarde: FE crédito **#10** (Ventas #68), tiquete **#21** + NC **#19** (Ventas #69 + Cobros #27).
- Cajas: **#20** cerrada (DiferenciaCaja #8). Caja **#21** abierta (AperturaCaja Contabilizado), fondo ₡10.000.
- Marco (112940871) quedó con **crédito activo**, ruta UAT Comisiones, límite ₡500.000.
- El cliente Rafael no puede factura nueva de crédito mientras tenga la factura vencida.
