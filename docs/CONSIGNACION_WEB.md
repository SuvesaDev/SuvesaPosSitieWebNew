# Modelo de Consignación — Sitio web (`SuvesaPosSitioAplicacion`)

> Documento de trabajo. Repo: `SuvesaPosSitieWebNew`, rama base `feature/bonificaciones-web`.
> Gemelo del API: `DevSuvesaPosWeb/ApiSuvesaPos/docs/CONSIGNACION_API.md` — leerlo primero.
> Fuente del requerimiento: `Especificacion_Proceso_Consignacion.docx`.
> **Depende de** que el API exponga el módulo nuevo de consignación (ver doc API §3.4).
> **No se toca código con este documento: es análisis + plan.**

---

## 0. Resumen del requerimiento

Ver `CONSIGNACION_API.md` §0. En una frase: cada cliente tiene su **bodega de consignación** (inventario
independiente de la empresa, físicamente en el local del cliente). Se mueve con **boletas de ingreso** (entrega
inicial + reposición), **boletas de salida** (retiro parcial / cierre) y **facturación** (tras un inventario
físico del agente que genera una **prefactura editable** → **factura** que sólo descuenta, nunca repone).
Cuando la bodega llega a 0 → **"Consignación cerrada"**. Todo debe ser trazable (cliente, bodega, fecha,
usuario, documento) y validado (no sacar/facturar más de lo que hay).

---

## 1. Estado actual

### 1.1 Pantallas y rutas

| Función de menú (`Class/MenuSeePos.cs`) | Ruta | ¿Página Blazor? |
|---|---|---|
| `CONSIGNACION.REGISTRO_DE_CONSIGNACIONES` | `/buys/consignment/register` | **No** (cae al SPA React / 404) |
| `CONSIGNACION.FACTURACION_DE_CONSIGNACIONES` | `/buys/consignment/billing` | **No** (idem) |
| `CONSIGNACION.SEGUIMIENTO_DE_CONSIGNACIONES` | `/buys/consignment/following` | **Sí** — `Views/Consignacion/Seguimiento.razor` |

El módulo "Consignación" cuelga de **Compras** en el menú (comentario en `MenuSeePos.cs`: el API lo declara bajo
"Inicio" pero el permiso casa por título de pantalla).

### 1.2 `Views/Consignacion/Seguimiento.razor` (lo único portado)

- `<AppPantalla Pantalla="Seguimiento de Consignaciones" Nivel="NivelPantalla.Tableta">`.
- Gate por clave: `AppDesbloqueoClave` — hay que desbloquear con una clave para operar.
- Filtro **Estado**: "Pendientes de aceptar" / "Aceptadas" → `Api.PorEstado(bool)`.
- Lista de consignaciones (`FacturaDTO`): número, fecha, cliente, total, estado (Aceptada/Pendiente).
- Abrir una → detalle con líneas (`FacturaDetallesDTO`): "Solicitado" y un input "A despachar".
- Si `ConsignacionAceptada == true` y el usuario validado tiene `AceptaConsignacion` → botón **Despachar**
  (`Api.Despachar(ConsignacionAplicacionDTO)`), que arma `IdConsignacion`, `EsParcial`, `Articulos[]`.
- Si está pendiente y el usuario tiene `AceptaConsignacion` → botón **Aprobar** (`Api.Aprobar(id)`).

### 1.3 Proxy — `IConsignaciones` / `ProxyClass/Consignaciones.cs` (`: ProxyBase`)

| Método | Endpoint API |
|---|---|
| `PorEstado(bool)` | `Consignacion/ObtenerConsignacionEstado` (NSwag `IConsignacionApiCliente`) |
| `Buscar(texto)` | `venta/BuscarConsignacion` |
| `Obtener(id)` | `venta/ObtenerConsignacion2` |
| `Aprobar(id)` | `Consignacion/AceptarRechazarConsignacion` |
| `Despachar(ConsignacionAplicacionDTO)` | `Consignacion/GenerarVentaConsignacion` |

Comentarios en el código: `Consignacion/ObtenerConsignacionEncabezadoEstado` **responde 500** contra el API real
(verificado con curl), por eso se usa `venta/ObtenerConsignacionEstado`.

### 1.4 Lo que NO existe en el sitio

- No hay pantalla de **bodega de consignación por cliente** (ni el concepto).
- No hay **boleta de ingreso** ni **boleta de salida**.
- No hay **inventario físico** del agente ni **prefactura editable**.
- No hay **conversión prefactura → factura** como flujo propio (sólo el "Despachar" actual).
- No hay **kardex** ni **reporte** de consignación, ni indicador **"consignación cerrada"**.
- No hay **bonificaciones** en el flujo de consignación.

### 1.5 Patrones reutilizables

- Cliente HTTP a mano + `LoteEnvelope<T>` + `ContextoLlamada.Token`: `ApiConexion/LotesApiCliente.cs`,
  `ApiConexion/ProduccionApiCliente.cs`.
- Pantalla lista + acción → `HxModal` + reporte + export CSV: `Views/Produccion/Calculadora.razor` /
  `Views/Produccion/Bitacora.razor`, `Views/Compras/TomaFisica.razor`.
- Helper de descarga: `window.seepos.descargarTexto` (en `Views/Shared/App.razor`).
- `AppPantalla` (`Pantalla`, `Codigo`, `Icono`, `Subtitulo`, `Nivel`, `Acciones`), `IServicioDialogos`,
  `IManejadorRespuestas`.
- Selección de cliente: ver cómo lo hace Facturación / Clientes (`ICliente...`), reutilizar el buscador.
- Bonificación en facturación (pantalla y proxy ya existentes) — reusar para la prefactura.

---

## 2. Requerimiento → cambio en el sitio

| # | Cambio |
|---|---|
| W1 | **Nuevo submódulo** de consignación con pantallas para: bodegas por cliente, boletas (ingreso/salida), inventario físico, prefactura, facturación, kardex/reporte. |
| W2 | **Bodegas de consignación**: lista de clientes con bodega de consignación, existencia total y estado (Activa / **Cerrada** cuando existencia = 0). Acción "Abrir bodega" para un cliente nuevo. |
| W3 | **Boleta de ingreso**: elegir cliente → agregar artículos (y lote) + cantidad → guardar. Aumenta la bodega. Sirve para la entrega inicial y para reposiciones. |
| W4 | **Boleta de salida**: elegir cliente → agregar artículos a retirar (validado contra existencia) → guardar. Marca "cierre total" si aplica. Disminuye la bodega, **sin factura**. |
| W5 | **Opcional (del doc)**: unir W3+W4 en una sola pantalla **"Ajuste de bodega de consignación"** con pestaña **Entrada / Salida**. |
| W6 | **Inventario físico**: pantalla para el agente/usuario: elegir cliente → tabla con lo consignado por artículo (y lote) → capturar **físico** → el sistema muestra **vendido = consignado − físico**. Guarda el conteo. |
| W7 | **Prefactura**: desde un conteo → generar prefactura; pantalla **editable** (cantidades, precios, descuentos, **bonificaciones** con el mismo componente que facturación) → **Aprobar**. |
| W8 | **Facturar**: desde una prefactura aprobada → elegir **contado/crédito** (y plazo) → generar la **factura** (descuenta la bodega del cliente; nunca repone). Mostrar el resultado. |
| W9 | **Kardex / Reporte** de la bodega de consignación de un cliente: movimientos (ingresos, salidas, facturas) con fecha, usuario, documento; existencias por artículo; indicador **"Consignación cerrada"**; **exportación** CSV. |
| W10 | **Menú**: revisar dónde vive. Hoy `CONSIGNACION.*` cuelga de "Compras". Con el alcance nuevo conviene un módulo propio **"Consignación"** con sub-items (Bodegas, Ajuste, Inventario físico, Prefacturas, Kardex) — o mantener los 3 códigos actuales y repartir. **Coordinar con el seed del API y `FiltroMenuTests`.** |
| — | Migrar `Seguimiento.razor` al flujo nuevo o dejarlo hasta que el API retire los endpoints viejos (§3.4 doc API). |

---

## 3. Diseño propuesto (sitio)

### 3.1 Contratos a mano — `ApiConexion/ConsignacionApiClienteV2.cs`

`IConsignacionApiClienteV2` (patrón `LotesApiCliente` / `ProduccionApiCliente`: `HttpClient` + `IContextoSesion`,
`LoteEnvelope<T>`, setea `ContextoLlamada.Token`). Métodos espejo de `CONSIGNACION_API.md §3.4`:

| Método | Endpoint |
|---|---|
| `AbrirBodegaAsync(AbrirBodega req)` | `POST Consignacion/AbrirBodega` |
| `BodegasAsync(BodegasFiltro req)` | `POST Consignacion/Bodegas` |
| `RegistrarBoletaAsync(BoletaConsignacion req)` | `POST Consignacion/RegistrarBoleta` |
| `BoletaAsync(long id)` | `GET Consignacion/Boleta?id=` |
| `RegistrarConteoAsync(ConteoConsignacion req)` | `POST Consignacion/RegistrarConteo` |
| `ConteoAsync(long id)` | `GET Consignacion/Conteo?id=` |
| `GenerarPrefacturaAsync(GenerarPrefactura req)` | `POST Consignacion/GenerarPrefactura` |
| `EditarPrefacturaAsync(PrefacturaConsignacion req)` | `PUT Consignacion/EditarPrefactura` |
| `AprobarPrefacturaAsync(long id)` | `POST Consignacion/AprobarPrefactura` |
| `FacturarPrefacturaAsync(long id)` | `POST Consignacion/FacturarPrefactura` |
| `PrefacturaAsync(long id)` | `GET Consignacion/Prefactura?id=` |
| `KardexAsync(KardexFiltro req)` | `POST Consignacion/Kardex` |
| `ReporteAsync(ReporteFiltro req)` | `POST Consignacion/Reporte` |
| `AnularBoletaAsync(Anular req)` / `AnularPrefacturaAsync(Anular req)` | `POST Consignacion/AnularBoleta` / `AnularPrefactura` |

DTOs a mano en `DTOs/Consignacion/ConsignacionDTOs.cs` — espejo de los del API (§3.1 y §3.3 doc API):
`BodegaConsignacionResumen` (con `Estado` Activa/Cerrada), `BoletaConsignacion(+Linea)`, `ConteoConsignacion(+Linea)`
(con `Consignado`/`Fisico`/`Vendido`), `PrefacturaConsignacion(+Linea)` (con `MontoBonificacion`),
`MovimientoKardex`, `ReporteConsignacion`, `Anular`.

Registrar el `AddHttpClient<IConsignacionApiClienteV2, ...>` en `Program.cs` (misma config que `ILotesApiCliente`).

### 3.2 Pantallas

Carpeta `Views/Consignacion/`. Todas `@attribute [Authorize]` + `<AppPantalla Codigo="CONSIGNACION.*">`.

1. **`Bodegas.razor`** (`/consignment/warehouses`) — lista de clientes con bodega de consignación:
   código/cédula, nombre, existencia total, **estado** (badge Activa / **Cerrada**), fecha apertura/cierre.
   Filtro texto + "sólo cerradas". Acción **"Abrir bodega"** (buscar cliente → `AbrirBodegaAsync`). Fila → botones
   a Ajuste, Inventario físico, Kardex de ese cliente.

2. **`Ajuste.razor`** (`/consignment/adjust`) — **una pantalla con pestaña Entrada / Salida** (opción del doc, W5):
   - Elegir cliente (buscador).
   - **Entrada**: agregar artículos (buscador) + lote + cantidad → tabla → guardar → `RegistrarBoletaAsync({ Tipo: Ingreso, ... })`.
   - **Salida**: igual, pero el selector de lote muestra existencia en la bodega del cliente y valida
     `cantidad ≤ existencia`; checkbox **"Cierre total"** que exige dejar todo en 0 → `RegistrarBoletaAsync({ Tipo: Salida, CierreTotal, ... })`.
   - Muestra la boleta generada + opción de exportar/imprimir.
   (Si negocio prefiere separado: `Ingreso.razor` + `Salida.razor` con el mismo contenido por pestaña.)

3. **`InventarioFisico.razor`** (`/consignment/count`) — elegir cliente → `ConteoAsync`/carga de lo consignado:
   tabla por artículo (y lote): **Consignado** (solo lectura, existencia de sistema), input **Físico**, columna
   calculada **Vendido = Consignado − Físico** (no negativa; marcar en rojo si Físico > Consignado). Guardar →
   `RegistrarConteoAsync`. Botón **"Generar prefactura"** → §4.

4. **`Prefactura.razor`** (`/consignment/prebill`) — abrir una prefactura (o generarla desde un conteo):
   - Cabecera: cliente, fecha, **condición Contado/Crédito** + plazo.
   - Tabla de líneas **editable**: cantidad (default = Vendido), precio unitario, descuento; totales en vivo.
   - **Bonificaciones**: reutilizar el componente/really flujo de bonificación de facturación (agregar líneas de
     bonificación según config del cliente/artículo). Mostrar `MontoBonificacion`.
   - Botones: **Guardar** (`EditarPrefacturaAsync`), **Aprobar** (`AprobarPrefacturaAsync`), y cuando esté
     aprobada, **Facturar** (`FacturarPrefacturaAsync`) → muestra la factura generada (número, total) y refresca.
   - La factura **sólo descuenta**; dejarlo explícito en la confirmación.

5. **`Kardex.razor`** (`/consignment/ledger`) — elegir cliente + rango de fechas → `KardexAsync`: tabla de
   movimientos (fecha, tipo — Ingreso/Salida/Factura —, documento, artículo, lote, cantidad, existencia
   anterior/nueva, usuario, observaciones). Encabezado con existencia total y badge **"Consignación cerrada"** si
   corresponde. Botón **Exportar CSV**. (Opcional: `Reporte.razor` con el resumen consignado/vendido/retirado y
   export cabecera+detalle, patrón Bitácora de Producción.)

### 3.3 Menú + seguridad

Decidir (W10) con el seed del API:
- **Opción 1 (mínima):** mantener los 3 códigos actuales y repartir: `REGISTRO_DE_CONSIGNACIONES` → Ajuste,
  `FACTURACION_DE_CONSIGNACIONES` → Prefactura/Facturar, `SEGUIMIENTO_DE_CONSIGNACIONES` → Bodegas/Kardex.
- **Opción 2 (recomendada):** módulo propio **"Consignación"** con sub-items `CONSIGNACION.BODEGAS`,
  `CONSIGNACION.AJUSTE`, `CONSIGNACION.INVENTARIO_FISICO`, `CONSIGNACION.PREFACTURAS`, `CONSIGNACION.KARDEX`.
  Requiere: `MenuSeePos.cs` + `tests/.../Fixtures/seed-seguridad.json` + `SecuritySystem/Seed/seed-seguridad.json`
  del API + bump de `FiltroMenuTests.ElMenuRealSeCargoCompleto` + conceder las funciones a los roles
  (Parámetros → Roles, no lo hace el seeder).

El gate por clave (`AppDesbloqueoClave`) y el permiso `AceptaConsignacion` del perfil se conservan para las
acciones sensibles (aprobar prefactura, facturar, cierre total).

### 3.4 Migrar / retirar lo viejo

- `Seguimiento.razor` y el proxy `IConsignaciones` (endpoints `ObtenerConsignacionEstado`,
  `AceptarRechazarConsignacion`, `GenerarVentaConsignacion`) quedan hasta que el API retire esos endpoints
  (doc API §3.4/§5 paso 9). Luego se borran o se reemplaza `Seguimiento` por `Prefactura`/`Bodegas`.
- Revisar referencias a `ConsignacionAplicacionDTO` / `FacturaDTO` en consignación antes de borrar.

---

## 4. Orden de implementación

1. `DTOs/Consignacion/ConsignacionDTOs.cs` + `ApiConexion/ConsignacionApiClienteV2.cs` + registro en `Program.cs`.
   Build verde.
2. Menú + seeds + `FiltroMenuTests` (según la opción elegida en §3.3). Tests verdes.
3. `Bodegas.razor` — lista + estado Cerrada + "Abrir bodega".
4. `Ajuste.razor` — pestañas Entrada / Salida (con validación de existencia y cierre total).
5. `InventarioFisico.razor` — conteo + cálculo Vendido + "Generar prefactura".
6. `Prefactura.razor` — edición + bonificaciones + Aprobar + Facturar.
7. `Kardex.razor` (+ `Reporte.razor` opcional) + exportación CSV.
8. Retiro/migración de `Seguimiento.razor` y el proxy viejo cuando el API lo permita.

Verificación: `dotnet build src/SuvesaPosSitioAplicacion/SuvesaPosSitioAplicacion.csproj` +
`dotnet test tests/SuvesaPosSitioAplicacion.Tests/...`.

---

## 5. Decisiones de negocio pendientes (espejo de doc API §7)

1. Bodega por cliente: ¿`Bodega` física dedicada o atributo? ¿una o varias por cliente?
2. Prefactura: ¿documento propio o `Venta` con flag?
3. Kardex: ¿derivado de `Stocks` o tabla propia?
4. Inventario físico: ¿por artículo o por artículo + lote?
5. `Vendido` cuando `Físico > Consignado`: ¿ignorar / marcar sobrante / forzar 0?
6. Bonificaciones de prefactura: ¿mismo motor y config que facturación? (el doc dice que sí).
7. Precio de la prefactura: ¿tarifa del cliente / lista / siempre editable?
8. Condición contado/crédito y plazo: ¿al generar la prefactura o al facturar?
9. Menú: ¿módulo propio "Consignación" (opción 2) o repartir los 3 códigos actuales (opción 1)?
10. `Seguimiento.razor` y el flujo actual: ¿migrar, cerrar o convivir?
11. "Ajuste de bodega de consignación": ¿una pantalla con pestañas o dos separadas?

---

## 6. Bitácora

- _(pendiente: registrar aquí cada commit / paso con su verificación.)_
