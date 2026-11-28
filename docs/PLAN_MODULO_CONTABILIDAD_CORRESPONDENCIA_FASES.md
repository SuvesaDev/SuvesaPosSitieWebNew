# Correspondencia de fases — Contabilidad General (API ↔ Web)

Este documento vive por duplicado en ambos repos (`DevSuvesaPosWeb/docs/` y
`SuvesaPosSitieWebNew/docs/`) porque enlaza dos planes que viven cada uno en su
propio repo: `PLAN_MODULO_CONTABILIDAD_GENERAL_API.md` y
`PLAN_MODULO_CONTABILIDAD_GENERAL_WEB.md`. Si se edita uno, editar el otro.

## Por qué hace falta

Cada plan declara su propia lista de fases (API: A0–A6: Web: W1–W6) y el plan
Web ya anota una "Dependencia API" por fase, pero en texto libre, sin señalar
la fase exacta del API. Sin esa correspondencia explícita, coordinar cuándo
puede arrancar cada fase Web queda a interpretación de quien lo lea.

## Tabla de correspondencia

| Fase Web | Depende de (API) | Qué debe estar REALMENTE listo, no solo iniciado |
|---|---|---|
| W1 Cimientos | A1 Cimientos | Migraciones de `accounting_company_settings`/`accounting_issuer_settings`, flag `Contabilidad:Habilitada` en dos niveles, permisos `CONTABILIDAD.*` dados de alta, endpoint de estado de activación por empresa/emisor/sucursal. |
| W2 Configuración | A1 Cimientos (catálogo, dimensiones) + A0 Decisiones (validaciones de importación de catálogo) | CRUD de `chart_of_accounts`/`account_dimensions`/`dimension_values` funcionando, con el asistente de importación del catálogo (borrador → validación → publicación) ya expuesto — no solo el modelo de datos. |
| W3 Operación | A2 Motor + A3 Integración | `AccountingEventProcessor` consumiendo eventos reales (no simulados) desde al menos Ventas y Cobros; bandeja de errores con estado real, no mockeada. |
| W4 Consulta | A3 Integración (completa, todos los módulos) + A4 Conciliación (parcial: auxiliares expuestos) | Diario/mayor con datos reales de TODOS los módulos operativos integrados (ver lista de eventos abajo), y `SubledgerReconciliationService` exponiendo diferencias consultables. |
| W5 Cierre y estados | A4 Conciliación (completa) + A5 Estados y cierre | `CloseService` y `AccountingReportingService` con balanza/estados que cuadran contra un cierre real ejecutado en el API, no contra datos de prueba locales. |
| W6 UAT | A6 Día Cero/UAT | Ambiente con saldos de apertura cargados y aprobados (ver "Día Cero" del plan API), para que el UAT firmado por contador se haga sobre datos representativos. |

## Eventos que A3 debe cubrir antes de que W3/W4 puedan considerarse "listas"

La lista original de eventos del plan API (`VentaEmitida`, `VentaAnulada`,
`NotaCreditoEmitida`, `CobroAplicado`, `CompraRegistrada`, `CompraAnulada`,
`PagoProveedorAplicado`, `MovimientoInventario`, `AjusteInventario`,
`ComisionDevengada`, `ComisionRevertida`, `ComisionLiquidada`,
`DepositoConfirmado`, `DepositoNoConfirmado`, `ChequeConfirmado`,
`ChequeRechazado`) **no cubre el módulo de Importaciones** (facturas de
proveedor extranjero con proceso de aduana, construido en esta misma rama de
trabajo). `ImportacionesManager.CerrarAsync` inserta las `Compra` (cuentas por
pagar al proveedor extranjero y a cada proveedor local con XML fiscal) de
forma directa, sin pasar por `ComprasManager.addEnvoiceEntry` — a propósito,
para no duplicar el movimiento de inventario que la propia importación ya
aplica. Si `CompraRegistrada` se dispara solo dentro de `addEnvoiceEntry`,
esas cuentas por pagar nunca emiten evento contable y quedan fuera del motor
en silencio.

**Acción para A3**: agregar `ImportacionCerrada` a la lista de eventos, con su
propio outbox enganchado directamente en `ImportacionesManager.CerrarAsync`
(mismo patrón de transacción-documento-más-outbox que los demás eventos, no
una excepción). W3/W4 no deben darse por completas si este evento no está
cubierto — de lo contrario el diario/mayor tendría un hueco silencioso
exactamente en el módulo más reciente del sistema.

## Ruta crítica y duración estimada

El API es el cuello de botella: cada fase Web solo puede empezar en serio
cuando su dependencia API está *entregada*, no simplemente iniciada (columna
derecha de la tabla). La duración total del proyecto no es la suma de los dos
totales (18–24 semanas API + ~10–16 semanas Web) — es aproximadamente:

```
A0 → A1 → A2 → A3 (+ retiro de MayorCuentaPorCobrar, ver más abajo) → A4 → A5 → A6
                                                                              └→ W6 (1–2 sem finales)
```

Con la ampliación de A3 (ver "Decisión resuelta 1" abajo), la estimación total
del API sube de 18–24 a **~21–28 semanas**. Los tramos W1–W5 se solapan con
las fases A posteriores a su dependencia (ej. W2 puede avanzar mientras corre
A2, siempre que A1 ya haya entregado catálogo/dimensiones), pero W6 no puede
cerrar antes de que A6 entregue el ambiente con saldos de apertura. Estimado
de punta a punta: **~23–30 semanas**, no 28–40 (que sería la suma ingenua).

## Decisiones resueltas (esta sesión)

Estas cuatro decisiones estaban abiertas en el análisis de huecos y ya se
resolvieron con el dueño del proyecto. Cada plan (`_API.md`/`_WEB.md`) se
actualizó por separado con el detalle; aquí solo el resumen para no tener que
leer los dos documentos completos para saber qué se decidió:

| # | Decisión | Resolución | Impacto en el plan |
|---|---|---|---|
| 1 | Futuro de `MayorCuentaPorCobrar` (submayor de CxC real y activo, usado por `ServicioDevolucionInterna`/`ServicioNotaCreditoCxC`/`ServicioAnulacionCobro`) | **Se retira.** CxC pasa a vivir únicamente en `accounting_entries`/`accounting_entry_lines` del motor nuevo. | Amplía el alcance de **A3 Integración** en el plan API: ya no es solo "agregar eventos en paralelo", incluye migrar los tres servicios de CxC para que dejen de escribir en `MayorCuentaPorCobrar` y escriban (o generen evento hacia) el motor nuevo. Se estima +2–3 semanas sobre la duración original de A3. |
| 2 | Lenguaje de "instalación/tenant" en el plan API | **Se mantiene**, es intencional: SUVESA planea alojar eventualmente varios clientes distintos en la misma instalación. | Ninguno en el alcance actual (ya se diseñaba con `company_id` en toda entidad contable), pero queda confirmado como dirección de producto deliberada, no boilerplate sin adaptar — para que futuras decisiones de aislamiento (¿fila-por-tenant o base-por-tenant?) se tomen sabiendo que es un requisito real. |
| 3 | Mecanismo de reautenticación para reapertura de período / reversión de póliza | **Mecanismo nuevo y separado**, no se reutiliza `ClaveInterna` (el PIN de Agente/SAC). Contabilidad tiene su propio rol de "contador autorizando un cierre", distinto del cajero validado en caja. | Nuevo requisito en **A1 Cimientos** del plan API: diseñar el flujo de confirmación (probablemente contraseña de Identity, no un PIN numérico corto como `ClaveInterna`, dado que es una operación de mayor impacto). Pendiente de A0: decidir si es contraseña de Identity re-ingresada, un segundo factor, o algo distinto. |
| 4 | Campos legado huérfanos (`Compra.Contabilizado`/`.Asiento`/`.ContaInve`/`.AsientoInve`, `Proveedore.CuentaContable`, `Configuracione.Contabilidad`) | **Se dejan morir**, no se migran ni se reusan. | Se documentan como obsoletos en el plan API (sección Día Cero) y quedan listados para una limpieza de código aparte — no bloquean ni informan el diseño del motor nuevo. |
