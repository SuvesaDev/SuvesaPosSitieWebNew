# Saneamiento de series, cobros y caja — Fase 0 (Sitio Web)

> Parte de sitio del plan derivado de
> `ARQUITECTURA_SERIES_FACTURACION_RECIBOS_COBROS_CAJA.md`. El API es la parte
> pesada (numeración, mayor de CxC, caja, comandos transaccionales); el sitio
> **deja de coordinar varias llamadas** y pasa a **un comando por operación
> crítica**.
>
> Documento hermano (API): `../DevSuvesaPosWeb/docs/SANEAMIENTO_SERIES_COBROS_CAJA_API.md`.

---

## 1. Hallazgos verificados en el sitio

| # | Archivo | Hallazgo |
|---|---|---|
| W1 | `Views/Ventas/Cobrar.razor` | La verificación de caja abierta es **solo de interfaz**; efectivo/tarjeta por `"EFE"`/`"TAR"`; crédito deducido del texto `TipoFactura`; registra el cobro y **luego** convierte la preventa (si falla la 2.ª llamada, el cobro queda sin factura — el propio mensaje lo admite); no crea ni imprime el recibo. |
| W2 | `Views/Ventas/CuentasPorCobrar.razor(.cs)` | "Abono Cobrar" hoy cobra **preventas** (ver `ABONO_COBRAR_PREVENTAS_WEB.md`). **No** busca facturas de crédito con saldo; **no** crea el recibo clásico `Abonoccobrar`; cobro y facturación se llaman **por documento** → un lote puede quedar parcial; depende de `"EFE"`. |
| W3 | `Views/Ventas/Facturacion.razor` | Elige tipo de factura pero **no** un perfil/serie explícito; filtra crédito por `Abierto`/`Sinrestriccion`; **no** muestra ni valida límite/saldo/disponible/vencimiento; **no** asigna el `IdPlazo` del cliente; **no** exige apertura; **no** crea preventa ni manda contado a Cobrar; ejecuta creación de factura sin orquestar recibo/caja/pago. |
| W4 | `Views/Caja/{Apertura,Arqueo,Cierre}.razor` | Cubren los pasos pero heredan del API cálculos y estados débiles (total mezcla ventas y dinero). |
| W5 | Proxies | `Cobros.cs` encadena `Cobrar` → `FacturarPreventa` → (emitir) desde el navegador. La arquitectura pide que esa secuencia viva en el API. |

---

## 2. Plan por fases (sitio) — sigue al API

> **Estado (2026-09-04)**
> - **Fase 8.1 — parcial.** Hecha la pantalla **Series operativas**
>   (`/parameters/operational-series`, `PARAMETROS.SERIES_OPERATIVAS`): listado
>   con filtro por tipo/emisor, alta/edición (emisor + centro obligatorios, D1),
>   activar/desactivar, marca de predeterminada. Proxy `ISeriesOperativas` →
>   `api/series-operativas`. Falta el resto del desglose de configuración
>   (perfiles de emisión, serie única de NC, formas de pago semánticas, panel de
>   inconsistencias).
> - **Fase 8.4 — parcial.** Hecha la pantalla **Conciliación de caja**
>   (`/initial/cash/reconciliation`, `CAJA.CONCILIACION`): elige una apertura sin
>   cerrar y muestra fondo inicial + saldo esperado por forma de pago y moneda
>   **desde el mayor** (`GET api/caja/{napertura}/conciliacion`), con las ventas
>   del período como dato meramente informativo. Falta: bloqueo durante arqueo
>   final, diferencias declaradas y cierre idempotente desde esta vista.
> - Fases 8.2, 8.3, 8.5 pendientes (8.2 tiene ya la pestaña "Facturas de
>   crédito", commit `7361cc8`).

### Fase 8.1 — Configuración (tras Fases 1–7 del API)
Separar en pantallas distintas (hoy mezcladas): **Tipos fiscales** ·
**Ámbitos de numeración fiscal** · **Perfiles de emisión (contado/crédito)** ·
**Serie única de NC** · **Series operativas** · **Formas de pago (con
propiedades semánticas)** · **Plazos de crédito**. Detectar: perfiles sin
contador, >1 predeterminado, series sin emisor/sucursal, contadores duplicados,
formas sin código Hacienda, series operativas faltantes, plazo de cliente sin
correspondencia.

### Fase 8.2 — Cobrar unificado (tras Fases 2–4 del API)
Una sola pantalla `Cobrar` con pestañas: **Pendientes de contado** ·
**Facturas de crédito** · **Recibos emitidos** · **Operaciones fallidas**
(autorizados). Reglas:
- apertura **desde el API** (no de interfaz);
- solo formas activas permitidas; referencias obligatorias; vuelto solo efectivo;
- una o varias facturas; mostrar la distribución antes de confirmar; impedir
  sobreaplicación;
- **un solo comando** (`ConfirmarCobroYFacturarPreventaContado` /
  `CobrarFacturasCredito`), idempotente;
- tras éxito: **Imprimir recibo**; para contado además factura/tiquete;
- Hacienda y correo como estados asíncronos.
- `/sales/collect` se mantiene como **entrada filtrada** o redirige a `Cobrar`
  con la pestaña adecuada; **no** implementa un segundo motor de cobro.
- La `ABONO_COBRAR_PREVENTAS` actual se conserva hasta migrar y probar; luego se
  retira.

### Fase 8.3 — Facturación alrededor de preventa + perfil (tras Fase 4 del API)
- exigir apertura; mostrar emisor/caja/apertura;
- seleccionar un **PerfilEmision elegible** (devuelto por el API);
- para crédito: límite, saldo, disponible, plazo, vencimiento; bloquear
  combinaciones inelegibles;
- guardar siempre como **preventa**; contado → Cobrar; crédito → comando
  específico; estados Hacienda/correo sin bloquear.

### Fase 8.4 — Caja
Presentar: apertura activa · bloqueo durante arqueo final · totales por forma y
moneda **desde el mayor de caja** · diferencias declaradas · desglose comercial
separado · cierre idempotente · alerta de pendientes/inconsistentes.

### Fase 8.5 — Recibos
Consulta e impresión de recibos emitidos (usa `SuvesaPos.Impresion` vía
`/documentos/recibo-cobro/{id}/pdf` y `recibo-pago`).

---

## 3. Proxies a rehacer
- `ICobros` deja de exponer `Cobrar` + `FacturarPreventa` sueltos; expone los
  **comandos** del API (`ConfirmarCobroYFacturarPreventaContado`,
  `CobrarFacturasCredito`, `AnularCobro`) que devuelven identificadores, números
  y estados.
- `IAbonoCobrarPreventas` (creado para la entrega anterior) se pliega dentro de
  ese `ICobros` unificado.
- Formas de pago: consumir `naturaleza`, `permiteVuelto`, `requiereReferencia`,
  `afectaCaja` — **nunca** comparar `"EFE"`/`"TAR"`.

---

## 4. Pruebas E2E (de §13 de la arquitectura)
- Facturación contado abre Cobrar y termina con recibo imprimible.
- Facturación crédito muestra límite y vencimiento.
- Cobrar encuentra factura de crédito por cédula y número.
- Cierre refleja los cobros reales.
- Estados Hacienda y correo se actualizan sin repetir la operación.

---

## 5. Qué se puede hacer **ya** sin decisiones
- Formas de pago: que el API devuelva `naturaleza` y el sitio deje de comparar
  `"EFE"` — cambio contenido (necesita el campo en el API, Fase 1.5 / 8.1).
- Nada más del sitio antes de las Fases 1–4 del API: cualquier rediseño de
  Cobrar/Facturación sobre los endpoints actuales repetiría los problemas.
