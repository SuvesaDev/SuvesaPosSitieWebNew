# Evidencia UAT certificación sitio — 2026-11-12

Plan: `docs/PLAN_UAT_CERTIFICACION_SITIO.md`  
Entorno: Blazor `http://localhost:5199`, API `http://localhost:5000`  
Web commit: `51b43e4`  
API commit: `c43b0b3e`  
Probador: agente (navegador integrado)  
Usuario: AdminCosta26 / CostaPets  
Prefijo: `UAT-CERT-1112`  
Viewport: 1440×900 + `seepos-nivel-c`  
Clave interna: `UatCaja2026` · Cliente Marco `112940871` · Artículo `002`

## Veredicto de certificación (5 condiciones del plan)

| # | Condición | ¿Cumple? | Evidencia |
|---|---|---|---|
| 1 | Cada función del menú con corrida Pasa (datos reales) | **No** | Capa C: **148 Pasa / 7 Pendiente**. Las 7 hubs/mockup pendientes impiden cobertura total. La plantilla de 8 revisiones por pantalla no se agotó (smoke de apertura/lista). |
| 2 | Los 18 flujos B de punta a punta pasaron | **No** | Solo **F8, F10, F15, F17** en Pasa; el resto Parcial o sin ciclo completo (doc + operativo + póliza). |
| 3 | Contabilidad cuadra + pre-cierre limpio + período cerrado | **No** | C11 **bloqueado**: **72** docs sin evento + **31** diffs. Ningún período cerrado. |
| 4 | Matriz de roles (pantalla + API) | **No** | Solo AdminCosta26. Sin Cajero/Vendedor/Contador ni 403 API. |
| 5 | Cero defectos sev. 1–2 + automatizado verde | **No** | Sev.2 abiertos (**27**, **29**). Web **254/254**; API Smoke **1063 Passed / 680 Failed** (timeouts del host de prueba). |

**Certificación: NO CERTIFICADO** (faltan las cinco a la vez).

## 0. Capa A — automatizado

| Suite | Resultado | Notas |
|---|---|---|
| Web unitarias | **254/254 Passed** | `SuvesaPosSitioAplicacion.Tests.dll` |
| Web E2E Playwright | No corrido | |
| API SmokeTests | **1063 Passed / 680 Failed** | Fallos masivos por timeout al construir `IHost` (`WebApplicationFactory`); no es verde |

## 1. Fase 0 / entorno

| Caso | Resultado | Notas |
|---|---|---|
| Blazor responde | Pasa | `:5199` |
| Login + sucursal | Pasa | AdminCosta26 / CostaPets (sesión previa) |
| Caja abierta | Pasa | **#21** Costa PetsAdmin |

## 2. Capa C — smoke por función

Detalle: `docs/_uat_smoke_1112.json`

| Estado | Cantidad |
|---|---:|
| Pasa | 148 |
| Pendiente (pantalla pendiente / hub) | 7 |
| Falla | 0 |
| Sesión perdida | 0 |

Pendiente: `/buys`, `/buys/orders/warehouseorders`, `/sales`, `/parameters`, `/parameters/locations`, `/parameters/areas`, `/collections`.

## 3. Fases 1–3 — muestra con datos reales

| Caso | Resultado | Notas |
|---|---|---|
| Usuarios `/parameters/users` | Pasa | Lista (AdminCosta26, Alberto, Maria Jose, …) |
| Roles `/parameters/role` | Pasa | Unlock `UatCaja2026`; 3 roles (Administrador, Admin_CostaPets, SA COSTA PETS) |
| Sucursales `/parameters/branch` | Pasa | Formulario/listado |
| Emisores `/parameters/issuers` | Pasa | Emisor 112780286 |
| Cajas `/parameters/cash-registers` | Pasa | Caja 1 → Apertura #21 |
| Bancos / Familias / Monedas / Presentaciones / Impuestos | Pasa | Listas con datos |
| Clientes filtro Marco | Pasa | `112940871 MARCO…` crédito ₡500.000 (1 de 1) |
| Inventarios filtro 002 | Pasa | Artículo 002 visible |

## 4. Capa B — flujos F1–F18

JSON: `docs/_uat_flujos_1112.json`

| Id | Flujo | Resultado | Documento / evento | Notas |
|---|---|---|---|---|
| F1 | Compra | Parcial | Compras #14/#17 Contabilizado | UI unlock; sin compra nueva; aviso “sin caja” en contado |
| F2 | Pago proveedor | Parcial | #11 hist. | No corrido completo |
| F3 | Toma física | Parcial | Inventario #8 | No corrido completo |
| F4 | Traslado bodegas | Parcial | — | Smoke capa C |
| F5 | Pedidos / OC | Parcial | — | Pedidos pendiente |
| F6 | Proforma | Parcial | Lista 6 (Marco #6) | Sin conversión |
| F7 | Factura Contado | Parcial | FE **31** Aceptado → Ventas **#75** | Caja #21; intento nuevo incompleto; sin CobroAplicado |
| F8 | Factura Crédito | Pasa | FE 11/12 → Ventas #70/#71 | Verificado; Marco con crédito |
| F9 | Devoluciones | Parcial | NC #18/#19 hist. | Smoke pantalla |
| F10 | Cobrar | Pasa | Unlock + #21 | `UatCaja2026` |
| F11 | Ciclo caja | Parcial | Apertura #21 | Sin cierre |
| F12 | Cobranza | Parcial | 1 factura ₡10.383,70 | Seguimiento OK |
| F13 | Comisiones | Parcial | Pendiente 0 / Liquidada OK | Ruta UAT Comisiones en filtro |
| F14 | Bodega | Parcial | — | Smoke capa C |
| F15 | Consignación | Pasa | #73 + Cobros #28 | Settlements UI |
| F16 | Producción | Parcial | — | Smoke capa C |
| F17 | Fiscal / docs | Pasa | Facturas Aceptado | Contado 29–31, FE 11–12 |
| F18 | Reportes | Parcial | Compras: 4 registros | Consultar OK |

## 5. Contabilidad C1–C12

| Id | Caso | Resultado | Notas |
|---|---|---|---|
| C1 | Sucursal contab. apagada | No corrido | |
| C2 | Sin plantilla vigente | Pasa (hist.) | |
| C3 | Regenerar | Pasa (hist.) | Compra #14 |
| C4 | Anterior a puesta en marcha | No corrido | |
| C5 | Bandeja | Pasa | Contabilizado 11/11: Ventas #70–#75, Cobros #26–#28, Compras #17, NC #19 |
| C6 | Período cerrado | No corrido | |
| C7 | Abrir mes | Pasa | 09/10/11 Abierto |
| C8 | Moneda extranjera | No corrido | |
| C9 | Reverso / aprobaciones | Parcial | Smoke |
| C10 | Auxiliares | Parcial | CxC **3** diffs; Inv **0**; pre-cierre reporta CxP/Inv vs mayor |
| C11 | Pre-cierre 09/2026 | **Bloqueado** | **72** docs sin evento; **31** diffs; CxC/CxP/Inv vs mayor=0 |
| C12 | Cierre anual | No corrido | |

Balanza y Estado de resultados: Consultar OK (smoke con cifras).

## 6. Seguridad (Fase 1)

| Caso | Resultado | Notas |
|---|---|---|
| Login AdminCosta26 | Pasa | |
| Roles UI + clave | Pasa | Matriz visible tras unlock |
| Matriz multi-rol / 403 API | No corrido | |

## 7. Hallazgos (desde **30**; 27–29 previos)

| # | Sev. | Descripción |
|---|---|---|
| 27 | 2 | *(abierto)* Contado #74/#75: `VentaEmitida` Contabilizado sin `CobroAplicado` nuevo (sigue #28 máx en bandeja). |
| 28 | 3 | *(abierto)* Facturación: selector de ruta comercial no usable en el flujo Contado probado → no genera comisión Pendiente E2E. |
| 29 | 2 | *(abierto)* Cierre 09/2026 bloqueado por docs sin evento + diffs (hoy: **72** / **31**). |
| 30 | 3 | Compra desbloqueada con la misma clave de caja #21, pero UI indica que el cajero no tiene caja abierta para compra de contado. |
| 31 | 3 | Pre-cierre: el checklist de bloqueo aparece al seleccionar período, pero un clic en «Ejecutar pre-cierre» a veces no deja el resultado visible de forma estable. |

## 8. Artefactos

- `docs/_uat_smoke_1112.json` — capa C
- `docs/_uat_flujos_1112.json` — F1–F18 + C1–C12
- `docs/_uat_routes.json` — rutas menú (155)
- `docs/_uat_smoke_summary.md` — resumen smoke

## 9. Veredicto final

**NO CERTIFICADO** — no se cumplen las cinco condiciones del plan (contabilidad no cierra, matriz de roles no ejercida, defectos sev.2 abiertos, cobertura de flujos B incompleta).
