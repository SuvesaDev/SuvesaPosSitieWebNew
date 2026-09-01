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
| **A. Las pantallas de Caja son "solo alta"** | No se puede consultar, editar ni anular ninguna apertura, arqueo o cierre ya registrado. Los endpoints existen. |
| **B. El menú de CostaPets no se portó** | Blazor generó el menú desde `SidebarData.jsx` (el genérico). CostaPets usa `SidebarDataCostaPets.jsx`, que es **otro árbol distinto**. |
| **C. Campos del artículo que no llegaron** | ~13 campos del `InventarioDTO` que React edita no tienen control en Blazor; se guardan con el valor que traiga el DTO. |

---

## P1 — Bloqueantes

### P1.1 · Caja: no existe consultar / editar / anular

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

---

### P1.2 · Arqueo: falta el detalle de operaciones (el corazón del arqueo)

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

---

### P1.2-bis · DEFECTO CONFIRMADO: el total del arqueo suma colones y dólares sin convertir

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

---

### P1.3 · El menú de CostaPets es otro árbol y no se portó

**Evidencia:**
- React elige menú en `components/Shared/Sidebar.jsx:21-22`:
  `costaPets ? <SidebarDataCostaPets/> : SidebarData.map(...)`
- `SidebarDataCostaPets.jsx` tiene su propio `IteamsAdmin` (58 nodos únicos) y
  **además filtra por `modulos`/`pantallas` que devuelve el API** (líneas 21-31).
- `CLAUDE.md` documenta que `MenuSeePos.cs` se generó desde `SidebarData.jsx`
  — el genérico.

**Entradas que existen solo en el menú CostaPets y faltan en `MenuSeePos.cs`:**

| Título | Ruta | ¿En MenuSeePos? |
|---|---|---|
| Bonificaciones | `/sales/bonuses` | ❌ no |
| Consignación (grupo) | `/buys/consignment` | ❌ no |
| └ Reportes | `/buys/consignment/reports` | ❌ no |
| Plazos | `/parameters/deadlines` | ❌ no (ver nota) |

*Nota Plazos:* la función **sí está migrada** como `ConfiguracionPlazosFiscal.razor`
en `/parameters/payment-terms`, usando los endpoints correctos
(`/ConfiguracionPlazo/*`). Solo difiere el título y la ruta. **No reconstruir** —
decidir si se renombra la entrada o se añade la ruta vieja como segundo `@page`.

**Cómo implementarlo:** `MenuSeePos.cs` debería exponer **dos árboles** (o uno
con nodos marcados por variante) y `FiltroMenu` elegir según
`IContextoSesion.EsCostaPets`, igual que hace `Sidebar.jsx`. Ojo: la prueba
`FiltroMenuTests.ElMenuRealSeCargoCompleto` fija 9 raíces y 86 nodos — hay que
actualizarla junto con el menú.

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

### P2.1 · Inventario: campos del artículo que no llegaron

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

---

### P2.2 · Inventario: CostaPets puede editar la existencia; Blazor no

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

### P2.4 · Facturación: faltan varias funciones reales del encabezado

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

---

### P2.5 · Abono a pagar: banco y cuenta se digitan a mano

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

1. **P1.2-bis** Total del arqueo en dólares. Es un defecto de dinero ya
   confirmado y de arreglo acotado — va primero.
2. **P1.1** Caja: consultar/editar/anular (endpoints listos, patrón resuelto).
3. **P1.2** Arqueo: detalle de operaciones. *Antes:* aclarar con negocio los tres
   arqueos (Caja/Efectivo/Tarjeta).
4. **P1.3** Menú CostaPets (toca `MenuSeePos.cs` y `FiltroMenuTests`).
5. **P2.1 / P2.2** Campos de inventario y ajuste de existencia.
6. **P2.4** Facturación: empezar por condiciones de factura y correos del
   comprobante.
7. **P2.5** Abono a pagar: catálogos de banco/cuenta y tipo de cambio.
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
