# Bodegas por centro, traslados y consignación por traslado — Sitio web (`SuvesaPosSitioAplicacion`, Blazor)

> Documento de trabajo. Repo: `SuvesaPosSitieWebNew`, rama base **`feature/ola-0-cimientos`**.
> Gemelo del API: `DevSuvesaPosWeb/ApiSuvesaPos/docs/BODEGAS_POR_CENTRO_Y_TRASLADOS_API.md` — **leerlo primero**:
> el sitio depende de los endpoints, campos y migraciones que ahí se definen.
> **Lee también** `docs/MEJORA_LOTES_WEB.md` y `docs/CONSIGNACION_WEB.md`: la ficha de movimientos, la pestaña
> Existencias por bodega y todo el módulo de consignación ya están implementados ahí; este documento sólo
> cubre lo que **falta** para los 8 puntos.
> **Regla del repo:** el API es la autoridad. El sitio pinta, valida en cliente para UX y manda intención.
> Nada de lógica de inventario en el navegador.
> **No se toca código con este documento: es análisis + plan.** No se crean ramas: se trabaja en
> `feature/ola-0-cimientos`.

---

## 0. Requerimiento (los 8 puntos)

Ver `BODEGAS_POR_CENTRO_Y_TRASLADOS_API.md §0`. Resumen de lo que toca al **sitio**:

1. Stocks por bodega; bodega pertenece a un **centro**; centro = N bodegas → el sitio filtra bodegas por el
   **centro de la sesión** (`IContextoSesion.IdSucursal`, ya existe) y el mantenimiento de bodegas deja elegir
   centro.
2. **Venta, compra y preventa**: selector de **bodega** (hoy no existe; el sitio manda `IdBodega = 0` y el API
   cae a la bodega por defecto).
3. **Ficha de movimientos**: filtro y columna **bodega** (hoy la pestaña Movimientos no los tiene). La pestaña
   **Existencias** ya es por bodega — sin cambio. La **toma física** (`Compras/TomaFisica.razor`) también se
   hace **por bodega**: hoy manda `Bodega = 0` fijo; falta el selector.
4. Pantalla nueva de **traslado bodega → bodega**.
5. El **ajuste de consignación** (ingreso/salida) se explica como traslado desde/hacia una **bodega de
   consignación central**; la pantalla lo refleja.
6. Las bodegas de **clientes de consignación** (y la central) **no aparecen** en ningún selector fuera del
   módulo de consignación.
7. Al crear la bodega de un cliente e ingresarle productos, la pantalla muestra que **se rebaja la central** y
   **aumenta la del cliente** (validación y mensajes vienen del API).
8. **Inventario físico de consignación**: al elegir el cliente, **precargar** todos sus artículos + lotes con
   existencia, dejar digitar cantidades, **validar que todo fue contado** y guardar.

---

## 0bis. Cobertura de los 8 puntos (checklist)

| # | Punto | Secciones que lo cubren | Estado de partida en el sitio |
|---|---|---|---|
| 1 | Bodegas filtradas por el centro de la sesión + mantenimiento con centro | §1.1, §1.2, §2·1, §3.1, §5·1 | `Sesion.IdSucursal` ya existe; `bodega/ObtenerBodegas` trae todo |
| 2 | Selector de bodega en venta/compra/preventa | §1.2, §1.5, §2·2, §3.2, §4, §5·3 | **no existe**; se manda `IdBodega = 0` |
| 3 | Ficha de movimientos por bodega + **toma física por bodega** (actualizar existencia ya es por bodega) | §1.3, §1.3b, §2·3, §2·3b, §3.3, §3.3b, §5·2, §5·2b | pestaña Existencias ya multi‑bodega; Movimientos sin bodega; TomaFisica manda `Bodega = 0` fijo |
| 4 | Pantalla de traslado bodega ↔ bodega | §1.5, §2·4, §3.4, §4, §5·4, §5bis | no hay pantalla ni nodo de menú |
| 5 | Ajuste de consignación explicado como traslado desde/hacia la central | §2·5, §3.5, §5·5 | `Ajuste.razor` no menciona la central |
| 6 | Bodegas de consignación fuera de todo selector salvo el módulo | §1.5, §2·6, §3.1, §3.6, §5·7 | ninguna fuente filtrada hoy |
| 7 | Al ingresar productos: se ve "‑N central / +N cliente" y se bloquea si la central no tiene | §2·7, §3.5, §5bis | no se ve efecto ni validación |
| 8 | Inventario físico de consignación: precargar artículos+lotes del cliente + validar "todo contado" | §1.4, §1.5, §2·8, §3.7, §4, §5·6 | conteo manual, sin precarga ni validación |

Ningún punto queda sin diseño, contrato a escribir y lugar en el orden de implementación.

---

## 1. Estado actual (mapa)

### 1.1 Sesión y centro

- El **centro se pide siempre justo después del login** (`/cuenta/sucursal`) y vive en la **cookie del sitio**:
  `IContextoSesion.IdSucursal` / `NombreSucursal` (claims `seepos:idSucursal` / `seepos:nombreSucursal`).
  `MainLayout` obliga a tenerlo antes de operar. **Las pantallas sólo lo leen (`Sesion.IdSucursal`); nunca lo
  piden de nuevo.**
- **El token de la API NO lleva el centro** (`construirToken` sólo emite `Usuario` y
  `PermiteExistenciaNegativa`). Por eso **el sitio debe MANDAR `idSucursal`** en cada llamada que lo necesite
  (query string en los GET de bodegas / toma física / traslado; campo del body en los POST), igual que ya
  manda `FacturaDTO.IdSucursal`. El proxy de bodegas (`Bodegas.DeMiCentroAsync()`, §3.1) lo pone solo desde
  `Sesion.IdSucursal`.

### 1.2 Bodegas

- Mantenimiento: `Views/Parametros/Bodegas.razor` (`PARAMETROS.BODEGAS`, `/parameters/wineries`) — lista de
  bodegas. Proxy vía `bodega/ObtenerBodegas` (o el CRUD `api/mantenimientos/bodegas`).
- Selector de bodega en venta/compra/preventa: **no existe**. `Facturacion.razor`, `Compra.razor`,
  `Proforma.razor` no piden bodega; la línea sale con `IdBodega = 0`.
- `Compra.razor`, `Facturacion.razor`, `DevolucionesVenta/Compra.razor`, `Produccion/*.razor` mencionan
  "bodega" sólo de pasada (lotes, existencia consolidada), sin selector.

### 1.3 Ficha del artículo — `Views/Inventario/Consulta.razor`

- Pestaña **Existencias** (`_pestanaInventario == "existencias"`): tabla **por bodega** (`_existenciaConsol.PorBodega`)
  con "Ajustar a" por fila → `AjustarExistencia(idBodega, idLote?)` → `InventarioMovimientos/ActualizarExistencia`.
  Para artículos con lote, tabla de lotes con su `PorBodega`. **Ya es multi‑bodega.**
- Pestaña **Movimientos** (`_pestanaInventario == "movimientos"`): filtro Desde/Hasta, tabla con Fecha, Tipo,
  Documento, Usuario, Prov./Cliente, Lote, Anterior, Cantidad, Nueva. **Sin filtro ni columna de bodega.**
- Proxy: `ILotesApiCliente` (`ExistenciaConsolidadaAsync`, movimientos, actualizar existencia) —
  `ApiConexion/LotesApiCliente.cs`, DTOs en `DTOs/Lotes/*`.

### 1.3b Toma física — `Views/Compras/TomaFisica.razor`

- Proxy `ITomaFisicaApiCliente` (`TomaArticulosAsync`, `GuardarAsync`, reporte) — patrón `LoteEnvelope<T>`.
- **Manda `Bodega = 0` fijo** en `TomaArticulosAsync` y en el guardado. No hay selector de bodega, así que la
  toma cae siempre a la bodega por defecto del API. El API **ya** soporta contar por bodega
  (`TomaFisicaFiltro.Bodega`) — sólo falta el selector en la pantalla.

### 1.4 Consignación — `Views/Consignacion/*` (ya portado, ver `CONSIGNACION_WEB.md`)

| Pantalla | Ruta / código | Qué hace |
|---|---|---|
| `Bodegas.razor` | `/consignment/warehouses` · `CONSIGNACION.BODEGAS` | Lista bodegas de consignación + estado (Activa/Cerrada) + "Abrir bodega" |
| `Ajuste.razor` | `/consignment/adjust` · `CONSIGNACION.AJUSTE` | Pestañas **Entrada / Salida**: buscador cliente + artículo + lote, cantidades, cierre total; "Anular boleta por N.º" |
| `InventarioFisico.razor` | `/consignment/count` · `CONSIGNACION.INVENTARIO_FISICO` | Conteo del agente. **Hoy: buscar y agregar artículo por artículo a mano** (`_texArt` → `ElegirArt` → `AgregarLinea`). Sin precarga. Sin validación de "todo contado". |
| `Kardex.razor` | `/consignment/ledger` · `CONSIGNACION.KARDEX` | Movimientos de la bodega del cliente + badge "Consignación cerrada" + export CSV |
| `Prefactura.razor` | `/consignment/prebill` · `CONSIGNACION.FACTURACION_DE_CONSIGNACIONES` | Abrir prefactura, Aprobar / Facturar (Contado‑Crédito + plazo) / Anular; tabla de prefacturas |

- Proxy: `IConsignacionInvApiCliente` (`ApiConexion/ConsignacionInvApiCliente.cs`, patrón `LoteEnvelope<T>` +
  `ContextoLlamada.Token`), DTOs en `DTOs/Consignacion/*`.
- `InventarioFisico.razor` usa `LotesApi.ExistenciaConsolidadaAsync` (stock `Tipo = 1`) sólo para sacar ids de
  lote válidos — **no** refleja la existencia de consignación (`Tipo = 2`).

### 1.5 Lo que NO existe en el sitio

- Selector de bodega en venta/compra/preventa (#2).
- Filtro/columna de bodega en la pestaña Movimientos (#3).
- Pantalla de traslado bodega→bodega (#4) — no hay ni nodo de menú.
- Precarga del inventario físico de consignación desde la existencia del cliente (#8).
- Filtrado de bodegas por centro de la sesión (#1) y exclusión de las de consignación en selectores (#6) —
  hoy `bodega/ObtenerBodegas` trae todo.

---

## 2. Requerimiento → gap → propuesta (resumen)

| # | Gap en el sitio | Propuesta (detalle en §3) |
|---|---|---|
| 1 | Bodegas no se filtran por centro | Proxy de bodegas manda `IdSucursal` de la sesión; mantenimiento deja elegir centro y muestra la columna |
| 2 | Sin selector de bodega en transacciones | Selector de bodega (cabecera) en `Facturacion.razor`, `Compra.razor`, `Proforma.razor`; default = bodega principal del centro; se manda `IdBodega` en cada línea/cabecera |
| 3 | Movimientos sin bodega | Añadir filtro + columna **Bodega** en la pestaña Movimientos (depende de `MovimientoInventarioFiltroDTO.Bodega` + `IdBodega`/`NombreBodega` en la fila — API §3.3) |
| 3b | Toma física manda `Bodega = 0` | Selector de **bodega** en `Compras/TomaFisica.razor` (centro‑filtrado, sin consignación); mandar `Bodega` en `TomaArticulosAsync` y en el guardado. Es la bodega que se está contando |
| 4 | No hay pantalla de traslado | `Views/Inventario/Traslado.razor` (o bajo Compras) + nodo de menú + proxy `TrasladoBodegaApiCliente`. **Origen y Destino sólo bodegas del centro de la sesión** (no se puede trasladar a otro centro) |
| 5 | El ajuste de consignación no explica el traslado | `Ajuste.razor`: mostrar "Origen: bodega de consignación central" / "Destino: bodega del cliente" y viceversa; mostrar el **disponible en la central** por línea; propagar el bloqueo del API si no alcanza |
| 6 | Bodegas de consignación aparecen en selectores | El proxy general de bodegas ya viene filtrado por el API (`!EsConsignacion`); el sitio no las lista en ningún `<select>` fuera de `Views/Consignacion/*` |
| 7 | No se ve el efecto del ingreso | `Ajuste.razor` (Entrada): **no deja registrar** si la central no tiene disponible (botón deshabilitado / aviso); al guardar OK, mostrar el resumen "‑N en central, +N en <cliente>" que devuelve el API |
| 8 | Conteo manual, sin validación | `InventarioFisico.razor`: al elegir cliente → `ConsignacionInventario/Existencia` → precargar una fila por artículo+lote; input de cantidad por fila; bloquear "Guardar" hasta que todas tengan cantidad (0 es válido) |

---

## 3. Diseño propuesto (sitio)

### 3.1 Bodegas por centro (#1)

- **Proxy de bodegas.** Donde el sitio pide la lista de bodegas para un `<select>` de operación, mandar
  `IdSucursal = Sesion.IdSucursal`. El API devuelve las del centro + las globales (`IdSucursal = null`), sin
  las de consignación. Un método `Bodegas.DeMiCentroAsync()` centraliza esto (proxy `IBodegas` /
  `ApiConexion/ProxyClass/Bodegas.cs` — crear si no existe, patrón `ProxyBase`).
- **Mantenimiento** `Views/Parametros/Bodegas.razor`: columna **Centro**; en alta/edición un `<select>` de
  centro (proxy `ISucursales` ya existe) — opcional dejar "— Global —". Filtro "Centro" arriba (por defecto el
  de la sesión, opción "Todos").
- **DTO** `BodegaDTO` (o el partial del sitio): `IdSucursal` (int?), `NombreSucursal`, `EsConsignacion`,
  `EsConsignacionCentral`.

### 3.2 Selector de bodega en venta / compra / preventa (#2)

- **`Facturacion.razor`**: en la cabecera (junto a cliente / condición) un `<select>` **Bodega** poblado con
  `Bodegas.DeMiCentroAsync()`. Default: la bodega marcada como principal del centro, o la primera, o la última
  usada (guardar en `ProtectedLocalStorage` por centro, patrón del espacio de trabajo). Al emitir, cada
  `FacturaDetallesDTO.IdBodega` = la bodega elegida (una por documento; si en el futuro se quiere por línea, el
  modelo ya lo soporta).
- **`Compra.razor`**: igual, `<select>` Bodega en la cabecera; `FacturaCompraDTO.IdBodega` / cada línea.
  En "Importar XML" la bodega es la de la cabecera.
- **`Proforma.razor`** (preventa/cotización): `<select>` Bodega en la cabecera; se manda en la preventa para
  que la **reserva** quede en esa bodega y al facturarla se descuente la misma (el API lo respeta).
- **UX**: si el API responde `ExigirBodegaEnTransaccion` (config) y no hay bodega elegida, bloquear "Emitir"
  con aviso. Mientras la config esté en `false`, el `<select>` es obligatorio en el sitio igual (mejor pedirlo
  siempre).
- Sin lógica de stock en el navegador: el guard de negativo y el reparto por lote siguen del lado del API.

### 3.3 Bodega en la ficha de movimientos (#3)

`Views/Inventario/Consulta.razor`, pestaña `movimientos`:
- Añadir un `<select>` **Bodega** al bloque de filtros (Desde/Hasta) — poblado con las bodegas del centro
  (incluida opción "Todas"). Mandar `Bodega` en el filtro del proxy (`ILotesApiCliente` /
  `MovimientoInventarioFiltroDTO`).
- Añadir columna **Bodega** a la tabla (`m.NombreBodega`), entre "Tipo" y "Documento".
- La pestaña **Existencias** ya está por bodega — sin cambio (salvo mostrar el centro de cada bodega si el API
  lo añade a `ExistenciaPorBodegaDTO`).

Depende de: API §3.3 (`MovimientoInventarioFiltroDTO.Bodega?`, `MovimientoInventarioConsultaDTO.IdBodega` +
`NombreBodega`). Hasta que exista, la columna se puede dejar oculta.

### 3.3b Toma física por bodega (#3)

`Views/Compras/TomaFisica.razor`:
- `<select>` **Bodega** arriba del listado, poblado con `Bodegas.DeMiCentroAsync()` (centro de la sesión, sin
  consignación). Sin opción "Todas": la toma física se hace de **una** bodega a la vez.
- Mandar la bodega elegida en `TomaArticulosAsync(new TomaFisicaFiltro { Bodega = _bodega, ... })` y en el
  `GuardarAsync`. Quitar el `Bodega = 0` fijo.
- El reporte de cierre muestra la bodega contada. Recordar la última bodega usada en `ProtectedLocalStorage`
  por centro (igual que el selector de facturación, §3.2).

### 3.4 Pantalla de traslado bodega → bodega (#4)

- **Proxy** `ApiConexion/TrasladoBodegaApiCliente.cs` (`ITrasladoBodegaApiCliente`, patrón `LoteEnvelope<T>`):
  `RegistrarAsync(TrasladoBodegaRequest)`, `ObtenerAsync(long)`, `ListarAsync(TrasladoBodegaFiltro)`,
  `AnularAsync(id, motivo)`. DTOs en `DTOs/Traslado/*` (partials a mano, no NSwag — regen no viable, ver
  `MEJORA_LOTES_WEB.md §0`).
- **Pantalla** `Views/Inventario/Traslado.razor` (ruta `/inventory/transfer`), nivel Tableta:
  - `<select>` **Origen** y **Destino**, ambos poblados **sólo con las bodegas del centro de la sesión**
    (`Bodegas.DeMiCentroAsync()`, sin consignación), distintas entre sí. **No hay forma de elegir una bodega de
    otro centro** — el traslado inter‑centro no existe (decisión fijada). Si el API rechaza por centro
    distinto, se muestra el mensaje tal cual.
  - Buscador de artículo (patrón `AppBuscadorArticulo`) + selector de lote (si maneja lote) — la existencia
    disponible en el origen la muestra el API vía `ExistenciaConsolidada` (filtrada a la bodega origen) sólo
    como ayuda; la validación real es del API.
  - Tabla de líneas (artículo, lote, cantidad, quitar). Botón "Registrar traslado".
  - Al guardar, mostrar la boleta (`TrasladoBodegaDTO`) con el costo total y el resumen por artículo.
  - Sección "Traslados recientes" (`ListarAsync`) con "Ver" y "Anular" (modal de motivo).
- **Menú**: nuevo nodo bajo **Inventario** (o **Compras**, alinear con dónde vive Toma Física —
  `Compras/TomaFisica.razor`). Código `INVENTARIO.TRASLADO_BODEGA` (o `COMPRAS.TRASLADO_BODEGA`).
  Requiere: alta en `Class/MenuSeePos.cs`, en `Fixtures/seed-seguridad.json` **y** en la semilla del API
  (`SecuritySystem/Seed/seed-seguridad.json`), subir el conteo de nodos en
  `FiltroMenuTests.ElMenuRealSeCargoCompleto`, y `MenuCodigosTests` verde.

### 3.5 Consignación como traslado (#5, #7)

`Views/Consignacion/Ajuste.razor`:
- Pestaña **Entrada**: rótulo "Traslada desde la **bodega de consignación central** hacia la bodega de
  **<cliente>**". Pestaña **Salida**: al revés.
- **No se puede registrar una entrada si la central no tiene disponible** el artículo/lote (regla dura del
  API, clave para no desequilibrar el inventario). En la tabla de líneas mostrar, por fila, el **disponible en
  la central** (de `ConsignacionInventario/Existencia` de la central, o del mensaje del API); si alguna línea
  pide más de lo disponible, marcar la fila y **deshabilitar "Registrar"**. Aun así, la validación final la
  hace el API y su mensaje ("La bodega de consignación central no tiene N unidades de X") se muestra tal cual.
- Al guardar OK, mostrar el resumen que devuelve el API: por artículo, "‑N en central / +N en cliente" (o el
  inverso en salida).
- Si el negocio aprueba `ReponerCentral` (API §6.5), añadir una pestaña/acción "Reponer central" (traslado de
  una bodega operativa → central). Si no, la central se repone con una compra normal cuyo destino es la
  central (que sólo se elige desde el mantenimiento / el módulo de consignación).
- `Bodegas.razor` (consignación): al "Abrir bodega" de un cliente, dejar claro que la mercadería se le carga
  con una **Entrada** (traslado desde la central), no aparece sola.

### 3.6 Ocultar bodegas de consignación en selectores (#6)

- El proxy general de bodegas (`Bodegas.DeMiCentroAsync()`) ya viene filtrado por el API (`!EsConsignacion`).
- Auditar que **ningún** `<select>`/lista de bodegas fuera de `Views/Consignacion/*` use una fuente sin
  filtrar. Hoy los candidatos: el nuevo selector de venta/compra/preventa (§3.2), el de la ficha de
  movimientos (§3.3), el de traslado (§3.4), Toma Física (`Compras/TomaFisica.razor`), Producción.
- El mantenimiento `Views/Parametros/Bodegas.razor` puede tener un check "Ver bodegas de consignación" (sólo
  para administración) que pida `incluirConsignacion=true`; por defecto no las muestra.

### 3.7 Precarga del inventario físico de consignación (#8)

`Views/Consignacion/InventarioFisico.razor` — reescribir el bloque de captura:
- Añadir a `IConsignacionInvApiCliente`: `ExistenciaAsync(long idCliente)` →
  `ConsignacionExistenciaDTO { IdCliente, NombreCliente, IdBodega, NombreBodega, BodegaAbierta, Articulos[] }`
  (DTO en `DTOs/Consignacion/*`).
- `ElegirCliente(c)` → llama `ExistenciaAsync(c.Identificacion)`. Si la bodega no está abierta, aviso y no
  deja contar. Si hay artículos, **precargar `_lineas`**: una fila por `(IdArticulo, IdStockLote)` con
  `CodArticulo`, `Descripcion`, `NumeroLote`, `Vencimiento`, existencia de sistema (`Consignado`, sólo lectura)
  y un input **Físico** editable (arranca vacío / null, no 0).
- Mantener el buscador de artículo **sólo** para agregar algo que no estaba en la precarga (sobrante).
- **Validación "todo contado"**: el botón "Guardar conteo" se habilita cuando **todas** las filas precargadas
  tienen un `Físico` digitado (`0` es válido y significa "vendido todo"). Mostrar un contador "contados X / Y".
  Mandar `RegistrarConteoRequest { ..., Completo = true, Lineas = todas }`. Si el API responde que falta
  alguna (carrera con un movimiento nuevo), mostrar la lista.
- Tras guardar, el resto del flujo (respuesta con consignado/vendido/sobrante, botón "Generar prefactura")
  queda igual.

---

## 4. Contratos / DTOs a escribir a mano

Regen NSwuag completo no es viable (rompe ~50 proxies — `MEJORA_LOTES_WEB.md §0`). Para este trabajo:

- `ApiConexion/TrasladoBodegaApiCliente.cs` + `DTOs/Traslado/*` (request, línea, DTO, filtro, resumen).
- `DTOs/Consignacion/ConsignacionExistencia*.cs` (existencia por cliente) + método en
  `IConsignacionInvApiCliente`.
- Partials de `MovimientoInventarioFiltro` (+ `Bodega`) y de la fila de movimientos (+ `IdBodega`,
  `NombreBodega`) en `DTOs/Lotes/*`.
- `BodegaDTO` (+ `IdSucursal`, `NombreSucursal`, `EsConsignacion`, `EsConsignacionCentral`) — partial o el
  DTO del proxy de bodegas.
- `FacturaDetallesDTO` / `FacturaCompraDetalleDTO` / cabeceras: confirmar que `IdBodega` está expuesto; si no,
  partial.

---

## 5. Orden de implementación (sitio)

Cada bloque va **después** del bloque equivalente del API (§5 del doc API). Verificación por commit:
`dotnet build src/SuvesaPosSitioAplicacion/SuvesaPosSitioAplicacion.csproj` +
`dotnet test tests/SuvesaPosSitioAplicacion.Tests/SuvesaPosSitioAplicacion.Tests.csproj` (72 hoy;
al tocar el menú, subir el conteo de `FiltroMenuTests` y actualizar los dos seeds).

1. **Bodegas por centro** (#1): proxy `IBodegas`/`Bodegas.DeMiCentroAsync()`, `BodegaDTO` con centro,
   mantenimiento con columna/selector de centro.
2. **Ficha de movimientos con bodega** (#3): filtro + columna en `Inventario/Consulta.razor`.
2b. **Toma física por bodega** (#3): selector de bodega en `Compras/TomaFisica.razor`; mandar `Bodega` en
   listar y guardar.
3. **Selector de bodega en transacciones** (#2): `Facturacion.razor`, `Compra.razor`, `Proforma.razor`.
4. **Traslado bodega→bodega** (#4): proxy + `Views/Inventario/Traslado.razor` + nodo de menú + seeds +
   `FiltroMenuTests`.
5. **Consignación como traslado** (#5, #7): textos y resúmenes en `Ajuste.razor`; (opcional) "Reponer
   central".
6. **Precarga del conteo de consignación** (#8): `ExistenciaAsync` + reescritura del bloque de captura de
   `InventarioFisico.razor` + validación "todo contado".
7. **Auditoría de selectores de bodega** (#6): confirmar que ninguno fuera de `Views/Consignacion/*` lista
   bodegas de consignación.

---

## 5bis. Reglas ya fijadas por negocio (NO reabrir)

- **Traslado sólo entre bodegas del mismo centro.** Los `<select>` Origen/Destino sólo listan bodegas del
  centro de la sesión; no hay traslado inter‑centro.
- **No se registra una entrada de consignación si la bodega de consignación central no tiene la cantidad
  disponible.** La pantalla muestra el disponible por línea y deshabilita "Registrar"; el API bloquea de todos
  modos. Es para no desequilibrar el inventario.

## 6. Decisiones que necesita negocio (además de las del doc API)

1. **Default del selector de bodega** en facturación/compra/preventa: ¿bodega "principal" del centro (nuevo
   flag), la primera alfabética, o la última usada por el cajero (recordada en el navegador)?
2. **¿El selector de bodega es por documento o por línea?** El modelo del API soporta por línea; el sitio
   propone **por documento** (una bodega por factura/compra/preventa) por simplicidad. Confirmar si algún caso
   real necesita repartir una misma factura entre bodegas.
3. **Dónde vive "Traslado de bodega" en el menú**: bajo **Inventario** o bajo **Compras** (junto a Toma
   Física). 
4. **Mantenimiento de bodegas**: ¿se permite crear/editar la **bodega de consignación central** desde
   `Parametros/Bodegas.razor` (con el flag), o sólo desde el módulo de consignación?

---

## 7. Bitácora

Rama `feature/ola-0-cimientos`. Verificación por commit: `dotnet build` del sitio + `dotnet test` (72) +
(al tocar menú) `FiltroMenuTests` / `MenuCodigosTests` / seeds de ambos repos.

| Paso | Commit | Qué entró | Verificación |
|---|---|---|---|
| §3.4 | _(este)_ | `Views/Inventario/Traslado.razor` (`/buys/warehouse-transfer`, código `COMPRAS.TRASLADO_ENTRE_BODEGAS`): selects Origen/Destino **sólo del centro de la sesión**, buscador de artículo + lote, tabla de líneas, "Registrar traslado" → `LotesApi.TrasladoRegistrarAsync`, "Traslados recientes" con Ver / Anular (modal de motivo). Nodo de menú bajo Compras + fixture `seed-seguridad.json` (site) + `SecuritySystem/Seed/seed-seguridad.json` (API, commit `8b83b256`) + `FiltroMenuTests` 77→78. | `dotnet build` 0 err · `dotnet test` 72/72 |
| §3.1/§3.3/§3.3b | `46c3a5f` | `DTOs/Lotes/LotesDTOs.cs`: `MovimientoInventarioFiltro.Bodega?`, `MovimientoInventarioConsulta.IdBodega`/`NombreBodega`, nuevo `BodegaOperativa` + DTOs de traslado. `ILotesApiCliente` gana `BodegasOperativasAsync(idSucursal?)` (→ `bodega/ObtenerBodegas?idSucursal=`) y `Traslado*Async`. `Inventario/Consulta.razor` pestaña Movimientos: `<select>` Bodega (del centro de la sesión) + columna **Bodega**. `Compras/TomaFisica.razor`: `<select>` **Bodega** obligatorio (una a la vez, centro de la sesión), se manda en `TomaArticulosAsync`/`TomaGuardarAsync` (quitado `Bodega = 0`). | `dotnet build` sitio 0 err · `dotnet test` 72/72 |
