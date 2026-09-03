# Mejora del manejo de Lotes — Sitio web (`SuvesaPosSitioAplicacion`, Blazor)

> Documento de trabajo. Repo: `SuvesaPosSitieWebNew`, rama base la de bonificaciones-web.
> El documento gemelo del API está en `DevSuvesaPosWeb/docs/MEJORA_LOTES_API.md` — **leerlo primero**: el sitio depende de los endpoints y campos que ahí se definen.
> **Regla del repo:** el API es la autoridad. El sitio pinta, valida en cliente para UX, y manda intención. Nada de lógica de inventario en el navegador.

---

## 0. Cómo verificar

```bash
cd SuvesaPosSitieWebNew
dotnet build src/SuvesaPosSitioAplicacion/SuvesaPosSitioAplicacion.csproj -c Debug -v q -nologo
dotnet test tests/SuvesaPosSitioAplicacion.Tests/SuvesaPosSitioAplicacion.Tests.csproj -v q --nologo   # 72 hoy
```
Menú: nodos en `Class/MenuSeePos.cs` (`ItemMenu { Titulo, Codigo, Ruta }`). Cada `Codigo` debe existir en la semilla de seguridad del API (`Fixtures/seed-seguridad.json` + `SecuritySystem/Seed/seed-seguridad.json`), lo enforcea `MenuCodigosTests`. `FiltroMenuTests.ElMenuRealSeCargoCompleto` cuenta los nodos: al agregar uno hay que subir el número.

### Contratos (NSwag)
El regen completo **no es viable** (rompe ~50 proxies: el API dejó de exponer schemas tras el desmontaje de `Datos`). Convención del repo: cuando el contrato generado no alcanza, se escribe a mano un cliente tipado + `partial` de DTO (ej. `ApiConexion/BonificacionApiCliente.cs`, `DTOs/Bonificacion/*`). Para este trabajo hará falta:
- `ApiConexion/InventarioMovimientosApiCliente.cs` (ficha de movimientos, existencia consolidada, actualizar existencia).
- `ApiConexion/TomaFisicaApiCliente.cs`.
- `partial`s en `DTOs/` para los campos nuevos de `StockLoteDTO` (vencimiento nullable, `EsUnico`, `Bloqueado`), `FacturaDetallesDTO` (`Lotes`), `FacturaCompraDetalleDTO` (`Lotes`), devoluciones (`IdStockLote`).

---

## 1. Estado actual (mapa)

### 1.1 Inventario → `Views/Inventario/Consulta.razor` (≈1500 líneas, un solo archivo)

Pestañas del editor (`_pestanaInventario`): `generales`, `precios`, `barras`, `imagen`, `formula`, `lotes`, `bonificacion`, `otros`.

- **Existencias**: hoy es una **sección dentro de "Generales"** (`línea 404` "Existencias": mínima / media / máxima / actual). `Existencia actual` = input `_existenciaEditable` con `@onchange="AlCambiarExistencia"` (`línea 1146`) → llama `Api.ActualizarExistencia(codArticulo, cantidad)` → proxy `InventarioConsulta.ActualizarExistencia` → `_stocks.ActualizarExistenciaArticuloAsync` → `Stocks/ActualizarExistenciaArticulo`.
- **Lotes**: pestaña `lotes` (`línea 610`), sólo visible si `_edicion.Lote`. Alta: número + vencimiento + existencia inicial (`_nuevoLote`, `_vencimientoLote`, `_cantidadNuevoLote`) → `Api.CrearLote`. Lista `_lotesEdicion`. Borrar → `EliminarLote` (`DesactivateLote`). **No hay editar lote.** No hay "lote único". El check `_edicion.Lote` + `AlCambiarLotes` (`línea 1496`) sólo muestra/oculta la pestaña.
- **Panel lateral de consulta** (`_elegido`, offcanvas): muestra `Existencia` (`línea 183`) y una tabla de `Lotes` read-only (`línea 217`, `_lotes` vía `Api.Lotes`).
- **No hay** pestaña "Movimientos del inventario".
- **Tipo de artículo**: `_tipoArticulo` se deriva en `Editar()`: `_edicion.EsPadre == true ? 2 : 3` ("materia prima" vs "producto terminado"). El valor `1` ("Normal") aparece pero no se asigna a artículos existentes. La pestaña `formula` sólo aparece si `_tipoArticulo == 3`.
- **`Servicio`**: `_edicion.Servicio` es un check en "otros"/"generales"; no afecta lotes.

### 1.2 Facturación → `Views/Ventas/Facturacion.razor`

- `_modalLotes` (`línea 181`): al agregar un artículo con `completo.Lote`, se abren los lotes (`Inventario.Lotes(codigo)`), lista filtrada `l.Activo != false && l.Cantidad > 0`, muestra `vence dd/MM/yyyy`. `ElegirLote` (`línea 656`) elige **un** lote → una `LineaVenta` con `IdLote`/`Lote`.
- **Un lote por línea.** Para vender dos lotes del mismo artículo hay que agregarlo dos veces (y ni así hay UI para elegir el segundo).
- **Sin filtro explícito de vencidos** en cliente: confía en que el API no devuelva vencidos.
- **Sin validación de existencia** (ni negativa ni por lote) en cliente.
- `_colaLotesCatalogo`: si se agregan varios artículos con lote desde el catálogo visual, se encolan y se piden lote uno por uno.
- Al emitir: `FacturaDetallesDTO { ..., Lote = l.IdLote, NumeroLote = l.Lote }` (`línea 883`).

### 1.3 Compras → `Views/Compras/Compra.razor`

- Alta manual: si `_articuloActual.Lote` → inputs `_lote` + `_venceLote` (**uno** por línea).
- Importar XML (`_modalImportarXml`): por línea se asocia el artículo interno; si `linea.Articulo?.Lote == true` (o CostaPets) aparece botón "Lote" (`AbrirLotesXml`) → `_modalLotesXml` con **un** `Lote` + `Vencimiento`. Aviso si falta lote/vencimiento. El botón "Aplicar" exige que **todas las líneas estén asociadas y vinculadas** (`VinculoConfirmado`).
- **Un lote por artículo importado.** No hay tabla de varios lotes por línea.
- No diferencia "lote único" (pide número/fecha igual).

### 1.4 Devoluciones → `Views/Ventas/DevolucionesVenta.razor`, `Views/Compras/DevolucionesCompra.razor`

- **Cero manejo de lote.** No se selecciona lote a devolver. (`grep [Ll]ote` vacío en ambos.)

### 1.5 Toma física / Ajuste de inventario

- El menú (`Class/MenuSeePos.cs`) tiene nodos `Toma` (`/buys/pretake`), `Toma`/`Pretoma`/`Pretoma Fisica General` (`/buys/take`, `/buys/pretake`, `/buys/taxclaim`), `Movimientos de articulos` (`/buys/movementitems`), `Ajuste Inventario` (`/buys/inventoryadjustment`).
- **Ninguno tiene página `.razor`.** `Views/Compras/` sólo tiene `AbonoPagar`, `Compra`, `ConsultarPedidos`, `CuentasPorPagar`, `DevolucionesCompra`, `OrdenCompra`. Son links muertos. La toma física en el sitio **se construye desde cero**.

### 1.6 Proxies de stock/lote

- `ApiConexion/ProxyInterface/IInventarioConsulta.cs`: `Lotes(idArticulo)`, `CrearLote`, `EliminarLote`, `ActualizarExistencia(codArticulo, cantidad, codBodega=0)`. **No hay** `EditarLote`, ni movimientos, ni existencia consolidada.
- `ProxyClass/InventarioConsulta.cs` usa `IStockLoteApiCliente` + `IStocksApiCliente` (generados).
- `StockLoteDTO` generado: `Id`, `Lote`, `Vencimiento` (**`DateTimeOffset` NO nullable**), `IdArticulo`, `Activo` (bool?), `Cantidad` (double).

---

## 2. Requerimiento → pantalla → gap

| Área | Requerimiento | Pantalla | Gap web |
|---|---|---|---|
| Inventario A1 | Producto terminado / materia prima manejan lote **obligatorio**; materia prima puede ser **lote único sin vencimiento**, fijado con check o creando el lote manual; **una vez fijado no se edita**. | `Inventario/Consulta.razor` | No hay tipo de artículo real ni check "lote único" ni bloqueo. El check `Lote` es libre. |
| Inventario A2 | Sacar "Existencias" del cuerpo → **pestaña** con tabla de lotes (o existencia si no lleva). Lote único: actualizar existencia, **no** editar lote. Lote normal: editar lote. | `Inventario/Consulta.razor` | "Existencias" está dentro de Generales; no hay tabla de lotes editable; no hay editar lote. |
| Inventario A3 | Toda modificación de existencia **registra movimiento**. | idem | El sitio ya llama a `ActualizarExistencia`; falta feedback del movimiento y que el API lo registre bien (ver doc API). |
| Inventario A4 | Pestaña **"Movimientos del inventario"**: ventas, compras, consignaciones, preventas, actualización de existencias, producciones… con **quién**, prov/cliente, existencia anterior/nueva, resultado, cantidad real. | `Inventario/Consulta.razor` | No existe. Falta endpoint + proxy + pestaña + tabla paginada. |
| Facturación V1 | Mostrar lotes disponibles **no vencidos** con existencia; permitir **varios lotes** por línea. | `Ventas/Facturacion.razor` | Un lote por línea; filtro de vencidos implícito. |
| Facturación V2/V4 | Venta/preventa actualiza artículo + lotes + ficha. | `Ventas/Facturacion.razor`, (Preventa no existe como pantalla) | El sitio manda el detalle; el API hace el resto. Multi-lote requiere cambiar el DTO del detalle. |
| Facturación V3 | No facturar con existencia negativa salvo **perfil** que lo permita. | `Ventas/Facturacion.razor` | Sin chequeo cliente; sin conocer el flag del perfil. |
| Compras C1/C2 | XML y manual: si el artículo **lleva lote**, crear **uno o varios** lotes con su cantidad; lote único / sin lote → normal. **Asociar el artículo antes** de validar. | `Compras/Compra.razor` | Un lote por línea; no diferencia lote único; el gate de "asociado" ya existe para XML pero no para alta manual. |
| Compras C3 | Al guardar: actualizar lotes + artículos + ficha. | idem | El API lo hace; el sitio sólo manda el detalle multi-lote. |
| Devoluciones D1/D2 | Elegir **lote** a devolver (o artículo si no lleva / lote único); se ve en movimientos. | `Ventas/DevolucionesVenta.razor`, `Compras/DevolucionesCompra.razor` | Cero manejo de lote. |
| Toma física T1-T4 | Pantalla en **Compras**, responsive (PC/tablet/portátil): todos los artículos con existencia (lote/único/normal), excluir `Servicio`; actualizar existencias; al guardar → **reporte** con pérdidas y **costeo**; se ve en movimientos; **resetea el acumulado**. | (nueva) | No existe. |

---

## 3. Diseño propuesto (web)

### 3.1 `Inventario/Consulta.razor` — reorganización de pestañas

Nuevas pestañas y cambios (mantener el patrón de un-archivo-con-`@if (_pestanaInventario == "...")`):

**a) Pestaña "Configuración de lote"** (o dentro de "Generales", sección propia)
- Selector **Tipo de artículo**: Normal / Producto terminado / Materia prima (bind a `_edicion.TipoArticulo`).
- Si Tipo ∈ {Producto terminado, Materia prima}: check `Lote` **forzado y deshabilitado** (siempre lleva lote).
- Si Tipo == Materia prima: check **"Lote único (sin vencimiento)"** → `_edicion.LoteUnico`.
  - Al marcarlo y guardar (o al crear el primer lote): el API fija `LoteUnicoFijado=true`. A partir de ahí el check y el número de lote quedan **read-only** (mostrar candado + texto "configuración fijada").
- Si Tipo == Normal: check `Lote` libre (como hoy).

**b) Pestaña "Existencias"** (nueva; saca la sección de "Generales")
- Llama `GET Inventario/ExistenciaConsolidada?idArticulo=` al abrir.
- Si **no lleva lote**: una fila "Existencia" con input editable → `PUT Inventario/ActualizarExistencia` (body con `observaciones` opcional). Al confirmar, toast + refrescar.
- Si **lleva lote (normal)**: tabla de lotes (`número`, `vencimiento`, `existencia`, acciones). Editar lote (número/fecha) → nuevo `PUT StockLote/Editar`. Editar existencia del lote → `PUT Inventario/ActualizarExistencia` con `idStockLote`. Agregar lote (como hoy).
- Si **lote único**: una fila con el lote único; **sólo** el input de existencia es editable (`PUT ActualizarExistencia` con `idStockLote`); número/fecha read-only con candado.
- Toda edición de existencia abre un pequeño diálogo "motivo" (opcional) que va como `observaciones`.

**c) Pestaña "Movimientos del inventario"** (nueva)
- `GET Inventario/Movimientos?idArticulo=&desde=&hasta=&tipoMov=` (paginado, `AppRejilla` con `GridDataProviderRequest`).
- Columnas: Fecha · Tipo (Venta/Compra/Consignación/Preventa/Producción/Actualización/Toma/Devolución) · Documento · **Usuario** · **Proveedor/Cliente** · **Existencia anterior** · **Cantidad** (real, con signo) · **Existencia nueva/total**.
- Filtros: rango de fechas + tipo de movimiento.
- Read-only.

**d) Quitar** la sección "Existencias" del `@if (_pestanaInventario == "generales")` (líneas ~404-421). `_existenciaEditable` / `AlCambiarExistencia` se mueven a la pestaña Existencias.

**e)** La pestaña `lotes` actual se **funde** en "Existencias" (o se deja sólo para el alta rápida). Evitar dos lugares para lo mismo.

### 3.2 `Ventas/Facturacion.razor` — multi-lote + negativo

**a) Multi-lote por línea.** Reemplazar `_modalLotes` de "elegí uno" por un modal de **reparto**:
- Tabla de lotes disponibles (no vencidos, `Cantidad > 0`) con un input "a consumir" por fila.
- Validar en cliente: `Σ a-consumir == cantidad de la línea`; ningún lote por encima de su existencia.
- Al aceptar: la `LineaVenta` guarda `List<(IdStockLote, Cantidad, NumeroLote)>` en vez de un solo `IdLote`.
- Al emitir: `FacturaDetallesDTO.Lotes = [{ IdStockLote, Cantidad }]` (campo nuevo; ver §0 contratos). Mantener `Lote`/`NumeroLote` para el caso de un solo lote (compat).
- Para **lote único**: autoseleccionar el lote único, sin modal.

**b) Filtro de vencidos.** Además de confiar en el API, filtrar en cliente `l.Vencimiento == null || l.Vencimiento.Date >= DateTime.Today`. (Requiere `Vencimiento` nullable en `StockLoteDTO` — partial.)

**c) Existencia negativa.**
- Cargar el flag del perfil: `Sesion.PermitirExistenciaNegativa` (nuevo, desde `IContextoSesion` / login — coordinar con el API).
- Antes de agregar la línea (o al emitir), si la cantidad total pedida (línea + lo ya en el detalle) supera la existencia disponible **y** `!Sesion.PermitirExistenciaNegativa` → `Dialogos.ErrorAsync("No hay existencia suficiente de <artículo>.")` y no agregar.
- El API igual valida (autoridad); esto es sólo UX.

**d) Preventa.** Si hay pantalla de preventa (o se reutiliza Facturación con `Preventa=true`), aplica lo mismo: multi-lote, negativo, y el API descuenta/reserva según decida el negocio (doc API §6.1).

### 3.3 `Compras/Compra.razor` — multi-lote

**a) Alta manual.** Si `_articuloActual.Lote`:
- Lote único → sólo cantidad (el lote lo pone el API).
- Lote normal → **mini-tabla** de lotes: `número`, `vencimiento`, `cantidad`; botón "agregar lote"; validar `Σ cantidad == cantidad de la línea`.
- La `LineaCompra` guarda `List<(Numero, Vencimiento?, Cantidad)>`.

**b) Importar XML.** El modal de lote por línea (`_modalLotesXml` / editor `EditorXml.Lotes`) pasa a **tabla multi-lote** por línea (misma validación). Mantener el aviso "indique lote y vencimiento" hasta que `Σ == cantidad`.
- Reforzar el gate C1: el botón "Aplicar a compra" ya exige asociación + vínculo; añadir que cada línea con artículo de lote tenga sus lotes cuadrados.

**c) DTO.** `FacturaCompraDetalleDTO.Lotes = [{ Numero, Vencimiento?, Cantidad }]` (campo nuevo; partial). `loteArticulo` singular queda de compat.

### 3.4 Devoluciones — `DevolucionesVenta.razor` / `DevolucionesCompra.razor`

- Al cargar la factura/compra original, por cada línea de un artículo con lote: mostrar de qué lote(s) salió (el API debe devolverlo en el detalle original) y dejar **elegir el lote a devolver** + cantidad.
- Artículo sin lote / lote único → sin selector (usa el único).
- Enviar `DevolucionVentaDetalleDTO.IdStockLote` (o lista) — campo nuevo, partial.
- Tras registrar, el movimiento aparece en "Movimientos del inventario" (nada extra en el sitio).

### 3.5 Toma física — pantalla nueva (`Views/Compras/TomaFisica.razor`)

- `@page "/buys/physical-count"` (+ alias a la ruta vieja `/buys/take` si se quiere). Nodo de menú en `Class/MenuSeePos.cs` bajo **Compras**, código `COMPRAS.TOMA_FISICA` (agregarlo también a `seed-seguridad.json` de ambos repos y subir el conteo de `FiltroMenuTests`).
- **Responsive obligatorio**: usar `AppPantalla` + `AppRejilla` con `Nivel="NivelPantalla.Movil"` o layout de tarjetas en `< md`. Probar en 375px, 768px, 1280px.
- Contenido:
  - Filtros: bodega, familia, texto. `GET TomaFisica/Articulos`.
  - Tabla: código · descripción · (lote / "lote único" / "sin lote") · **existencia sistema** · input **contado** · diferencia (calculada en cliente para feedback).
  - Excluir `Servicio == true` (lo hace el API, pero no mostrarlos).
  - Botón "Guardar toma" → `POST TomaFisica/Guardar` con las líneas que cambiaron.
- Al guardar: mostrar el **reporte** que devuelve el API en un modal / página:
  - artículos ajustados, unidades ganadas/perdidas, **costeo de pérdidas**.
  - link "ver en movimientos" (los movimientos de toma quedan con `existencia anterior` / `actual` / `total` reseteado).
- Guardar el reporte permite reconsulta: `GET TomaFisica/Reporte?id=`.

### 3.6 Proxies / contratos a crear

| Archivo | Contenido |
|---|---|
| `ApiConexion/InventarioMovimientosApiCliente.cs` + `ProxyInterface/IInventarioMovimientos.cs` + `ProxyClass/InventarioMovimientos.cs` | `Movimientos(idArticulo, desde, hasta, tipo, page)`, `ExistenciaConsolidada(idArticulo)`, `ActualizarExistencia(req)`, `EditarLote(dto)`. |
| `ApiConexion/TomaFisicaApiCliente.cs` + interface + proxy | `Articulos(filtro)`, `Guardar(req)`, `Reporte(id)`. |
| `DTOs/Lotes/StockLoteDTO.Lotes.cs` | partial: `Vencimiento` nullable (o `VencimientoNullable`), `EsUnico`, `Bloqueado`. |
| `DTOs/Lotes/FacturaDetallesDTO.Lotes.cs` | partial: `List<LoteConsumoDTO> Lotes`. |
| `DTOs/Lotes/FacturaCompraDetalleDTO.Lotes.cs` | partial: `List<LoteIngresoDTO> Lotes`. |
| `DTOs/Lotes/DevolucionVentaDetalleDTO.Lotes.cs` / `...Compra...` | partial: `IdStockLote` / `List<LoteConsumoDTO>`. |
| `Program.cs` | registrar los `HttpClient` nuevos con `.AddHttpMessageHandler<ApiAuthHeaderHandler>()` (patrón de `BonificacionApiCliente`). |

### 3.7 Sesión / perfil

- `IContextoSesion` + su implementación: agregar `bool PermitirExistenciaNegativa` (viene del login / claim del perfil, coordinado con el API doc §3.1).
- Usarlo en Facturación (§3.2c) y donde se valide salida de stock.

---

## 4. Tests web a agregar

- `MenuCodigosTests` / `FiltroMenuTests`: al agregar `COMPRAS.TOMA_FISICA` sincronizar `Fixtures/seed-seguridad.json` con el del API y subir el conteo del árbol.
- Servicio de reparto de lotes en Facturación: si se extrae a un `Services/RepartoLotes.cs` (recomendado, como `BonificacionCalculo`), test de "Σ consumos == cantidad", "no exceder existencia", "lote único autoselecciona".
- Toma física: si el cálculo de diferencia/costeo se hace en cliente para preview, test de ese servicio.

---

## 5. Orden de implementación sugerido

1. **Contratos y proxies** (§3.6, §3.7) — sin UI todavía, compila.
2. **Inventario: pestaña Existencias** (§3.1b) + **pestaña Movimientos** (§3.1c). Quitar "Existencias" de Generales.
3. **Inventario: configuración de lote / lote único** (§3.1a) — depende del API.
4. **Facturación multi-lote + negativo** (§3.2).
5. **Compras multi-lote** (§3.3).
6. **Devoluciones con lote** (§3.4).
7. **Toma física** (§3.5) — pantalla nueva + menú + responsive.

Cada paso: `dotnet build` 0 errores, `dotnet test` verde, y click-through manual contra el API de la rama correspondiente.

---

## 6. Depende de decisiones del API (ver `MEJORA_LOTES_API.md` §6)

- Preventa: ¿descuenta o reserva? → afecta §3.2d.
- Enum `Movimiento` y sus etiquetas → afecta el filtro y las columnas de §3.1c.
- `TipoArticulo`: catálogo vs enum → afecta el selector de §3.1a y el backfill.
- Multi-bodega real o bodega única → afecta si las pantallas muestran selector de bodega.
- Forma del `DevolucionVentaDetalleDTO` original (si trae de qué lote salió) → afecta §3.4.

---

## 7. Bitácora

Decisiones del API (§6) resueltas: `TipoArticulo` = enum fijo; **multi-bodega
real**; devolución de venta suma; costeo por `Inventario.Costo`; **preventa =
reserva**; consignación `Stock.Tipo=2`.

### Paso 1 — Contratos y sesión — HECHO

- `IContextoSesion.PermitirExistenciaNegativa` + claim `seepos:permiteExistenciaNegativa`
  (escrito en `ServicioAutenticacion.ConstruirClaims`; añadido a `PerfilLoginDTO`,
  `PerfilDTO` y `Autenticacion` planos). Test doubles de `IContextoSesion` actualizados.
- `DTOs/Generated/SeePosDtos.cs`: `StockLoteDTO.Vencimiento` → `DateTimeOffset?`
  (lote único no vence) + `EsUnico`/`Bloqueado`. Call sites de `.Vencimiento`
  ajustados a nullable en Facturacion/Inventario/Compra.
- `DTOs/Lotes/LotesDTOs.cs` (a mano): movimientos, existencia consolidada,
  actualizar existencia, consumo/ingreso de lote, toma física.
- `ApiConexion/LotesApiCliente.cs` (`ILotesApiCliente`): `Movimientos`,
  `ExistenciaConsolidada`, `ActualizarExistencia`, `TomaArticulos`,
  `TomaGuardar`, `TomaReporte`. Registrado en `Program.cs` con `ApiAuthHeaderHandler`.
- Build 0 errores, 72/72 tests.
- **Pendiente de contrato:** partials para `FacturaDetallesDTO.Lotes` y
  `FacturaCompraDetalleDTO.Lotes` (se añaden en los pasos 4/5).

### Paso 2 — Inventario: pestañas Existencias y Movimientos — HECHO

- `Views/Inventario/Consulta.razor` (+ `@inject ILotesApiCliente`):
  - Pestaña **Existencias**: `ExistenciaConsolidada` — total, por bodega y por
    lote (vencido/único/bloqueado). Ajuste de existencia por bodega/lote vía
    `ActualizarExistencia`; los lotes bloqueados sólo dejan tocar la existencia.
  - Pestaña **Movimientos**: `Movimientos` paginada, filtro por fechas, columnas
    fecha/tipo/documento/usuario/contraparte/lote/anterior-cantidad-nueva.
- La sección "Existencias" de la pestaña Generales se deja como está (moverla
  del todo es un follow-up menor).

### Paso 7 — Toma física — HECHO

- `Views/Compras/TomaFisica.razor` (`/buys/physical-count` + `/buys/take`):
  filtro, tabla responsive con existencia del sistema + input contado +
  diferencia en vivo; guardar → reporte (ajustados / ganadas / perdidas /
  costeo) en modal. `ILotesApiCliente` (`TomaFisica/*`).
- Nodo de menú `COMPRAS.TOMA_FISICA` (Compras) + `seed-seguridad.json`
  (fixture y API). `FiltroMenuTests` conteo 101 → 102.

### Paso 3 — Tipo de artículo + lote único — HECHO

`InventarioDTO.Lotes.cs` partial (`TipoArticulo`/`LoteUnico`/`LoteUnicoFijado`).
`Inventario/Consulta.razor`: el tipo se toma del DTO (1=Normal, 2=MP, 3=PT), con
fallback a `EsPadre`. MP/PT → "Maneja lotes" forzado. MP → checkbox "Lote único
(sin vencimiento)". `LoteUnicoFijado` → todo de sólo lectura con candado.
`AlCambiarTipoArticulo` sincroniza `Lote`/`LoteUnico`/`EsPadre`/`TipoArticulo`.
(El API alineó su enum a esta numeración — commit `4d66f5e2`.)

### Paso 4 — Facturación multi-lote — HECHO

`FacturaDetallesDTO.Lotes.cs` partial. `_modalLotes` pasa de "elegí uno" a un
**reparto**: tabla de lotes con "a consumir" por fila, valida `Σ == cantidad` y
`≤ existencia` (input `max`). Filtra vencidos en cliente. `LineaVenta.Lotes` +
`AgregarArticuloConLotes`; `Emitir` manda `Lotes`. El guard de negativo lo hace
el API (devuelve el mensaje).

### Paso 5 — Compras — HECHO (parcial)

`FacturaCompraDetalleDTO.Lotes.cs` partial. `Compra.razor` emit manda `Lotes`
con un elemento cuando el artículo lleva lote (además de `loteArticulo` compat),
para que el API use `InsertarLineaCompra` (decide por config + upsert). La
**mini-tabla multi-lote por línea** en la UI queda de follow-up.

### Paso 6 — Devoluciones con lote — HECHO

`DevolucionDetalle.Lotes.cs` partial (`IdStockLote?` en venta y compra).
`DevolucionesVenta.razor` / `DevolucionesCompra.razor`: la línea de devolución
hereda `IdStockLote` del lote de la venta/compra original y lo manda en el
detalle. (Selector explícito de lote para ventas multi-lote: follow-up.)

### Follow-ups menores — HECHOS

- **Existencias fuera de Generales**: `Inventario/Consulta.razor` — se quitó el
  input de existencia-actual (y su método `AlCambiarExistencia`); min/max pasan a
  "Niveles de reposición"; un aviso remite a las pestañas Existencias/Movimientos.
- **Multi-lote en Compras**: `Compra.razor` alta manual — repetidor "+ Otro lote"
  bajo el lote principal; al agregar la línea se arma `LineaCompra.Lotes` y el
  primer lote absorbe el resto para cuadrar con la cantidad. Emit prefiere
  `l.Lotes`.
- **Guard de existencia negativa (sin lote)**: `Facturacion.razor` — al agregar
  un artículo sin lote, si el perfil no permite negativo, consulta
  `ExistenciaConsolidada` y bloquea si `total − ya_en_detalle − cantidad < 0`.
- **No hecho (a propósito)**: selector explícito de lote en Devolución de venta
  para ventas multi-lote — caso muy de borde; hoy la devolución hereda el lote de
  la venta original y el API cae en lote único / sin lote si no se manda.
