# Calculadora de Producción — Sitio web (`SuvesaPosSitioAplicacion`)

> **Estado: CERRADO para implementación.** Todas las decisiones de negocio están
> resueltas (§5). Ejecutar siguiendo §4 una vez que el API tenga el módulo `Produccion/*`.
>
> Documento de trabajo. Repo: `SuvesaPosSitieWebNew`, rama base `feature/bonificaciones-web`.
> Gemelo del API: `DevSuvesaPosWeb/ApiSuvesaPos/docs/CALCULADORA_PRODUCCION_API.md` — leerlo primero.
> **Depende de** que el API exponga el módulo nuevo `Produccion/*` (ver doc API §3.5).
> "Artículo" = inventario.

---

## 0. Cómo verificar

```bash
cd src && dotnet build SuvesaPosSitioAplicacion/SuvesaPosSitioAplicacion.csproj -v q -nologo
cd .. && dotnet test tests/SuvesaPosSitioAplicacion.Tests/SuvesaPosSitioAplicacion.Tests.csproj -v q --nologo   # 72 hoy
```

Tests que se van a mover: `MenuCodigosTests` (todo `Codigo` del menú debe existir en
`SecuritySystem/Seed/seed-seguridad.json` **y** en
`tests/SuvesaPosSitioAplicacion.Tests/Fixtures/seed-seguridad.json`),
`FiltroMenuTests.ElMenuRealSeCargoCompleto` (cuenta de nodos: hoy 12 raíz / 102 total
→ subirá a **103**).

---

## 1. Estado actual

### 1.1 Dónde vive la función hoy

`Views/Inventario/Consulta.razor`, pestaña **"Fórmula y conversión"**
(`_pestanaInventario == "formula"`), visible sólo si `_tipoArticulo == 3` (Producto
terminado) **y** `!_esNuevo` **y** `Sesion.EsCostaPets`.

Campos: `_formula` (`List<ArticulosRelacionadosDTO>`), `_seleccionLotesFormula`
(`List<LoteFormulaSeleccionado>`), `_codigoFormula`, `_cantidadFormula`,
`_articuloLoteFormula`, `_loteFormula`, `_bodegaFormula`, `_cantidadLoteFormula`,
`_lotesFormula`, `_bodegas`, `_produccionDisponible`, `_cantidadConvertir`,
`_errorFormula`, `_errorLotesFormula`.

Métodos: `AgregarFormula`, `QuitarFormula`, `CargarLotesFormula`, `AgregarLoteFormula`
(dedup por `IdArticulo+IdLote` — **permite el mismo insumo con lotes distintos y no
valida suma**), `FormulaCompleta()` (sólo exige ≥1 lote por insumo),
`CalcularProduccion`, `ConvertirProduccion`, `SolicitudProduccion(bool, int)`.

```csharp
record LoteFormulaSeleccionado(long IdArticulo, long IdLote, int IdBodega,
    string Articulo, string Lote, string Bodega, double Disponible, int Cantidad);
```

`CargarProduccion()` fuerza `_tipoArticulo = 3` si `_formula.Count > 0`.

### 1.2 Proxy

`ApiConexion/ProxyInterface/IProduccionInventario.cs` + `ProxyClass/ProduccionInventario.cs`
(`: ProxyBase`):

| Método | Endpoint API |
|---|---|
| `Bodegas(bool costaPets)` | `Bodega/ObtenerBodegas` |
| `Formula(long principal)` | `ArticulosRelacionados/BuscarArticulosRelacionadosFormula` |
| `GuardarFormula(principal, componente, cantidad, activa)` | `ArticulosRelacionados/putArticuloRelacionadoFormula` |
| `Calcular(CalculadoraDTO)` | `CalculadoraProduccionLotes/CalculadoraProductiva` |

### 1.3 Menú

`Class/MenuSeePos.cs` nodo **Inicio**: `Titulo="Inicio"`, `Codigo="INICIO"`,
`Ruta="/initial"`. Hijos: `INICIO.CLIENTES` (`/initial/customers`),
`INICIO.INVENTARIOS` (`/initial/inventory`), `INICIO.FACTURACION` (`/initial/billing`),
`INICIO.COBRAR`, `INICIO.ENTREGA_A_CUENTA`, `INICIO.DOCUMENTOS_EMITIDOS`,
`INICIO.BANDEJA_FISCAL_V4_4` (`/invoices/fiscal-tray`), `INICIO.DEVOLUCIONES`,
`INICIO.CONSULTA_ALBARANES`.

No hay carpeta `Views/Inicio`: los ítems de Inicio apuntan a páginas en
`Views/Facturacion`, `Views/Clientes`, `Views/Inventario`, …

### 1.4 Patrones reutilizables

- Cliente HTTP a mano + envelope: `ApiConexion/LotesApiCliente.cs`
  (`LoteEnvelope<T>` espeja `ResponseGeneric`; setea `ContextoLlamada.Token` él mismo
  porque se llama directo desde componentes, no vía `ProxyBase` — fix del 401,
  commit `32c073f`). **Copiar este patrón exacto.**
- DTOs a mano: `DTOs/Lotes/LotesDTOs.cs` + partials `DTOs/Lotes/*.Lotes.cs`.
- Pantalla + reporte + export: **Toma Física** (`Views/.../TomaFisica.razor`, paso 7
  de MEJORA_LOTES_WEB) — es el molde para la pantalla nueva y la bitácora.
- `AppPantalla` params: `Pantalla`, `Codigo` (gate `Sesion.PuedeVer`), `Icono`,
  `Subtitulo`, `Nivel`, `Acciones`, `ChildContent`.
- `IServicioDialogos`, `IManejadorRespuestas.DatoAsync<T>` / `CorrectaAsync`.

---

## 2. Requerimiento → cambio en el sitio

| # | Cambio |
|---|---|
| R1 | Renombrar la función a "Calculadora de Producción" (nueva pantalla, ver R6). |
| R2 | Al agregar un componente a la fórmula, sólo se pueden elegir artículos **Materia prima** (`TipoArticulo == 2`): el selector filtra y el API valida. |
| R3 | Selección de "lotes a consumir": **no permitir repetir lote**; por insumo la suma de cantidades debe cuadrar exactamente con lo requerido para la cantidad a producir; deshabilitar "Calcular" / "Convertir" mientras no cuadre, con mensaje de qué insumo falta. |
| R4 | "Convertir" sólo se habilita después de "Calcular disponibles" (ya es así). |
| R5 | Antes de convertir pedir **número de lote** y **fecha de vencimiento** del producto terminado; enviarlos en el request. |
| R6 | Pantalla propia en el menú **Inicio** con la lista de todos los PT; seleccionar → calcular → convertir. |
| R7 | Ver la **bitácora de producción** (tabla filtrable) + **Exportar** (cabeceras+detalle) + **Anular** una producción (revierte todo). |
| — | Quitar la pestaña "Fórmula y conversión" de `Views/Inventario/Consulta.razor`. |

---

## 3. Diseño propuesto (sitio)

### 3.1 Contratos a mano — `ApiConexion/ProduccionApiCliente.cs`

`IProduccionApiCliente` (mismo patrón que `LotesApiCliente`: `HttpClient` +
`IContextoSesion`, `LoteEnvelope<T>`, setea `ContextoLlamada.Token`):

| Método | Endpoint |
|---|---|
| `BodegasAsync()` | `Bodega/ObtenerBodegas` (o reusar el proxy de bodegas existente) |
| `ProductosTerminadosAsync(string? texto)` | `POST Produccion/ProductosTerminados` |
| `FormulaAsync(long idPrincipal)` | `GET Produccion/Formula?idPrincipal=` |
| `GuardarComponenteAsync(GuardarComponenteFormula req)` | `PUT Produccion/GuardarComponente` |
| `CalcularAsync(CalculoProduccionRequest req)` | `POST Produccion/Calcular` |
| `ConvertirAsync(CalculoProduccionRequest req)` | `POST Produccion/Convertir` |
| `ReporteAsync(long id)` | `GET Produccion/Reporte?id=` |
| `ReportesAsync(BitacoraFiltro req)` | `POST Produccion/Reportes` |
| `AnularAsync(AnularProduccion req)` | `POST Produccion/Anular` |

DTOs a mano en `DTOs/Produccion/ProduccionDTOs.cs` — espejo de los del API
(§3.3 doc API): `ProductoTerminado`, `FormulaComponente`, `GuardarComponenteFormula`,
`CalculoProduccionRequest`, `ConsumoInsumo`, `LoteConsumo`, `CalculoProduccion`,
`DetalleInsumoCalculo`, `ProduccionReporte` (incluye `Anulada`, `FechaAnulacion`,
`UsuarioAnulacion`, `MotivoAnulacion`), `ProduccionLineaInsumo`, `BitacoraFiltro`,
`AnularProduccion { long IdProduccion; string Motivo }`.

Registrar en `Program.cs`:
`builder.Services.AddHttpClient<IProduccionApiCliente, ProduccionApiCliente>(...)`
con la misma config de base URL + `ApiAuthHeaderHandler` que `ILotesApiCliente`.

### 3.2 Pantalla nueva — `Views/Inicio/CalculadoraProduccion.razor`

- `@page "/initial/production-calculator"`
- `@attribute [Authorize]`
- Crear carpeta `Views/Inicio/` (primera de esa carpeta; ok).
- `<AppPantalla Pantalla="Calculadora de Producción" Codigo="INICIO.CALCULADORA_PRODUCCION" Icono="bi bi-calculator" Nivel="NivelPantalla.Escritorio">`
- Inyecta `IProduccionApiCliente`, `IServicioDialogos`, `IManejadorRespuestas`,
  `IContextoSesion`.

**Flujo / secciones:**

0. **Bodega** — selector obligatorio en la cabecera de la pantalla (`HxSelect` sobre
   la lista de bodegas; reusar el endpoint de bodegas ya usado por el proxy actual —
   `Bodega/ObtenerBodegas`, o agregar `BodegasAsync` al `IProduccionApiCliente`).
   Guardar en `_bodega`. Todos los listados de lotes y el request usan `_bodega`.
   Si el usuario cambia de bodega con lotes ya seleccionados → limpiar
   `_seleccionLotes` y el `_calculo`.
1. **Selección de PT** — `HxGrid`/tabla con `ProductosTerminadosAsync` (buscador por
   texto). Columnas: código, descripción, "tiene fórmula". Botón "Abrir".
2. **Fórmula del PT** (al abrir):
   - Tabla de componentes (`FormulaAsync`). Cada fila: insumo, cantidad por unidad.
   - "Agregar componente": selector de artículos **filtrado a Materia prima**
     (`TipoArticulo == 2`) + cantidad. Llama `GuardarComponenteAsync`; si el API
     responde error de tipo, mostrarlo (`IManejadorRespuestas`).
   - "Quitar": `GuardarComponenteAsync` con `Activo=false`.
3. **Cantidad a producir** — input numérico (`_cantidadAProducir`). Define el
   `requerido` de cada insumo.
4. **Lotes a consumir** — por cada componente:
   - Selector de lote (los del insumo **en `_bodega`**, no vencidos, con existencia) + cantidad.
   - **No permitir repetir** `IdStockLote` dentro del mismo insumo. Botón "Agregar
     lote" deshabilitado si ese lote ya está en la lista del insumo.
   - **Consumo estricto:** mostrar por insumo `Σ seleccionado` vs
     `requerido = CantidadPorUnidad · _cantidadAProducir`; marca verde **sólo si son
     exactamente iguales**. Sobrante o faltante = rojo con el número.
5. **Calcular disponibles** — `CalcularAsync` (`Convertir=false`). Muestra el detalle
   por insumo (requerido / seleccionado / existencia total) y confirma
   `MaximoAProducir == _cantidadAProducir`. Habilita el paso 6 sólo si la respuesta
   fue exitosa y todos los insumos cuadran exactamente (R3/R4).
6. **Convertir** — pide **número de lote** (text, obligatorio) y **fecha de
   vencimiento** (`HxInputDate`, **obligatoria** — no dejar continuar sin fecha) del
   PT; confirmación `IServicioDialogos.ConfirmarAsync` ("Se descontarán los insumos y
   se creará el lote X del terminado con vencimiento dd/MM/yyyy"). `ConvertirAsync`.
   Al volver OK: mostrar el `ProduccionReporte` (líneas de insumo + costo) y refrescar
   la bitácora.
7. **Bitácora** (misma pantalla, pestaña o sección aparte): tabla `ReportesAsync`
   con filtros fecha desde/hasta + artículo; botón **Exportar** (ver §3.3).
   - Cada fila muestra su estado. Fila **no anulada** → botón **Anular**: pide motivo
     (input obligatorio) + `IServicioDialogos.ConfirmarPeligroAsync`
     ("Se devolverán los insumos y se retirará el terminado del stock"). Llama
     `AnularAsync`. Si el API rechaza (el terminado ya salió) → mostrar el mensaje tal
     cual (`IManejadorRespuestas`).
   - Fila **anulada** → sin botón; mostrar "Anulada — {motivo} ({usuario}, {fecha})"
     en gris/tachado. Al anular OK: refrescar la bitácora.

Estado local sugerido: `_bodegas`, `_bodega` (obligatorio), `_terminados`,
`_ptSeleccionado`, `_formula`, `_seleccionLotes` (`Dictionary<long, List<LoteConsumo>>`
por insumo), `_cantidadAProducir`, `_calculo` (`CalculoProduccion?`), `_loteProducido`,
`_vencimientoProducido` (`DateOnly?`, obligatorio para convertir), `_reporte`
(`ProduccionReporte?`), `_bitacora`, `_error`.

`CalculoProduccionRequest.Bodega = _bodega` en Calcular y Convertir. Deshabilitar
"Calcular" mientras `_bodega` no esté elegida.

Helpers de validación en el componente (espejo de la validación del API para dar
feedback inmediato; el API sigue siendo la autoridad):
`InsumoCuadra(componente)` → `Math.Abs(Σ lotes - componente.CantidadPorUnidad * _cantidadAProducir) < 1e-6`
(igualdad exacta, consumo estricto);
`TodoCuadra()` → `_cantidadAProducir >= 1 && _formula.All(InsumoCuadra)` && sin `IdStockLote` repetido por insumo.

### 3.3 Exportación de la bitácora (R7)

Patrón Toma Física: generar CSV/Excel en el cliente a partir de `_bitacora`
(+ `ReporteAsync` por fila para traer el detalle) y descargar con el helper de
descarga existente (`IJSRuntime` + `data:`/`Blob`, el mismo que usa el reporte de
toma física).

**El export incluye cabeceras y detalle** (dos hojas si es Excel; dos bloques si es
CSV):

- **Cabeceras:** fecha, código PT, descripción, cantidad producida, lote producido,
  vencimiento, bodega, costo total insumos, usuario, **estado** (Activa / Anulada),
  motivo de anulación, usuario/fecha de anulación.
- **Detalle** (una fila por `ProduccionLineaInsumo`, con la `IdProduccion` como
  vínculo): id producción, código insumo, descripción insumo, lote, cantidad
  consumida, costo unitario, costo línea.

### 3.4 Menú + seguridad

`Class/MenuSeePos.cs`, dentro de los hijos de `INICIO`, agregar:

```csharp
new ItemMenu {
    Titulo = "Calculadora de Producción",
    Codigo = "INICIO.CALCULADORA_PRODUCCION",
    Ruta   = "/initial/production-calculator"
},
```

Agregar el `Codigo` en **ambos** seeds:
- `SecuritySystem/Seed/seed-seguridad.json`
- `tests/SuvesaPosSitioAplicacion.Tests/Fixtures/seed-seguridad.json`

(bajo el árbol de `INICIO`, mismo formato que `INICIO.DEVOLUCIONES`).

`FiltroMenuTests.ElMenuRealSeCargoCompleto`: subir el conteo total de nodos de 102 a
**103** (raíz sigue en 12). Correr el test para confirmar el número exacto.

Si el API también expone permiso de seed para el endpoint (como
`CATALOGOS.TIPOS_DE_BONIFICACION`), agregar la función correspondiente en
`SecuritySystem/Seed/seed-seguridad.json` del API — ver doc API.

### 3.5 Quitar la pestaña vieja de `Views/Inventario/Consulta.razor`

- Borrar el `<TabPanel>`/bloque `_pestanaInventario == "formula"` y su markup.
- Borrar campos y métodos listados en §1.1 que queden sin uso (`_formula`,
  `_seleccionLotesFormula`, `AgregarFormula`, `QuitarFormula`, `CargarLotesFormula`,
  `AgregarLoteFormula`, `FormulaCompleta`, `CalcularProduccion`, `ConvertirProduccion`,
  `SolicitudProduccion`, `LoteFormulaSeleccionado`, `_produccionDisponible`,
  `_cantidadConvertir`, `_lotesFormula`, `_bodegaFormula`, etc.).
- `CargarProduccion()`: si sólo servía para setear `_tipoArticulo=3` desde la
  fórmula, revisar — el tipo ahora se administra con el selector de tipo de artículo
  (paso 3 de MEJORA_LOTES_WEB). Probablemente se puede borrar la llamada.
- Quitar del proxy `IProduccionInventario` los métodos que ya nadie use
  (`Calcular`, `GuardarFormula`, `Formula`) — o dejar el proxy sólo si otra pantalla
  lo usa (revisar referencias). El endpoint viejo del API se retira en su commit de
  limpieza (doc API §3.6), coordinar.
- Build + tests verdes; revisar que no quede `@ref`/binding roto.

---

## 4. Orden de implementación

1. `DTOs/Produccion/ProduccionDTOs.cs` + `ApiConexion/ProduccionApiCliente.cs` +
   registro en `Program.cs`. Build verde.
2. Menú + ambos seeds + ajuste de `FiltroMenuTests`. Tests verdes.
3. `Views/Inicio/CalculadoraProduccion.razor` — selección de PT + fórmula (con
   filtro Materia prima) + guardar/quitar componente.
4. Lotes a consumir con validación no-repetir + suma (R3), cantidad a producir,
   "Calcular disponibles" (R4).
5. "Convertir" con lote + vencimiento obligatorio del PT (R5) + confirmación + reporte.
6. Bitácora + exportación cabeceras+detalle (R7) + acción **Anular** por fila.
7. Quitar la pestaña "Fórmula y conversión" de `Views/Inventario/Consulta.razor` y
   limpiar el proxy. Build + 72+ tests verdes.

---

## 5. Decisiones de negocio (todas RESUELTAS — espejo de doc API §6)

1. **RESUELTO — consumo estricto.** El usuario fija la cantidad a producir y
   selecciona lotes que sumen por insumo exactamente `CantidadPorUnidad ·
   cantidadAProducir`. Sin reparto proporcional.
2. **RESUELTO — vencimiento del PT siempre obligatorio** en la UI (no se puede
   convertir sin fecha).
3. **RESUELTO — export cabeceras + detalle** (dos hojas / dos bloques).
4. **RESUELTO — se permite anular desde la bitácora**: revierte todo (devuelve
   insumos, retira el terminado). El API rechaza si el terminado ya salió.
5. **RESUELTO — selector de bodega en la pantalla** (obligatorio). Insumos y
   terminado se mueven en esa bodega; la anulación usa la bodega guardada.
6. **RESUELTO — costeo con `Inventario.Costo`.** El sitio sólo muestra los costos que
   devuelve el API (`CostoUnitario` / `CostoLinea` / `CostoTotalInsumos`); no calcula
   costos localmente.

---

## 6. Bitácora

- _(pendiente: registrar aquí cada commit / paso con su verificación.)_
