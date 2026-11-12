# Evidencia UAT certificación sitio — 2026-11-11

Plan: `docs/PLAN_UAT_CERTIFICACION_SITIO.md`  
Entorno: Blazor `http://localhost:5199`, API `http://localhost:5000`  
Web commit: `2d6d6b2` (`feature/modulo-contabilidad`)  
API commit: `fc3f53cc` (`feature/modulo-contabilidad`)  
Probador: agente (navegador integrado)  
Usuario: AdminCosta26 / CostaPets  
Prefijo: `UAT-CERT-1111`  
Viewport: 1440×900 + `seepos-nivel-c`

## Veredicto de certificación (sección 1 del plan)

| Condición | ¿Cumple? | Evidencia |
|---|---|---|
| 1. Cobertura ~150 funciones | **Parcial** | Capa C smoke: **148 Pasa / 7 Pendiente / 0 Falla** (`_uat_smoke_summary.md`). Pendientes son hubs/mockups (`/buys`, `/sales`, `/parameters`, `/collections`, Pedidos, Ubicaciones, Areas). |
| 2. Contabilidad cuadrada + período cerrado | **No** | C11 bloqueado (70 docs sin evento, 15 diffs). Auxiliares CxC/CxP/Mayor con diferencias. |
| 3. Seguridad matriz de roles | **No** | Solo AdminCosta26; no se probaron Cajero/Vendedor/Contador/etc. ni 403 API. |
| 4. Defectos sev. 1–2 en cero | **Parcial** | Hallazgos 27–29 abiertos (abajo); no hay sev.1 fiscal nuevo, pero CobroAplicado ausente en #74/#75 es sev.2 potencial. |
| 5. Automatizado verde | **Parcial** | Web unitarias **240/240 Passed**. API SmokeTests no corridas en esta sesión (NuGet/proxy). |

**El sitio NO se certifica en esta corrida.** Queda acta de evidencia y lista de pendientes.

## 0. Capa A — automatizado

| Suite | Resultado | Notas |
|---|---|---|
| Web unitarias (`SuvesaPosSitioAplicacion.Tests.dll`) | **Pasa 240/240** | `dotnet test` sobre DLL ya compilada |
| Web E2E Playwright | No corrido | Requiere credenciales/arranque propio |
| API SmokeTests | No corrido | Bloqueo NuGet/proxy en el entorno del agente |

## 1. Fase 0 / entorno

| Caso | Resultado | Notas |
|---|---|---|
| Blazor responde | Pasa | `:5199` |
| API responde | Pasa | `:5000` |
| Login + sucursal | Pasa | AdminCosta26 / CostaPets |
| Caja abierta | Pasa | #21 |

## 2. Capa C — smoke por función

Detalle: `docs/_uat_smoke_results.json` · resumen: `docs/_uat_smoke_summary.md`

| Estado | Cantidad |
|---|---:|
| Pasa | 148 |
| Pendiente (pantalla pendiente / hub) | 7 |
| Falla | 0 |
| Sesión perdida | 0 |

Pendiente: `/buys`, `/buys/orders/warehouseorders`, `/sales`, `/parameters`, `/parameters/locations`, `/parameters/areas`, `/collections`.

## 3. Capa B — flujos F1–F18

| Id | Flujo | Resultado | Documento / evento | Notas |
|---|---|---|---|---|
| F1 | Compra | Pasa | Compras #14/#17 Contabilizado | #14 regenerada (ImpuestoAcreditable) |
| F2 | Pago proveedor | Parcial | Depósito #11 Contabilizado | Abono CxP no re-ejercido hoy |
| F3 | Toma física | Pasa | Inventario #8 Contabilizado | |
| F4 | Traslado bodegas | Parcial | Smoke abre | Sin movimiento nuevo |
| F5 | Pedidos / OC | Parcial | OC/importaciones abren; Pedidos pendiente | Mockup pedido |
| F6 | Proforma | Parcial | Lista OK | Sin conversión nueva |
| F7 | Factura Contado | Parcial | FE consec. **31** → Ventas **#75** Contabilizado | EFE; FE RecibidoPorHacienda; **sin CobroAplicado**; sin ruta UI |
| F8 | Factura Crédito | Pasa | Ventas #70/#71 Contabilizado | Verificado |
| F9 | Devoluciones / NC | Parcial | NC #18/#19 Contabilizado (noche) | Smoke pantalla |
| F10 | Cobrar | Pasa | Preventa 31 cobrada | Clave `UatCaja2026` |
| F11 | Ciclo caja | Parcial | Apertura #21 Contabilizado | Sin arqueo/cierre completo |
| F12 | Cobranza | Parcial | Seguimiento: 1 factura · saldo ₡10.383,70 | Dashboard/gerencial smoke en capa C |
| F13 | Comisiones | Parcial | Corte #2 Contabilizado; 0 Pendiente | Sin liquidación nueva (falta ruta en venta) |
| F14 | Bodega | Parcial | Smoke capas C | Sin ciclo alistamiento |
| F15 | Consignación | Pasa | Ventas #73 + Cobros #28 Contabilizado | |
| F16 | Producción | Parcial | Smoke capas C | Sin conversión |
| F17 | Fiscal / bandeja docs | Parcial | Facturas con estados Hacienda | #31 RecibidoPorHacienda |
| F18 | Reportes | Parcial | `/moduloReportes/compras` consulta | Smoke 31 rutas en capa C; no cuadre documento a documento |

JSON: `docs/_uat_flujos_results.json`.

## 4. Contabilidad C1–C12 (plan §7.1)

| Id | Caso | Resultado | Notas |
|---|---|---|---|
| C1 | Sucursal contab. apagada | No corrido | Falta 2.ª sucursal apagada en datos |
| C2 | Sin plantilla vigente | Pasa (histórico) | ErrorConfiguracion + Regenerar (#14) |
| C3 | Regenerar tras cambio | Pasa | Compra #14 |
| C4 | Anterior a puesta en marcha | No corrido | |
| C5 | Documentos sin contabilizar / bandeja | Pasa | Bandeja hoy Ventas #66–#75; Cobros #26–#28 |
| C6 | Período cerrado | No corrido | Ningún período cerrado aún |
| C7 | Abrir mes nuevo | Pasa | UI Abrir período; 09/10/11 Abierto |
| C8 | Moneda extranjera | No corrido | |
| C9 | Reverso póliza / aprobaciones | Parcial | Pantallas abren |
| C10 | Conciliación auxiliares | Parcial | CxC 3 / CxP 5 / Inv 0 / Mayor 3 diffs |
| C11 | Pre-cierre | **Bloqueado** | 09/2026: 0 eventos bloqueantes; **70** docs sin evento; **15** diffs |
| C12 | Cierre anual | No corrido | |

Pólizas manuales planilla/fiscal: Contabilizado (noche).

## 5. Seguridad (Fase 1)

| Caso | Resultado | Notas |
|---|---|---|
| Login AdminCosta26 | Pasa | |
| Matriz multi-rol | No corrido | Falta usuarios Cajero/Vendedor/… |
| Token no en navegador | No verificado | |

## 6. Hallazgos (desde **27**)

| # | Sev. | Descripción |
|---|---|---|
| 27 | 2 | Contado #74/#75: `VentaEmitida` Contabilizado pero **sin** `CobroAplicado` nuevo (SQL transient / cobro incompleto). |
| 28 | 3 | Facturación: selector de **ruta comercial** no visible/usable → no se puede generar comisión Pendiente E2E. |
| 29 | 2 | Cierre 09/2026 bloqueado: 70 docs sin evento + 15 diffs de conciliación (saldos iniciales / mayor en 0). |

## 7. Artefactos

- `docs/_uat_smoke_results.json` / `_uat_smoke_summary.md`
- `docs/_uat_flujos_results.json`
- `docs/UAT_RETEST_2026-11-11_noche.md` (agent store + noche previa)
- `docs/UAT_CONTABILIDAD_EN_VIVO.md` (matriz contable)
