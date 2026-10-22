# Plan: Contabilidad tras el merge de Bodega y Despacho

Documento de trabajo para `DevSuvesaPosWeb` y `SuvesaPosSitieWebNew`, rama `feature/modulo-contabilidad` después de incorporar `develop`. La copia vive en `docs/` de los dos repos y tiene que mantenerse igual.

Este texto **no implementa nada**. Fija el impacto del merge sobre el motor contable, los huecos y el orden de trabajo. El motor, las plantillas y las pantallas W1–W6 siguen como están. No se autoactivan plantillas. No se congela el catálogo. No entra consolidación.

## Tesis

Bodega y Despacho **no reemplazan** la póliza de la venta. El picking confirma una salida que la factura o la preventa ya hizo: no vuelve a descontar inventario. Por eso el camino feliz (facturar → alistar → empacar → entregar) no duplica `VentaEmitida` ni mueve el auxiliar de inventario.

El riesgo está en las **excepciones de bodega**, que sí cambian stock o el documento fiscal, y hoy no todas llegan al outbox:

| Excepción de bodega | Qué hace el operativo | Qué ve Contabilidad hoy |
|---|---|---|
| Faltante / no entregado de **tiquete interno** | Devolución interna + reingreso | `NotaCreditoEmitida` vía `ServicioDevolucionInterna` |
| Faltante / no entregado de **factura electrónica** | `DevolucionVentaManager.addEnvoiceEntry` (NC a Hacienda) | El alta **no** registra evento. `NotificadorAceptacionFiscal` solo escucha `Venta` y `Compra`. Al anular la NC sí corre `EspejoAsientoContable.ReversarPorOrigen`, sobre un asiento que nunca nació |
| Merma o pérdida en tránsito | Tras la NC, `InventarioMovimientosManager` con `Movimiento.Ajuste` negativo | Stock baja. **No** hay `AjusteInventario` |
| Faltante / no entregado de **preventa** | Se quita cantidad de la preventa y se suelta reserva | Correcto: la preventa nunca emitió `VentaEmitida` |
| Pretoma | Conteo. El ajuste vive al aprobar la toma | `AjusteInventario` al aplicar la toma, igual que antes |
| Boleta de consignación | Movimiento entre bodega central y bodega del cliente | `ConsignacionBodega` vía `PolizaTrasladoIntrasucursal` (ya estaba enganchado) |
| Traslado entre sucursales del mismo emisor | Kardex origen/destino | `TrasladoIntrasucursal` si sucursales distintas y costo > 0 |
| Traslado o bin dentro de la misma sucursal | Kardex / ubicación | Sin póliza (a propósito: misma cuenta, misma dimensión) |

El merge **no rompe** venta, cobro, compra, pago, comisión ni importación. Amplifica huecos que el plan original ya nombraba (`NotaCreditoEmitida` electrónica, `AjusteInventario` / `MovimientoInventario`) porque ahora Bodega los dispara todos los días.

## Qué entra del merge y cómo encaja

### Bodega y Despacho (núcleo)

- `VentasManager` llama `Alistamiento.AsegurarSilencioso` al grabar la venta.
- Entran a cola: factura y preventa con líneas físicas. No entran: anulado, consignación, tiquete de mostrador (salvo forzar).
- El flag `Venta.RequiereDespacho` se guarda (cliente o solicitud) y **la cola no lo lee**. Toda factura no-tiquete genera orden.
- Estados: Pendiente → En alistamiento → Empacado → En ruta → Entregado / No entregado / Anulado.
- `AlistamientoManager` no tiene `IEventosContablesManager`. Contabiliza por los documentos que ya existían (NC, preventa, kardex).

### Pretoma

La pretoma no ajusta existencias. Al aprobar, `TomaFisicaManager` aplica diferencias y registra `AjusteInventario`. Ese evento **no tiene plantilla en el catálogo inicial Tico Foodster**; si el contador no arma una versión Activa, el evento queda en bandeja.

### Consignación (boletas)

`RegistrarBoleta` mueve kardex y llama `ContabilizarConsignacion`. El asiento se publica en el acto, sin plantilla versionada, igual que el traslado. La pantalla nueva de boletas no abre un segundo canal.

### Precio en colones (`ConviertePrecioColones`)

Cambia el precio de venta en pantalla. El payload de `VentaEmitida` sigue siendo `Total` / `SubTotal` / `ImpVenta` / `Descuento` del documento. Si la factura se guarda ya convertida a CRC, el mayor cuadra. Hay que **verificar** un caso USD→CRC de punta a punta; no se asume rotura.

## Punto de vista

1. **Reconocimiento de ingreso en la factura (o en Hacienda), no en la entrega.** Eso ya estaba cerrado. Entregar un bulto no debe generar póliza. El mayor de CxC no espera la firma del chofer.
2. **Bodega es dueña de la excepción física; Contabilidad es dueña de la póliza.** Faltante y no entregado deben pasar por el documento fiscal (NC o ajuste de preventa). Merma debe pasar por `AjusteInventario`, no por un kardex mudo.
3. **Un solo outbox por hecho.** La NC electrónica de bodega y la NC de mostrador son el mismo tipo de evento. El notificador de Hacienda tiene que cubrir devoluciones, o el alta de `DevolucionVenta` tiene que registrar el evento con la misma clave de idempotencia que usará la aceptación.
4. **Plantilla Borrador hasta que el contador active.** Cualquier plantilla nueva de merma / ajuste se carga en Borrador. El evento puede existir antes; sin versión Activa queda en bandeja, visible, no inventado.
5. **Sucursal de la póliza = sucursal del documento.** La orden copia `Venta.IdSucursal`. La merma usa la bodega de la línea. Si un día la bodega de picking pertenece a otra sucursal, el `AjusteInventario` de merma debe ir con la sucursal de esa bodega, no con la de la factura.

## Huecos

### P0 — la NC electrónica de bodega no poliniza

`Faltante` y `No entregado` de una factura electrónica llaman `DevolucionVentaManager.addEnvoiceEntry`. Ese manager no registra `NotaCreditoEmitida`. El notificador fiscal no trata origen `Devolucion` / `DevolucionesVenta`.

Efecto: se restaura inventario y se manda la NC a Hacienda; CxC operativo baja; el mayor y el auxiliar de CxC **no**. La anulación de esa NC intenta reversar un asiento inexistente.

Esto aplica a **toda** NC electrónica, no solo a Bodega. Bodega lo vuelve frecuente.

**Cierre:** registrar `NotaCreditoEmitida` al aceptar Hacienda (y, si es interna, al grabar, como ya hace `ServicioDevolucionInterna`). Misma clave de idempotencia. Payload alineado con la plantilla actual (`Total`, `SubTotal`, `ImpVenta`, `Descuento`). Prueba: faltante de FE aceptada crea un evento; reintento no duplica; sucursal apagada no poliniza.

### P0 — merma en tránsito sin evento

Tras la NC, `RegistrarMermaTrasNc` descuenta de nuevo con `Movimiento.Ajuste`. El kardex queda bien (la NC reingresó; la merma saca lo perdido). El mayor de inventario no se entera.

Cuando exista plantilla de costo de ventas / inventario sobre `VentaEmitida`, este hueco deja el auxiliar de inventario descuadrado de forma permanente.

**Cierre:** en la misma transacción de la merma, `AjusteInventario` (o un tipo `MermaTransito` si el contador quiere cuenta de gasto distinta de la toma física). Payload con costo, sucursal de la bodega, origen `Alistamiento:{idOrden}`. Plantilla inicial en **Borrador**: débito gasto merma / crédito `1.1.6.1` (cuentas configurables, no cerradas). No autoactivar.

### P1 — se alista y se despacha antes de que Hacienda acepte (o rechace)

La orden nace al grabar la venta, no al polinizar. Una FE puede ir en ruta y Hacienda rechazarla después. Hoy no hay vínculo “evento `VentaEmitida` / estado Hacienda” ↔ cola.

**Cierre (decisión de negocio, luego código):**

- A) La cola espera `Aceptado` / `AceptadoParcial` en FE; el tiquete interno sigue al emitir.
- B) La cola sigue al emitir y, si Hacienda rechaza con orden abierta, se bloquea Empacar/En ruta y se exige no entregado o anulación.

Recomendación: **A para Empacar/En ruta**; crear la orden en Pendiente al facturar para que bodega vea la carga, sin salir a ruta sin aceptación.

### P1 — preventa entregada sin factura

La preventa entra a cola (`DebeEntrarACola` ignora el flag). Empacar y Entregar no convierten la preventa en venta. No hay `VentaEmitida`. Mercancía en casa del cliente, ingreso no reconocido, reserva o existencia según el movimiento original.

**Cierre:** al confirmar entrega de preventa, o bien exigir facturar (flujo guiado a Facturación), o bien dejar rastro duro en bandeja operativa (“entregada sin factura”) y no marcar Entregado. Contabilidad no inventa un asiento de preventa.

### P1 — `AjusteInventario` de toma/pretoma sin plantilla inicial

`TomaFisicaManager.RegistrarEventoAjusteInventario` ya dispara. `CatalogoInicialTicoFoodster.Plantillas` no incluye ese tipo. Pretoma aumenta el volumen de tomas aplicadas.

**Cierre:** plantilla Borrador de ajuste (pérdida: débito gasto / crédito inventario; ganancia inversa). El contador activa. Hasta entonces, la bandeja es el comportamiento correcto.

### P2 — anular una orden empacada no corrige el documento

`Anular` solo cambia estado. No está permitido en En ruta / Entregado. Si ya se empacó y se anula, la factura sigue viva y el stock sigue salido (la factura ya descontó).

**Cierre:** anular en Empacado pide el mismo camino que no entregado (NC o suelta de preventa), o se prohíbe anular pasado En alistamiento y se obliga “no entregado”.

### P2 — `RequiereDespacho` no filtra la cola

Toda FE/factura de crédito o contado no-tiquete genera orden. Un cajero puede reportar faltante sobre una venta de mostrador que nunca debió alistarse y emitir NC de más.

**Cierre:** `Asegurar` respeta `RequiereDespacho` (y el del cliente). Forzar sigue existiendo para el caso excepcional. El sitio muestra el flag en Facturación de forma explícita.

### P2 — costo de ventas sigue fuera de `VentaEmitida`

La plantilla inicial de venta mueve CxC, descuento, ingreso e IVA. No acredita inventario ni debita costo. Compra sí debita `1.1.6.1`. El auxiliar de inventario nace descuadrado contra existencias desde el primer día, **antes** del merge. Bodega (merma, faltante) lo hace visible.

**Cierre:** plantilla Borrador con líneas de costo (configurable). Activación del contador. No mezclar este corte con P0: primero que la NC y la merma existan como eventos.

### P3

- Ubicación bin / ruta / chofer: dato operativo. No son dimensión contable en este corte.
- `InventarioMovimientosManager.RegistrarMovimiento` genérico (ajustes sueltos, no toma ni merma): el plan original listaba `MovimientoInventario`; sigue sin outbox. Tratarlo después de merma, con el mismo evento `AjusteInventario` o uno propio.
- Precio en colones: una prueba de integración venta USD convertida → payload → póliza en CRC.
- Traslado misma sucursal distinta bodega: sin póliza. Correcto mientras no haya dimensión de bodega.
- Manuales: el de Bodega y el de Contabilidad cuentan historias distintas (entrega vs póliza). Hay que alinear el capítulo de excepciones cuando se implemente P0/P1.

## Plan de trabajo

Orden fijo. Un corte se cierra con prueba en API y, si hay pantalla, con el texto de Bodega alineado. Sin eso no se abre el siguiente.

### Corte 1 — API: NC electrónica (P0)

1. Decidir el disparo: alta de `DevolucionesVenta` si es interna; aceptación Hacienda si es electrónica (espejo de `VentaEmitida`).
2. Extender `NotificadorAceptacionFiscal` (o el callback real de devoluciones) con origen de NC.
3. Registrar `NotaCreditoEmitida` con clave `{libro}:DevolucionesVenta:{id}:NotaCreditoEmitida:1`.
4. Comprobar que `EspejoAsientoContable` al anular encuentra ese asiento.
5. Pruebas: NC de mostrador; NC disparada por `Alistamiento.Faltante` de una FE; idempotencia; sucursal apagada.

Sitio: ningún cambio de pantalla. El faltante ya llama al API.

### Corte 2 — API: merma (P0) + plantilla Borrador de ajuste (P1 toma)

1. En `RegistrarMermaTrasNc`, mismo patrón que `TomaFisicaManager.RegistrarEventoAjusteInventario` (sucursal de la bodega, costo de las líneas, origen alistamiento).
2. Plantilla inicial Borrador `AjusteInventario` (sirve toma y merma si el payload comparte `CostoPerdidas` / `Costo`). Si merma debe ir a otra cuenta de gasto, línea con rol configurable, no código cerrado.
3. Pruebas: no entregado + merma crea NC **y** `AjusteInventario`; no entregado + reingreso solo NC; preventa + merma no inventa `VentaEmitida`.

Sitio: el modal de entrega ya distingue reingreso/merma. Ajustar el texto para decir que merma genera ajuste contable cuando el módulo está efectivo. No ocultar Bodega detrás de Contabilidad.

### Corte 3 — política de cola vs Hacienda y preventa (P1)

Decisión explícita con el usuario (A/B de cola; facturar vs bloquear preventa entregada). Luego:

- API: estado de la orden ligado a Hacienda y a `EsPreventa`.
- Sitio: cola y empaque muestran “espera aceptación” / “preventa sin facturar”. Entrega de preventa no pasa a Entregado sin esa regla.

### Corte 4 — disciplina de cola (P2)

`DebeEntrarACola` lee `RequiereDespacho`. Anular empacado reutiliza el camino de no entregado. Facturación muestra el check de despacho.

### Corte 5 — costo de ventas (P2, con el contador)

Plantilla Borrador de `VentaEmitida` con inventario y costo. Activar cuando el auxiliar de inventario sea requisito de cierre. Independiente de Bodega; Bodega solo deja de ensuciar el auxiliar si el Corte 2 ya está.

## Qué no se hace en estos cortes

- Consolidación ni intercompany.
- Autoactivar plantillas.
- Contabilizar el picking, el bulto, el manifiesto ni la firma.
- Dimensión contable de bin o de chofer.
- Regenerar NSwag completo.
- Mover el reconocimiento de ingreso a la entrega.

## Pruebas que cierran el plan

| Caso | Esperado |
|---|---|
| Factura interna, alistada y entregada | Un `VentaEmitida`. Cero eventos de alistamiento |
| FE, Hacienda acepta, entregada | Un `VentaEmitida` en la aceptación. Cero eventos de alistamiento |
| Faltante de tiquete interno | `NotaCreditoEmitida` + reingreso de stock |
| Faltante de FE aceptada | NC fiscal + `NotaCreditoEmitida` (Corte 1) |
| No entregado con reingreso | Igual que faltante; sin `AjusteInventario` |
| No entregado con merma | NC + `AjusteInventario` de merma (Corte 2) |
| Faltante de preventa | Sin `VentaEmitida`; stock/reserva sueltos |
| Pretoma sin aprobar | Sin evento |
| Pretoma aprobada en toma | `AjusteInventario` de la toma |
| Boleta de consignación | `ConsignacionBodega` como hoy |
| Traslado entre sucursales del mismo emisor | `TrasladoIntrasucursal`; saldo de `1.1.6.1` igual |
| Sucursal apagada | Bodega opera; no hay eventos nuevos contabilizados |

## Relación con documentos ya existentes

- `PLAN_MODULO_CONTABILIDAD_GENERAL_API.md` / `_WEB.md` / `PLAN_MODULO_CONTABILIDAD_CORRESPONDENCIA_FASES.md`: A3 ya listaba `NotaCreditoEmitida`, `AjusteInventario` y `MovimientoInventario`. Este plan dice **dónde** el merge los toca.
- `MEJORAS_CONTABILIDAD_API_Y_WEB.md`: activación por sucursal, Hacienda y cobro con instrumento siguen vigentes. Este documento no los rehace.
- Manuales de Bodega y de Contabilidad: actualizar excepciones cuando cierren los Cortes 1–3.

## Orden resumido

1. NC electrónica al mayor (API).
2. Merma al mayor + plantilla Borrador de ajuste (API, texto menor en sitio).
3. Cola vs Hacienda y preventa entregada (decisión + API + sitio).
4. `RequiereDespacho` y anulación empacada.
5. Costo de ventas, con el contador.

## Aplicado (2026-10-22)

Cortes 1 a 5 aplicados en API y sitio. Las plantillas nuevas (`AjusteInventario`, líneas de costo en `VentaEmitida` / `NotaCreditoEmitida`) quedan en **Borrador**. No se autoactivan. El picking no genera póliza. El ingreso sigue en la factura (o en Hacienda).

| Corte | Qué quedó |
|---|---|
| 1 | `NotificadorAceptacionFiscal` cubre `DevolucionVenta`. Clave `{libro}:DevolucionesVenta:{id}:NotaCreditoEmitida:1`. Alta de NC electrónica espera Hacienda. |
| 2 | Merma en tránsito publica `AjusteInventario` (sucursal de la bodega, origen alistamiento). Plantilla Borrador débito `6.1.6.8` / crédito `1.1.6.1` con `CostoPerdidas`. |
| 3 | Empacar y salir a ruta esperan `Aceptado` / `AceptadoParcial` en FE. Preventa se alista; Entregado exige factura. Cola y empaque muestran el aviso. |
| 4 | `DebeEntrarACola` lee `RequiereDespacho`. Anular empacado reutiliza no entregado (reingreso). Facturación muestra el check. |
| 5 | Plantilla Borrador de venta y NC con `Costo`. Cuenta `6.1.6.8` Merma y faltante. Texto de merma en el modal de entrega. |
