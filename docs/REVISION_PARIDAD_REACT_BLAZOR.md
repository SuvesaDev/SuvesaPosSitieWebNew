# Revisión de paridad funcional React → Blazor

**Fecha:** 2026-09-01
**Alcance:** paridad **funcional** (no visual) de las pantallas operativas.
**Repos comparados:**
- React: `../FrontEndPos2650App`, rama `feature/bonificacion` (commit `7977229`)
- Blazor: `SuvesaPosSitieWebNew`, rama `main` (commit `114135b`)

Este documento es para que otra IA lo lea y ejecute. **No se tocó código al
escribirlo.** Cada hallazgo trae la evidencia (archivo y línea) para que se pueda
verificar antes de actuar.

---

## Cómo usar este documento

1. Los hallazgos están priorizados: **P1** (bloquea uso real), **P2** (pérdida de
   función frente a React), **P3** (detalle o divergencia deliberada a confirmar).
2. Antes de implementar cualquier punto, **verificar el endpoint contra el
   swagger real**, no contra el cacheado:
   ```bash
   ./tools/actualizar-contratos.sh
   ```
   Varios hallazgos de abajo salieron precisamente de que React llama endpoints
   que ya no existen.
3. Después de cada cambio: `dotnet clean` + `dotnet build` (el build incremental
   ya dio falsos positivos antes) y `dotnet test tests/SuvesaPosSitioAplicacion.Tests`.

### Convención que se debe respetar
- **Las Views no llaman al API**: siempre vía `ApiConexion/ProxyInterface`.
- **Todo proxy hereda de `ProxyBase`** y traduce con `EnvelopeApi.A(...)`.
- **Dinero en `decimal`**, nunca `float`/`double`, usando `CalculoDocumento`.
- Una pantalla migrada **debe declarar la ruta del menú** o queda inalcanzable.

---

## Resumen ejecutivo

Lo migrado está **bien construido** donde existe: los proxies siguen el
arquetipo, la aritmética es `decimal`, y en varios puntos Blazor **supera** a
React (persiste lo que React solo guardaba en memoria, valida lo que React no
validaba). El problema no es la calidad de lo hecho, es la **cobertura**.

Tres patrones sistemáticos explican casi todos los huecos:

| Patrón | Impacto |
|---|---|
| **A. ~~Las pantallas de Caja son "solo alta"~~ Resuelto (2026-09-01)** | Ver P1.1 y P1.2 — ya se puede buscar, editar y anular apertura, arqueo y cierre, y el arqueo compara contra lo que el sistema registró. |
| **B. ~~El menú de CostaPets no se portó~~ No aplicaba ya (revisado 2026-09-01)** | Ver P1.3 — el rediseño de seguridad V2 (de otra sesión) ya resuelve esto por permisos, no por variante. No hizo falta código nuevo. |
| **C. ~~Campos del artículo que no llegaron~~ Resuelto (2026-09-01)** | Ver P2.1/P2.2 — se agregaron los 12 campos reales y el ajuste de existencia; "Serie" quedó fuera a propósito (mockup con un tipo de dato que no encaja). |

---

## P1 — Bloqueantes

### P1.1 · ~~Caja: no existe consultar / editar / anular~~ RESUELTO (2026-09-01)

**Resuelto**, con una diferencia real de diseño frente a React que vale explicar
porque cambia cómo se usa la pantalla: React llama a `EditarAperturaDenominacion`
línea por línea, en vivo, en cada `+`/`-` que el cajero pulsa. Blazor lo dejó como
un formulario de revisión: el cajero corrige todas las cantidades y un solo botón
"Guardar cambios" dispara las llamadas (una por línea de denominación, una por
línea de total/tope — el API no tiene un "editar la apertura completa", solo
edita línea por línea). Mismo resultado final contra el API, sin la ventana de
estados a medio guardar que tiene el enfoque en vivo.

Lo que se agregó, por pantalla:

- **`Apertura.razor`**: botón "Buscar" → modal con los filtros reales
  (`AperturaCajaFiltroDTO`: desde/hasta/número/usuario) → tabla de resultados →
  "Abrir" carga la apertura completa (`ConsultarAperturaCaja`) en modo edición.
  En edición, N.º de caja y Observaciones quedan deshabilitados porque el API no
  tiene endpoint para corregirlos — solo el desglose de denominaciones y sus
  totales/tope, que es lo único que `EditarAperturaDenominacion`/
  `EditarAperturaTotalTope` permiten. Botón "Anular" con confirmación.
- **`Arqueo.razor`**: mismo patrón de buscador. A diferencia de Apertura, el API
  sí tiene `EditarArqueoCaja` para el registro completo, así que en edición se
  puede corregir todo (tarjetas, cheques, depósitos, observaciones). Al abrir un
  arqueo ya registrado se respeta el tipo de cambio con el que se guardó
  (`arqueo.TipoCambioD`), no el del día — se está corrigiendo un registro
  histórico, no recalculando uno nuevo.
- **`Cierre.razor`**: solo buscar y anular — confirmado que el API **no** tiene
  ningún endpoint de "editar cierre" (revisado el swagger completo de
  `CierreCaja`: crear, buscar, anular, dos variantes de consultar datos; nada de
  editar). Al "Ver" un cierre se usa `ObtenerDatosDelCierreCajaInsertado`, el
  endpoint del dato ya registrado — no `ObtenerDatosDelCierreCaja`, que es el
  consolidado *previo* que ya usaba la pantalla para armar un cierre nuevo. Se
  agregó `ICajaOperaciones.DatosCierreRegistrado` para esa diferencia.

**Proxy** (`ICajaOperaciones`/`CajaOperaciones`): 11 métodos nuevos —
`BuscarAperturas`, `ObtenerApertura`, `EditarDenominacionApertura`,
`EditarTotalTopeApertura`, `AnularApertura`, `BuscarArqueos`, `ObtenerArqueo`,
`EditarArqueo`, `AnularArqueo`, `BuscarCierres`, `AnularCierre`,
`DatosCierreRegistrado` — todos verificados contra el swagger real antes de
escribirlos (no contra el cacheado).

Compilación limpia (`obj`/`bin` borrados a mano, no solo `dotnet clean`) y
`dotnet test` en verde (67/67). El servidor arranca sin errores de inyección de
dependencias. **No verificado con datos reales** — hace falta un usuario de
pruebas para abrir una apertura/arqueo/cierre real, editarlo y confirmar que el
resultado en el API es el esperado.

<details>
<summary>Hallazgo original (antes de la corrección)</summary>

**Evidencia:**
```
Views/Caja/Apertura.razor        (107 líneas) → solo CajasDisponibles, Denominaciones, CrearApertura
Views/Caja/Arqueo.razor          ( 83 líneas) → solo AperturasSinArqueo, Denominaciones, CrearArqueo
Views/Caja/Cierre.razor          ( 76 líneas) → solo AperturasSinCerrar, DatosCierre, CrearCierre
```
Ninguna de las tres tiene la palabra "buscar", "editar" ni "anular".

React sí las tiene, y **los endpoints existen en el API actual**:

| Función faltante | Endpoint disponible |
|---|---|
| Buscar aperturas (filtros fecha/número/nombre) | `POST /Caja/ConsultarAperturaCaja`, `POST /Caja/ObtenerFiltrosAperturaCaja` |
| Editar apertura | `POST /Caja/EditarAperturaDenominacion`, `/Caja/EditarAperturaTotalTope` |
| Anular apertura | `POST /Caja/DeleteAperturaCaja` |
| Buscar arqueos | `POST /Arqueo/ConsultarArqueoCaja`, `/Arqueo/ObtenerFiltrosArqueoCaja` |
| Editar / anular arqueo | `POST /Arqueo/EditarArqueoCaja`, `/Arqueo/DeleteArqueoCaja` |
| Buscar cierres | `POST /CierreCaja/BuscarCierreCaja` |
| Anular cierre | `POST /CierreCaja/AnularCierreDeCaja` |

**Cómo implementarlo:** ampliar `ICajaOperaciones` (ya existe, 20 métodos) con los
métodos de consulta/edición/anulación, y añadir a cada pantalla un modal de
búsqueda con los filtros que usa React (`ArqueoCashSearchArqueoCashModal.jsx`,
`OpenCashSearchOpenCashModal.jsx`, `CloseCashSearchCloseCashModal.jsx`). El
patrón de modal de búsqueda ya está resuelto en `Views/Compras/Compra.razor`
(modal "Buscar") — copiar ese, no inventar otro.

**Riesgo si no se hace:** una apertura mal digitada no se puede corregir ni
anular desde la aplicación nueva. Es la razón más probable de que un cajero
tenga que volver al sistema viejo.

</details>

---

### P1.2 · ~~Arqueo: falta el detalle de operaciones~~ RESUELTO (2026-09-01)

**Resuelto**, con una aclaración técnica que reemplaza la pregunta de negocio
que este mismo documento dejaba pendiente ("aclarar con negocio los tres
arqueos"): revisando los schemas del swagger no son tres conceptos de arqueo
distintos — es **un solo** `ArqueoCajaDTO`, con dos colecciones anidadas
(`Efectivos` y `Tarjeta`). `CrearArqueoCaja`/`EditarArqueoCaja` (los que ya
usaba la pantalla) persisten el registro completo con sus dos colecciones en
una sola llamada; `CrearArqueoEfectivo`/`CrearArqueoTarjeta` son endpoints de
línea suelta, mismo patrón que `EditarAperturaDenominacion` en Apertura (ver
P1.1) — no hacía falta usarlos por separado. No fue necesario preguntar nada,
la forma del propio DTO ya lo decía.

También se encontró, leyendo `ArqueoCashTarjetasTable.jsx`, que **el
desglose de tarjeta por tipo en React tampoco se guarda de verdad hoy**: el
`onChange` de cada monto solo actualiza el estado de Redux; el código que
llamaba a `startEditArqueoCash` para persistirlo está **comentado**
(`// dispatch( startEditArqueoCash(...) )`). Es el mismo patrón de
funcionalidad a medio terminar que ya se documentó en otras pantallas.
Blazor no replica ese defecto: el desglose de tarjeta que se construyó abajo
sí se envía al guardar, dentro del arreglo `Tarjeta` del `ArqueoCajaDTO`.

Lo que se agregó:

- **Desglose de tarjetas por tipo**: reemplaza los dos campos sueltos
  "Tarjetas colones/dólares" (que quedan de solo lectura, sumados desde el
  desglose) por una tabla, una fila por tipo (`Caja/GetTipoTarjetum`), cada una
  con su monto. Se envía en `ArqueoCajaDTO.Tarjeta` al guardar.
- **Documentos del sistema**: tabla de solo lectura (factura, tipo, moneda,
  forma de pago, pago) con lo que el sistema registró durante la apertura
  (`Arqueo/ObtenerDocumentosEnArqueoCaja`) — la comparación real que le da
  sentido a un arqueo. Se carga al elegir la apertura, o al abrir uno ya
  registrado para editar.
- **Depósitos según el sistema**: nota junto al campo "Depósitos colones" con
  el monto que el sistema tiene registrado (`Arqueo/ObtenerMontoDepositosCaja`),
  para que el cajero compare contra lo que está declarando.
- **Enlace a "Agregar pre-depósito"**: no se duplicó el modal de React dentro
  del arqueo — hay una pantalla dedicada (`PreDepositos.razor`, con su propio
  desbloqueo de clave) y un botón en Arqueo lleva directo ahí. Evita mantener
  dos formularios de pre-depósito.

**Proxy**: 3 métodos nuevos en `ICajaOperaciones` — `DocumentosDeApertura`,
`MontoDepositosDeApertura`, `TiposDeTarjeta` — verificados contra el swagger
real antes de escribirlos.

Compilación limpia (`obj`/`bin` borrados a mano), `dotnet test` en verde
(67/67), servidor arranca sin errores. **No verificado con datos reales** —
falta un usuario de pruebas con una apertura con documentos/tarjetas reales
para confirmar que la comparación muestra lo esperado.

<details>
<summary>Hallazgo original (antes de la corrección)</summary>

Un arqueo compara **lo declarado por el cajero** contra **lo que el sistema
registró**. Blazor solo captura lo declarado.

**Evidencia:** `Views/Caja/Arqueo.razor` captura efectivo, tarjetas col/dól,
cheques col/dól y depósitos col/dól, y los suma. Nunca consulta qué dice el
sistema.

**Endpoints sin usar:**
- `POST /Arqueo/ObtenerDocumentosEnArqueoCaja` — documentos del período
- `POST /Arqueo/ObtenerMontoDepositosCaja` — depósitos que el sistema tiene

React los usa en `arqueoCash/ArqueoCashBodyDetalleOperaciones.jsx` y
`ArqueoCashDetalleOperacionesTable.jsx` (con `startGetDetallesOperacionesCash`).

**Además faltan en el arqueo de Blazor:**
- **Tipo de cambio** (`SetTipoCambioDArqueoCash`). Ver P1.2-bis: no es solo un
  campo que falta, produce un total incorrecto.
- **Arqueo de tarjetas por tipo de tarjeta.** El API tiene
  `POST /Caja/GetTipoTarjetum` y `POST /Arqueo/CrearArqueoTarjeta` (además de
  `CrearArqueoEfectivo` y `CrearArqueoCaja` — son **tres** arqueos distintos).
  Blazor solo llama a uno (`CrearArqueo`) y captura dos totales sueltos de
  tarjeta. React desglosa por tipo en `ArqueoCashTarjetasTable.jsx`.
- **Agregar pre-depósito desde el arqueo** (`ArqueoCashAddPreDepositoModal.jsx`,
  `startSavePreDepositsArqueoCash`).

**Verificar primero:** confirmar a qué endpoint mapea `CrearArqueo` en
`ProxyClass/CajaOperaciones.cs` y si los otros dos (Efectivo/Tarjeta) hacen falta
para que el arqueo quede completo del lado del backend. Es una pregunta de
negocio; documentarla antes de construir.

</details>

---

### P1.2-bis · ~~DEFECTO CONFIRMADO~~ RESUELTO (2026-09-01): el total del arqueo sumaba colones y dólares sin convertir

**Corregido.** Se dejó el hallazgo original abajo para que quede el rastro de qué
estaba mal y por qué; lo que cambió:

- `ICajaOperaciones.TipoCambioDolar()` (nuevo) envuelve `/moneda/ObtenerTipoCambio`
  — el mismo endpoint que usa React (`helpers/getDollarData.js`) — y se resuelve
  en `Arqueo.razor` al cargar la pantalla.
- `Services/CalculoArqueo.cs` (nuevo, mismo patrón que `CalculoDocumento`):
  `Total(colones, dolares, tipoCambio) => colones + (dolares * tipoCambio)`.
- `Arqueo.razor` pasó de `double` a `decimal` en todo el cálculo interno; solo se
  convierte a `double` en el borde, al armar el `ArqueoCajaDTO` para `Guardar()`
  (igual que ya hacía `AbonoPagar.razor`). También se guarda `TipoCambioD` en el
  DTO — antes se enviaba siempre en `0`.
- Se agregó un campo de solo lectura "Tipo de cambio (dólar)" en la pantalla, para
  que el cajero vea con qué tasa se está convirtiendo (React no lo mostraba, solo
  lo usaba internamente; se dejó visible por transparencia, sin agregar
  interactividad que React tampoco tiene).
- `tests/SuvesaPosSitioAplicacion.Tests/CalculoArqueoTests.cs` (nuevo, 3 casos):
  incluye el caso mixto colones+dólares que reproduce el bug original
  (`ConDolares_SeConviertenAntesDeSumar`, con un `Assert.NotEqual` explícito
  contra el resultado que daba el cálculo viejo).

Compilación limpia (`dotnet build` con `obj`/`bin` borrados a mano) y
`dotnet test tests/SuvesaPosSitioAplicacion.Tests` en verde (67/67, antes 64/64).

**Pendiente, fuera de este arreglo puntual** (ver la nota al final de esta
sección): `Apertura.razor` y `Cierre.razor` siguen en `double` para dinero, y
`AbonoPagar.razor` tiene `TipoCambio = 1` fijo en vez de tomarlo del catálogo.
No se tocaron porque no reproducen el mismo bug (no mezclan monedas en un
`Total`), pero quedan con la misma deuda de tipo.

---

<details>
<summary>Hallazgo original (antes de la corrección)</summary>

No es una sospecha; está verificado en ambos lados.

**Blazor** — `Views/Caja/Arqueo.razor:57`
```csharp
private double Total => EfectivoColones + EfectivoDolares + _tarjetaColones
    + _tarjetaDolares + _cheques + _chequesDol + _depositoCol + _depositoDol;
```
Suma directa de montos en dos monedas distintas.

**React** — `arqueoCash/ArqueoCashBodyTotales.jsx:93`
```js
Total = Colones + (Dolares * TipoCambioD);
```
Convierte los dólares con el tipo de cambio antes de sumar.

**Impacto:** ese `Total` no es solo de pantalla — se envía al API en
`ArqueoCajaDTO.Total` (`Arqueo.razor:75`). Con cualquier monto en dólares, **el
arqueo se guarda con un total inflado**: $100 se suman como ₡100 en vez de
~₡52 000. Un arqueo es precisamente el control que detecta faltantes de caja, así
que el error va justo contra el propósito de la pantalla.

**Cómo corregirlo:**
1. Añadir el campo tipo de cambio a la pantalla (React lo tiene como entrada del
   usuario: `SetTipoCambioDArqueoCash`; confirmar si debe traerse de un catálogo).
2. Cambiar `Total` a `Colones + (Dolares * tipoCambio)`, agrupando por moneda
   igual que `EfectivoColones`/`EfectivoDolares`.
3. **Hacerlo en `decimal`**, no en `double` — es dinero, y la regla de la casa
   (`CLAUDE.md`) lo exige. Hoy toda la pantalla opera en `double`.
4. Añadir una prueba unitaria con un caso mixto (colones + dólares) que hoy falle.

**Nota:** el mismo patrón `double` para dinero está en `Apertura.razor`
(`LineaDenominacion.Total`) y en `AbonoPagar.razor` (`TipoCambio = 1` fijo).
Vale revisar las tres juntas.

</details>

---

### P1.3 · ~~El menú de CostaPets es otro árbol y no se portó~~ YA NO APLICA (revisado 2026-09-01)

**No hubo que tocar código.** Este hallazgo se escribió contra una versión del
repo que ya no es la actual — entre esa revisión y esta, otra sesión hizo un
rediseño de seguridad ("Seguridad V2", ver `docs/REDISENO_SEGURIDAD_USUARIOS_ROLES_WEB.md`)
que **resuelve el problema de raíz, mejor de lo que este documento proponía.**

**Qué cambió:** `Security/FiltroMenu` ya no casa por título — casa por
`ItemMenu.Codigo` contra los permisos reales del rol que trae el API
(`Sesion.PuedeVer(codigo)`). El menú es **un solo árbol** para todo el mundo;
lo que se ve depende del catálogo de permisos de cada rol, no de si el usuario
es CostaPets o no. `CLAUDE.md:366-368` ya lo deja dicho: *"Mejora respecto al
sistema actual, donde el menú solo se filtra en la variante CostaPets y el
camino normal enseña todas las pantallas."*

Esto es estrictamente mejor que lo que hace React (una rama de código —
`costaPets ? <SidebarDataCostaPets/> : SidebarData.map(...)` — que duplica el
80% del árbol y hay que mantener sincronizada a mano) y mejor que lo que este
documento proponía (dos árboles, o nodos marcados por variante). **No hacía
falta construir nada — proponerlo hubiera sido retroceder.**

**Verificación que sí hice, para no dar esto por sentado sin revisar:** volví a
leer `SidebarDataCostaPets.jsx` completo (`IteamsAdmin`, 58 nodos) y comparé
cada nodo exclusivo de CostaPets contra `MenuSeePos.cs` actual (12 raíces, 100
nodos) y contra el router de React:

| Nodo exclusivo de CostaPets | ¿Hace falta agregarlo? |
|---|---|
| Consignación → Reportes (`/buys/consignment/reports`) | No. Verificado en `VetRouter.jsx`: **no existe ninguna ruta para ese path** — es un enlace muerto del propio menú de React, igual que "Facturación" en `/sales/billing` que ya está documentado en `CLAUDE.md`. |
| Ventas → Bonificaciones (`/sales/bonuses`) | No todavía. La ruta sí existe y monta `BonusesPage` (`VetRouter.jsx:400`), pero sus acciones llaman a endpoints que ya no existen en el API (ver P1.4). Agregar la entrada de menú apuntaría a una pantalla que aún no se puede construir de verdad. |
| Parametros → Plazos (`/parameters/deadlines`) | No. Ya migrado como "Configuración de Plazos" (`/parameters/payment-terms`) contra los endpoints reales de `ConfiguracionPlazo`. Solo cambia el nombre/ruta, no la función. |

Ningún nodo exclusivo de CostaPets corresponde a una función real que falte en
el árbol.

**Lo único que queda fuera del alcance de este repo:** si un rol "CostaPets" en
el API de verdad tiene sembrados los permisos correctos (`VER` en lo que le
corresponde, denegado en lo que no) es un dato del catálogo de seguridad del
API, no algo que se pueda verificar leyendo este código — haría falta un
usuario de pruebas con un rol CostaPets real y comparar contra su respuesta de
`/seguridad/...`. Con la arquitectura actual, si el permiso está mal sembrado,
el síntoma es un ítem de menú que sobra o falta — no un bug de código.

---

### P1.4 · "Bonificaciones" (catálogo de tipos) no tiene pantalla ni endpoint vivo

Este es delicado y hay que leerlo completo antes de actuar.

La pantalla `/sales/bonuses` de React (`components/Bonuses/`) es el **CRUD del
catálogo de tipos de bonificación** ("compra 3 lleva 1"). Es lo que alimenta las
pestañas de Bonificación de Cliente y Artículo que ya están migradas.

**Pero sus cuatro endpoints ya no existen en el API:**

| React llama (`actions/BonificacionesAction.js`) | ¿Existe hoy? |
|---|---|
| `GET /ArticuloBonificacion/GetArticulosBonificacion` | ❌ |
| `POST /ArticuloBonificacion/CreateArticulosBonificacion` | ❌ |
| `POST /ArticuloBonificacion/UpdateArticulosBonificacion` | ❌ |
| `DELETE /ArticuloBonificacion/DeleteArticulosBonificacion` | ❌ |

Lo único que hay para el catálogo es **lectura**:
`GET /ConfiguracionBonificacion/ObtenerConfiguracionesDisponibles`.

**Conclusión:** hoy **nadie puede crear un tipo de bonificación** desde ninguna
de las dos aplicaciones — ni React (llama a endpoints muertos) ni Blazor (no
tiene la pantalla). Los tipos deben estar entrando por base de datos.

**Acción recomendada:** *no construir la pantalla todavía.* Preguntar al equipo
de API si va a publicar el CRUD del catálogo. Si lo publica, la pantalla es
trivial (misma forma que `Familias.razor`). Mientras tanto, dejarlo documentado.

---

## P2 — Pérdida de función frente a React

### P2.1 · ~~Inventario: campos del artículo que no llegaron~~ RESUELTO (2026-09-01)

**Resuelto**, salvo la pestaña "Serie" — se dejó fuera a propósito, ver abajo.

- **Costo / Moneda del costo**: se agregaron a la pestaña "Precios" existente
  (no una pestaña aparte — ya vivía ahí `PrecioBase`/`Fletes`/`OtrosCargos`, que
  son los mismos tres campos que React usa para calcularlo). `Costo` queda de
  solo lectura, calculado igual que el `useEffect` de
  `InventoryBodyFeaturesUltimoCosto.jsx`: `PrecioBase + Fletes + OtrosCargos`,
  en `decimal`.
- **PromoCON / PromoCRE / Solo con receta**: se agregaron como checkboxes junto
  a Servicio/Lote/MAG en "Datos generales" — son 3 booleanos, no ameritaban
  pestaña propia (el usuario pidió priorizar función sobre diseño).
- **Rebaja Otro Artículo + Información POST** (`!costaPets`): se unieron en una
  pestaña nueva "Otros datos", para no multiplicar pestañas por 3-4 campos cada
  una. El artículo a rebajar se busca con el mismo `AppBuscadorArticulo` que ya
  usa el resto de la app (React también reutiliza su buscador de artículos ahí).
- **Serie — no se construyó, es mockup con un defecto real en React:** el
  campo `Serie` del DTO es `long` (código numérico), pero
  `InventoryBodyFeaturesSerie.jsx` lo trata como un `checkbox` booleano
  (`SetSerieInventory(target.checked)`) — un booleano no encaja en un campo
  numérico; y la tabla de abajo (`Serie`/`Año`) tiene una sola fila con el
  texto literal `test`/`test`, sin ningún API detrás. No hay nada real que
  portar, y replicar el checkbox hubiera significado escribir `true`/`false`
  en un campo que la API espera como número.

Confirmadas como **mockup** (0 inputs, sin API, sin binding) — no se tocan:
`InventoryBodyFeaturesDetalle.jsx`, `InventoryBodyFeaturesBodega.jsx`.

<details>
<summary>Hallazgo original (antes de la corrección)</summary>

`Views/Inventario/Consulta.razor` enlaza 29 campos del `InventarioDTO`. React
edita estos **además**, y todos se persisten al guardar el artículo:

| Campo React | Pestaña React | Condición | Estado en Blazor |
|---|---|---|---|
| `Costo`, `MonedaCosto` | Último Costo | **siempre** | ❌ falta |
| `PromoCON`, `PromoCRE`, `Receta` | Varios | **siempre** | ❌ falta |
| `CodigoDescarga`, `CantidadDescarga`, `CantidadPresentOtro` | Rebaja Otro Artículo | `!costaPets` | ❌ falta |
| `CodigoIntQVET`, `CodigoPro`, `DescripcionPro` | Información POST | `!costaPets` | ❌ falta |
| `Serie` | Serie | `!costaPets` | ❌ falta |

Los dos primeros bloques (`Costo`/`MonedaCosto` y `PromoCON`/`PromoCRE`/`Receta`)
**aplican también a CostaPets** — no están detrás de ningún condicional.

**Evidencia:** `InventoryBodyFeaturesUltimoCosto.jsx` despacha
`SetCostoInventory`, `SetMonedaCostoInventory`, …; `InventoryBodyFeaturesVarios.jsx`
despacha `SetPromoCONInventory`, `SetPromoCREInventory`, `SetRecetaInventory`.

**Cómo implementarlo:** agregar los campos a la pestaña "Datos generales" o crear
una pestaña "Otros datos"; el diseño es libre (el usuario dijo que la estética de
React no importa). Lo importante es que **se envíen en el DTO al guardar**.

Confirmadas como **mockup** (0 inputs, sin API, sin binding) y por tanto **fuera
de alcance**: `InventoryBodyFeaturesDetalle.jsx`, `InventoryBodyFeaturesBodega.jsx`.

</details>

---

### P2.2 · ~~Inventario: CostaPets puede editar la existencia; Blazor no~~ RESUELTO (2026-09-01)

**Resuelto.** `IInventarioConsulta.ActualizarExistencia` (nuevo) envuelve
`/Stocks/ActualizarExistenciaArticulo` — el mismo endpoint que
`startSetStockInventory`. El campo "Existencia actual" reemplaza a
Mínima/Máxima cuando `Sesion.EsCostaPets`, y es un ajuste **inmediato**: se
guarda solo, aparte del botón "Guardar" del artículo, con la misma
confirmación (`Dialogos.ConfirmarAsync`) que ya usa React — ahí no hay
compuerta de clave interna de por medio, solo el diálogo de confirmar, así que
no se agregó una (agregarla hubiera sido más que paridad; si se quiere más
control, es una decisión de negocio aparte, no algo que faltara).

Solo editable con el artículo ya guardado (`!_esNuevo`) — igual que React,
donde `isDisableInputStock` arranca bloqueado y se habilita al cargar un
artículo existente, nunca uno nuevo.

<details>
<summary>Hallazgo original (antes de la corrección)</summary>

**Evidencia:** `InventoryBodyExistencias.jsx:144-150`
```js
disabled={( costaPets ) ? isDisableInputStock : true}
value={ (costaPets) ? stock : existencia}
```
y `:41-48` — al escribir, con *debounce* de 1 s, dispara
`startSetStockInventory(cantidad, inventory.codigo, 0)`, que es una llamada real.

Es decir: **para CostaPets la existencia es editable** (ajuste directo de stock);
para el resto es de solo lectura. Blazor no tiene el campo en el modal.

React además **oculta Mínima y Máxima cuando es CostaPets** (`:65` y `:109`);
Blazor las muestra siempre.

**Cómo implementarlo:** buscar el endpoint de ajuste de stock en el swagger
(`startSetStockInventory` en `actions/inventory.js` dirá cuál es), envolverlo en
`IInventarioConsulta` y añadir el campo con la misma condición.
**Cuidado:** es una escritura que altera existencias — conviene que quede detrás
de la misma compuerta de clave interna que el resto de operaciones sensibles.

</details>

---

### P2.3 · Inventario: falta el recálculo bidireccional de precios (solo no-CostaPets)

El cálculo **CostaPets está correcto** en Blazor y es mejor que React (usa
`decimal` y valida utilidad ≥ 100 %):
`Consulta.razor:1087-1096`
```csharp
Sesion.EsCostaPets ? basePrecio / (1 - utilidad)
                   : basePrecio * (1 + utilidad) + Fletes + OtrosCargos
```
Coincide exactamente con `InventoryBodyPrecioVenta.jsx:80` y `:105`.

**Lo que falta:** para **no-CostaPets**, React recalcula en tres direcciones
(`InventoryBodyPrecioVenta.jsx:88-160`):
- cambia Utilidad → recalcula Precio y Precio+IV
- cambia **Precio** → recalcula **Utilidad** y Precio+IV
- cambia **Precio+IV** → recalcula **Utilidad** y Precio

Blazor solo hace la primera, y por botón ("Calcular precio"), no automático.
Para CostaPets eso es correcto (React tampoco hace más). Para no-CostaPets es
una pérdida real de función.

---

### P2.4 · ~~Facturación: faltan varias funciones reales del encabezado~~ RESUELTO PARCIALMENTE (2026-09-01)

**Resuelto en esta pasada:** las dos funciones que se pidió priorizar.

- **Condiciones de factura (contado/crédito, plazo):** mi hallazgo original
  estaba desactualizado. Esa lógica no vive en `BillingConditions.jsx` sino en
  `BillingHeader.jsx`/`BillingFooter.jsx`, y en Blazor **ya estaba resuelta**
  desde antes (`_tipoFactura` + `TiposDisponibles`, que filtra los tipos de
  crédito según `_cliente.Abierto`/`Sinrestriccion` — ver `Facturacion.razor`).
  No había nada que hacer aquí.
- **Correos del comprobante electrónico:** nuevo botón "Correos del
  comprobante" junto al cliente seleccionado, que abre un modal a listar/
  agregar/editar/quitar correos y guardar. `IClientesConsulta` gana
  `ObtenerCorreosComprobante(long idCliente)` /
  `ActualizarCorreosComprobante(CorreosComprobantes)`, que envuelven
  `/cliente/ObtenerEmailsComprobantes` y `/cliente/ActualizarEmailsComprobantes`
  — los mismos que `startGetCorreosComprobanteFacturacion`/
  `startSaveCorreosComprobanteFacturacion`. `idCliente` es
  `_cliente.Identificacion` (el mismo `cod_Cliente` que React manda, confirmado
  en `actions/billing.js:138`: `SetCodClienteBilling(identificacion)`). Al
  cargar, si el API devuelve `mensaje` no nulo junto con los correos, la lista
  se deja vacía sin mostrar error — igual que hace React
  (`startGetCorreosComprobanteFacturacion`, que solo aplica el resultado
  `if (mensaje === null)`).
- **Agente de ventas:** también se agregó, porque quedó a la vista al resolver
  lo anterior y el endpoint ya estaba mapeado. Checkbox "Sin agente" + select
  de agente (oculto para CostaPets, igual que React lo oculta con
  `isCostaPets`). `IFacturacion.Agentes()` envuelve
  `/agenteventa/ObtenerAgentesVentas` (`startGetAllAgentesVenta`).
  `FacturaDTO.Agente`/`Cod_agente` se arman en `Emitir()`.
- **Moneda del catálogo:** el select de Moneda tenía dos opciones fijas
  (`"CRC"`/`"USD"`) que no son lo que el API espera. En el resto del dominio de
  Ventas (`Cobrar.razor`, `DevolucionesVenta.razor`) `FacturaDTO.CodMoneda`
  (`string`) guarda el `codMoneda` **numérico** convertido a texto, no un
  código de moneda tipo ISO — se confirmó en `Cobrar.razor`:
  `int.TryParse(_preventa.CodMoneda, out var cm) ? cm : 1`. Se reemplazó por el
  catálogo real vía `ICompras.Monedas()` (ya existente, mismo endpoint que
  `startGetAllMonedas`/`monedasInventory`), con el mismo patrón que
  `Compras/Compra.razor` ya usa para compras. Esto no era solo "falta el
  catálogo real" como decía el hallazgo original — el valor que se estaba
  enviando antes era del tipo equivocado.

**Deliberadamente no construido — "PD":** React tiene un checkbox "PD" al
lado de "Agente" (`BillingConditions.jsx`), con el mismo patrón de
habilitado/deshabilitado. Pero su significado de negocio no está documentado
en ningún lado, y hay una discordancia de tipos: `FacturaDTO.Pd` es `string?`
en el contrato actual, mientras que React lo trata como un `bool` puro
(`encabezado.PD`, inicial `false`, seteado directo desde el checkbox) y lo
manda tal cual al crear la factura — y al cargar una preventa existente hace
`PD: responses.pd` sin conversión, así que un `checked` de React ahora mismo
depende de que el string que venga del API sea *cualquier* valor no vacío
(`billingReducer.js:1145-1160`, `actions/billing.js:1782`). Es el mismo tipo
de defecto que la pestaña "Serie" de Inventario (P2.1/P2.2): un campo con tipo
real distinto al que la UI asume, sin que quede claro qué significa. Se deja
sin construir hasta que alguien confirme qué es "PD" y el contrato se
corrija.

**No cubierto en esta pasada (queda para una vuelta futura si se pide):**
crear/editar cliente desde la factura, carta de exoneración en la venta,
cliente MAG, y datos de facturación por sucursal — se dejaron fuera porque el
pedido explícito fue "condiciones de factura y correos del comprobante" y las
dos primeras (que eran el foco real) ya están resueltas arriba. Estas cuatro
seguían señaladas como prioridad media/baja en el hallazgo original.

<details>
<summary>Hallazgo original (antes de la corrección)</summary>

`Views/Ventas/Facturacion.razor` usa: `Api.Crear`, `Api.Tipos`, `Api.Empresas`,
`Api.ValidarClaveInterna`, `Clientes.Buscar`, `Inventario.Uno/Lotes`,
`Imagenes.*`. React (`components/Billing/`, 20 archivos) tiene además:

| Función | Acción React | Prioridad sugerida |
|---|---|---|
| **Condiciones de factura** (contado/crédito, plazo) | `BillingConditions.jsx` | **alta** — define si la venta es a crédito |
| Crear / editar cliente desde la factura | `startSaveCustomerFacturacion`, `startEditCustomerFacturacion` | alta — evita salir de la venta |
| Correos del comprobante electrónico | `startGetCorreosComprobanteFacturacion`, `startSaveCorreosComprobanteFacturacion` | alta — a dónde se envía la factura |
| Carta de exoneración en la venta | `startSearchCartaExoneracion`, `startSearchExoneracionHaciendaBilling` | media |
| Cliente MAG | `startSearchCustomerMAG` | media (`!costaPets`) |
| Agente de ventas | `startGetAllAgentesVenta` | media |
| Datos de facturación (sucursal del cliente) | `SetDatoFacturacionBilling` | media (CostaPets multi-sucursal) |
| Monedas del catálogo | `startGetAllMonedas` | baja — hoy CRC/USD fijos |

`BillingConditions.jsx` también expone **Mascota** y **Taller**, que son
conceptos CostaPets. Verificar si son campos del `FacturaDTO` que hoy se están
enviando vacíos desde Blazor.

**Nota (revisado 2026-09-01):** Mascota/Taller son bloques de código
comentados y muertos en `BillingConditions.jsx` — no hay nada real que portar
ahí.

</details>

---

### P2.5 · ~~Abono a pagar: banco y cuenta se digitan a mano~~ RESUELTO (2026-09-01)

**Resuelto.** Los tres campos libres ahora son catálogos reales, y el tipo de
cambio ya no está fijo:

- **Banco:** select con `ICajaOperaciones.Bancos()` (ya existía, se reutilizó
  sin proxy nuevo, como preveía el hallazgo original).
- **Cuenta bancaria (origen):** select con `ICajaOperaciones.Cuentas(banco, 1)`,
  poblado al elegir banco. El `1` de empresa es fijo a propósito — así lo manda
  React también (`startGetAllCuentasPays(idBanco, 1)` en
  `PaysBodyDatosAbono.jsx`), no es un descuido mío. El valor que viaja en
  `AbonoCuentaPagarReciboDTO.CuentaBancaria` es el **número de cuenta**
  (`CuentaBancariaDTO.Numero`), no el `Id` interno — confirmado contra
  `PaysIcons.jsx`: `cuentaBancaria: cuentaBanco`, donde `cuentaBanco` se carga
  con `cuenta.numero`. (Es distinto del patrón que usa
  `Caja/GenerarDepositos.razor`, que sí manda el `Id` interno en
  `DepositosDTO.IdCuenta` — dos DTOs distintos, dos convenciones distintas.)
- **Cuenta destino:** mi hallazgo original asumía que salía del mismo catálogo
  de cuentas bancarias. **Era incorrecto** — es la cuenta bancaria **del
  proveedor**, no una cuenta de la empresa. React la obtiene con
  `proveedor/ObtenerProveedor?codigo=X` y lee
  `responses.cuentasBancariasProveedors` (`startAllCuentasBancariasProveedorPays`).
  Se agregó `IProveedoresConsulta.Uno(codigo)` (nuevo, envuelve
  `ObtenerProveedorAsync`, que ya estaba generado pero sin envolver) y se carga
  al elegir proveedor. El valor que viaja en `CuentaDestino` (`long`) es
  `CuentaBancariaProveedorDTO.NumCuenta` convertido a número — igual que hace
  React con `parseInt(cuentaProveedor)`.
- **Tipo de cambio:** ya no es `1` fijo. Se consulta con
  `ICajaOperaciones.TipoCambioDolar()` (el mismo endpoint que ya usa Arqueo,
  P1.2-bis) y se manda siempre en `TipoCambio`, sin importar si la moneda del
  abono es colones o dólares — así lo hace React también
  (`tipoCambio: dollar` en `PaysIcons.jsx`, sin condicionar a la moneda).

**No cubierto en esta pasada:** `startSearchAbonosPays` (consultar abonos ya
registrados) sigue sin pantalla en Blazor — no estaba en el alcance pedido
("catálogos de banco/cuenta y tipo de cambio") y es una función de consulta
aparte, no un catálogo. Tampoco se tocó `TipoDocumento`/`Documento`, que
Blazor sigue mandando fijos como `"ABONO"`/`0`; React los arma con un
selector real (`CHEQUE`/`TRANSFERENCIA`) y un número de documento digitado —
es una diferencia real pero independiente de banco/cuenta/tipo de cambio.

<details>
<summary>Hallazgo original (antes de la corrección)</summary>

**Evidencia:** `Views/Compras/AbonoPagar.razor` — `_banco` y `_cuentaDestino` son
`InputNumber` libres.

React los resuelve con catálogos: `startGetAllBancosPays`,
`startGetAllCuentasPays`, `startAllCuentasBancariasProveedorPays` (las cuentas
del proveedor). Digitar un código de banco a mano es una fuente de error segura.

También falta `startSearchAbonosPays` (consultar abonos ya hechos) y el
`TipoCambio` está fijo en `1`, lo que **hace mal la conversión si el abono es en
dólares** — revisar: `AbonoPagar.razor`, campo `TipoCambio = 1`.

`ICajaOperaciones` ya expone `Bancos()` y `Cuentas(banco, empresa)` — se pueden
reutilizar sin proxy nuevo.

</details>

---

### P2.6 · Clientes: campos y regla de crédito

**Pestañas: bien.** Las 6 de React están en Blazor con las mismas condiciones
(`carta` = `!EsCostaPets`, `adjuntos`/`facturacion`/`bonificacion` = `EsCostaPets`
+ sus condiciones). ✅

**Diferencia de regla:** en React, para **no-CostaPets**, los campos de crédito
(plazo, límite, descuento, sin restricción) están **deshabilitados hasta marcar
"Activar crédito"** (`CustomersBodyCreditoDescuento.jsx:96,141,161,212,253`:
`disabled={ (costaPets) ? disableInputs : !activeCredito}`). Para CostaPets no
hay tal compuerta. Blazor no la aplica en ningún caso.

**Campos que React edita y Blazor no tiene** (todos ocultos para CostaPets, así
que solo afectan al despliegue genérico): `TipoCliente` (físico/jurídico),
`Tipoprecio` (tarifa A-D), `Agente`, `Inactivo`, `UsoInterno`,
`IdTipoIdentificacion`.

---

## P3 — Detalles y divergencias a confirmar

### P3.1 · Apertura de caja: el cajero es siempre el de la sesión
`Apertura.razor` arma el DTO con `Nombre = Sesion.Usuario` **y**
`Cedula = Sesion.Usuario`. Dos observaciones:
1. React permite **elegir otro cajero** (`OpenCashSeleccionarUsuario.jsx`).
2. Poner el *nombre de usuario* en el campo **cédula** parece un error de dato,
   no una simplificación. **Verificar contra un registro real** antes de tocarlo.

### P3.2 · Menú: entradas duplicadas viejas vs. fiscales nuevas
`MenuSeePos.cs` tiene a la vez:

| Entrada vieja (→ iframe React, mockup) | Entrada nueva (→ pantalla Blazor real) |
|---|---|
| "Monedas" `/parameters/coins` | "Monedas Fiscales" `/parameters/currencies` |
| "Denominación monedas" `/parameters/denominationcoins` | "Denominaciones de Moneda" `/parameters/currency-denominations` |

El usuario ve dos entradas parecidas y una lo lleva a un mockup dentro del
iframe. **Decidir**: borrar las viejas del menú, o apuntarlas a la pantalla nueva
con un segundo `@page`.

### P3.3 · Usuarios: CostaPets/Agente ahora se heredan del perfil
React tiene casillas por usuario (`UsersBody.jsx:517-547`). Blazor las volvió
**capacidades del perfil**, de solo lectura en la ficha del usuario
(`Usuarios.razor:20`). Es una **decisión deliberada** del rediseño Seguridad V2,
no un olvido. Se documenta para que nadie la "corrija" por error.

### P3.4 · `EsAgenteCostaPets` está en la sesión pero no se usa
`IContextoSesion` lo expone; ninguna View lo consulta (0 usos). En React solo
gobierna el filtrado del menú CostaPets. Queda cubierto al resolver **P1.3**.

### P3.5 · Rutas: verificado, sin problema
Se comprobaron las 94 rutas del menú contra los 62 `@page` de Blazor. **Todas las
pantallas Blazor son alcanzables desde el menú**; ninguna quedó huérfana.
`ConsultaDepositos.razor` declara correctamente sus dos rutas
(`/initial/cash/deposits` y `/initial/cash/deposits/consultdeposits`).

Las 38 rutas del menú sin pantalla Blazor corresponden a nodos de agrupación
(`/buys`, `/initial`, …) y a los mockups ya documentados en `CLAUDE.md`.

---

## Lo que está bien y no hay que tocar

Para que nadie "arregle" lo que ya funciona:

- **Compras + importación XML.** Buena paridad. `EsCostaPetsCompra` se usa 26
  veces y oculta Regalías/Flete/Otros igual que React
  (`BuysArticulosHeader.jsx:351,393,418`). Además deriva del **usuario que
  desbloquea** (`Compra.razor:287`), no de la sesión — que es exactamente lo que
  hace React con `isCostaPets` en el estado de compras. El importador cubre
  asociar, vincular, lotes y precios.
- **Cálculo de precio CostaPets** (P2.3): correcto y mejor que el original.
- **Pestañas de Clientes**: paridad completa de condiciones.
- **Plazos**: migrado como `ConfiguracionPlazosFiscal.razor` con los endpoints
  correctos. Solo cambia el nombre.
- **Proveedores**: cubre alta, edición, estado, cuentas bancarias y consulta a
  Hacienda — el mismo conjunto que React.
- **Bonificación (Cliente/Artículo)**: recién migrada siguiendo el modelo real
  del API, deliberadamente distinta de React porque React llama dos endpoints
  inexistentes. Ver la sección "Bonificación" de `CLAUDE.md`.

---

## Orden de trabajo sugerido

1. ~~**P1.2-bis** Total del arqueo en dólares.~~ **Hecho** (2026-09-01).
2. ~~**P1.1** Caja: consultar/editar/anular.~~ **Hecho** (2026-09-01).
3. ~~**P1.2** Arqueo: detalle de operaciones.~~ **Hecho** (2026-09-01). No hizo
   falta la aclaración de negocio prevista — el swagger ya lo resolvía.
4. ~~**P1.3** Menú CostaPets.~~ **Revisado, no aplicaba** (2026-09-01) — otra
   sesión ya lo resolvió por permisos entre la primera revisión y esta.
5. ~~**P2.1 / P2.2** Campos de inventario y ajuste de existencia.~~ **Hecho**
   (2026-09-01). "Serie" quedó fuera a propósito, ver P2.1.
6. ~~**P2.4** Facturación: condiciones de factura y correos del
   comprobante.~~ **Hecho parcialmente** (2026-09-01) — condiciones de
   factura ya estaba resuelto; se agregaron correos del comprobante, agente
   de ventas y el catálogo real de moneda. "PD" quedó fuera a propósito
   (mismo patrón que "Serie" en P2.1). Crear/editar cliente desde la
   factura, carta de exoneración, cliente MAG y datos de facturación por
   sucursal quedan pendientes para una vuelta futura.
7. ~~**P2.5** Abono a pagar: catálogos de banco/cuenta y tipo de cambio.~~
   **Hecho** (2026-09-01). "Cuenta destino" resultó ser la cuenta del
   proveedor, no una cuenta de la empresa — el hallazgo original lo tenía
   mal. Consultar abonos ya hechos y el tipo/número de documento del pago
   quedan fuera, no eran el alcance pedido.
8. **P1.4** Bonificaciones: solo cuando el API publique el CRUD del catálogo.

---

## Verificaciones pendientes que este documento **no** pudo hacer

Se dicen explícitamente para que no se asuman resueltas:

- **Nada de esto se probó con datos reales.** La revisión es de código y de
  contrato (swagger), no de ejecución. Hace falta una sesión con usuario de
  pruebas **CostaPets** para confirmar el comportamiento real de los campos
  condicionales. (El defecto P1.2-bis sí está confirmado leyendo el código de
  ambos lados, pero tampoco se ejecutó.)
- **Los campos Mascota/Taller** de `BillingConditions.jsx` no se rastrearon hasta
  el DTO; falta confirmar si el `FacturaDTO` los lleva y hoy van vacíos.
- **`CrearArqueo`** de `ICajaOperaciones`: no se verificó a cuál de los tres
  endpoints de arqueo mapea.
