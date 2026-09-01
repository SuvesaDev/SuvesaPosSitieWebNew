# Bonificación — diseño y trabajo pendiente en el sitio Web (Blazor)

**Fecha:** 2026-09-01
**Depende de:** [`BONIFICACION_REQUERIMIENTOS_API.md`](../../DevSuvesaPosWeb/docs/BONIFICACION_REQUERIMIENTOS_API.md)
(repo `DevSuvesaPosWeb`) — casi todo lo de este documento está bloqueado, total o
parcialmente, por decisiones que le corresponden al API (marcado en cada punto).

> Este documento es análisis y plan, no implementación. No se tocó código para
> escribirlo.

## 1. El proceso de negocio esperado

Ver el detalle completo en el documento del API (§1). En resumen: catálogo de
tipos ("compra 10, regalo 1"), asignable a Clientes (solo el tipo) y a
Artículos (el tipo + una lista de artículos con los que se puede mezclar la
cantidad). Al facturar, el tipo elegido por el cliente manda sobre lo que tenga
el artículo; se arma un grupo de líneas (las pagadas + la gratis, que es la de
menor precio entre las usadas); esas líneas no se pueden editar ni borrar
sueltas; el artículo gratis va a precio 0 pero con el impuesto real cobrado.

## 2. Qué existe hoy en Blazor (confirmado, 2026-09-01)

Más de lo que parecía a primera vista — buena parte de la configuración ya está
construida, aunque nada de esto llega a aplicarse en una factura real:

| Pieza | Dónde | Estado |
|---|---|---|
| Catálogo (solo lectura) | `ICatalogoBonificacion.Disponibles()` | Envuelve el único endpoint que existe. Sin pantalla de catálogo propia — no hay dónde crear un tipo nuevo (ver API §3.1). |
| Bonificación por Cliente | [`Views/Clientes/Consulta.razor`](../src/SuvesaPosSitioAplicacion/Views/Clientes/Consulta.razor), pestaña "Bonificación" (gated por `Sesion.EsCostaPets && _edicion.TieneBonificacion`) | CRUD completo y funcionando: elegir tipo + buscar y elegir un artículo de regalo (`AppBuscadorArticulo`), listar/editar/quitar. **Pide un artículo específico al configurar** — no calza con el proceso de negocio (ver §4.1, depende de API §3.2). |
| Bonificación por Artículo | [`Views/Inventario/Consulta.razor`](../src/SuvesaPosSitioAplicacion/Views/Inventario/Consulta.razor), pestaña "Bonificación" (gated por `Sesion.EsCostaPets && _edicion.Bonificado && !_esNuevo`) | Ya tiene: checkbox "Bonificado" que habilita la pestaña, tabla de tipos asignados al artículo (CRUD), y una segunda sección para la mezcla de artículos (`Relacionados.BuscarBonificacion`/`GuardarBonificacion`, con su propio buscador). Estructuralmente calza bien con el ejemplo de Fanta del pedido de negocio. |
| Uso en Facturación | [`Views/Ventas/Facturacion.razor`](../src/SuvesaPosSitioAplicacion/Views/Ventas/Facturacion.razor) | Solo un panel informativo: si el cliente tiene bonificación, se listan sus configuraciones (`_bonificacionesCliente`) con una nota "Informativo. Agregue el producto de regalo manualmente si corresponde." **No aplica nada a la factura.** El propio código lo documenta: el botón "Bonificar" del sistema actual (React) quedó enganchado al handler equivocado (copiado del modal de correos) y nunca llegó a tocar la factura — así que tampoco hay un flujo real de referencia para portar 1:1; hay que construirlo desde la especificación de negocio. |
| Uso en Devoluciones | [`Views/Ventas/DevolucionesVenta.razor`](../src/SuvesaPosSitioAplicacion/Views/Ventas/DevolucionesVenta.razor) | No hace nada especial con bonificación (no hay nada que hacer todavía — el API tampoco expone el dato, ver API §3.6). |
| Cálculo de línea | [`Services/CalculoDocumento.cs`](../src/SuvesaPosSitioAplicacion/Services/CalculoDocumento.cs) | `Linea(cantidad, precioUnitario, %descuento, %impuesto)` calcula el impuesto **sobre el subtotal de la línea** (`cantidad × precioUnitario`). Con `precioUnitario=0` el impuesto también da 0 — no soporta hoy "precio 0 con impuesto real" (ver §4.6). |

## 3. Orden de trabajo sugerido (por dependencia)

El grueso de esto no se puede empezar en serio hasta que el API resuelva sus
puntos 3.1 (CRUD de catálogo), 3.2 (DTO de Cliente↔Bonificación) y 3.4 (alcance
real de `ObtenerDetallesArticulosBonificacion`) — son los que determinan la
forma de los datos y cuánta lógica de negocio hay que construir acá vs. cuánta
ya viene resuelta del servidor. Mientras tanto, se puede dejar este documento
como plan y, si se quiere adelantar algo sin bloquear, empezar por la pantalla
del catálogo (§4.7), que no depende de nada más que del CRUD.

### 4.1 Ajustar la pestaña "Bonificación" de Clientes → Consulta
Si el API cambia `ClienteBonificacionConfiguracionDTO` para que ya no pida un
artículo (API §3.2), esta pestaña se simplifica: en vez de "elegir tipo +
buscar artículo de regalo", pasa a ser solo "elegir tipo(s) que este cliente
puede usar" — una lista de checkboxes o un multi-select sobre
`CatalogoBonificacion.Disponibles()`, sin buscador de artículo. Bloqueado por
API §3.2.

### 4.2 Confirmar en vivo la pestaña de Inventario → Consulta
Ya existe y estructuralmente calza (tipos + mezcla). Antes de dar esto por
resuelto, probar con datos reales el escenario completo del ejemplo (un
artículo con dos configuraciones distintas — "10+2" y "8+1" — más una mezcla de
3 artículos) para confirmar que no hay ningún límite o supuesto oculto en la UI
actual que no se vio en la lectura de código (por ejemplo, que solo permita una
configuración activa a la vez). No depende del API.

### 4.3 El flujo de Facturación — el trabajo grande
En [`Views/Ventas/Facturacion.razor`](../src/SuvesaPosSitioAplicacion/Views/Ventas/Facturacion.razor):

**a. Al elegir cliente** (`ElegirCliente`): si `cliente.TieneBonificacion` y hay
tipos disponibles, preguntar (modal, no solo mostrar la lista como hoy) si se
quiere usar alguno; si el usuario acepta, dejar elegir **cuál tipo** (no
artículo) y guardarlo en un nuevo campo de estado de la factura en curso (p.ej.
`_tipoBonificacionClienteActivo`). Reemplaza el panel puramente informativo
actual.

**b. Al agregar una línea** (`PrepararArticulo`/`AgregarArticulo`): si el
artículo es `Bonificado`:
   - si hay un tipo de cliente activo (paso a), usarlo directamente sin volver
     a preguntar — "la configuración del cliente manda" (regla explícita del
     pedido de negocio).
   - si no hay tipo de cliente activo, y el artículo tiene sus propios tipos
     asignados (`ArticuloBonificacion.ObtenerPorArticulo`), preguntar si se
     quiere usar la bonificación de ese artículo y dejar elegir cuál de sus
     tipos propios (si tiene más de uno, como Fanta con 10+2 y 8+1).

**c. Selector de mezcla:** mostrar los artículos configurados como mezcla para
el artículo elegido (`Relacionados.BuscarBonificacion`, incluyendo el propio
artículo), dejar armar cantidades sin exceder el tope de la configuración
elegida (`CantidadVenta`). Reutilizable como un modal nuevo, en el mismo
espíritu que `AppBuscadorArticulo`.

**d. Resolver cuál sale gratis:** el de menor precio entre los efectivamente
usados. Depende directamente de API §3.4 — si `ObtenerDetallesArticulosBonificacion`
ya lo resuelve, este paso es solo llamar al endpoint y pintar el resultado; si
no, hay que calcularlo acá con los precios ya resueltos de cada artículo
(reutilizando `PrecioCliente`, que ya existe en este mismo archivo).

**e. Agregar las líneas resultantes al detalle:** las pagadas + la gratis,
marcando `EsBonificacion=true` en la línea gratis (y lo que el API termine
pidiendo para el agrupamiento — API §3.3).

**f. Bloquear edición de precio/descuento** en esas líneas — hoy `LineaVenta`
permite editar `Precio`/`Descuento` libremente vía los `<input>` de la tabla de
detalle; hace falta una forma de marcar una línea como "de bonificación,
solo lectura" y condicionar esos inputs.

**g. Solo la línea principal se puede eliminar; borrado en cascada** del grupo
— cambio en `QuitarLinea`, agrupando por el identificador de grupo que se
decida en API §3.3.

### 4.4 Precio 0 + impuesto real
Extender `CalculoDocumento` (nuevo overload de `Linea`, o un método aparte) para
aceptar un **precio de referencia** distinto del precio facturado, de forma que
el impuesto se calcule sobre el precio de lista aunque `PrecioUnit` viaje en 0.
Cambio acotado y aislado en un solo archivo
([`Services/CalculoDocumento.cs`](../src/SuvesaPosSitioAplicacion/Services/CalculoDocumento.cs)),
pero su forma exacta depende de cómo responda el API a la pregunta de negocio
#4 del documento del API (¿el impuesto de la línea gratis se cobra al cliente o
lo asume la empresa?).

### 4.5 Devoluciones
En [`Views/Ventas/DevolucionesVenta.razor`](../src/SuvesaPosSitioAplicacion/Views/Ventas/DevolucionesVenta.razor):
al traer el detalle de una factura para devolver, si alguna línea es de
bonificación, agrupar la selección igual que en Facturación — todo el grupo se
devuelve junto, no se puede devolver solo la pagada o solo la gratis. Bloqueado
por completo hasta que el API exponga el agrupamiento (API §3.6) — hoy no hay
forma de saber, mirando el detalle de una devolución, qué líneas eran
bonificación.

### 4.6 Pantalla del catálogo maestro
Equivalente a `/sales/bonuses` de React. Una vez el API tenga CRUD (API §3.1),
es una pantalla trivial — mismo patrón que [`Views/Parametros/Familias.razor`](../src/SuvesaPosSitioAplicacion/Views/Parametros/Familias.razor)
(tabla + alta/edición simple: descripción, cantidad de venta, cantidad
bonificable). Es lo único de esta lista que no depende de nada más que del
CRUD del API — se puede adelantar en cuanto ese endpoint exista, sin esperar al
resto.

## 4. Preguntas abiertas específicas del lado Web

- ¿Dónde vive la decisión de "tipo de cliente activo" durante la factura en
  curso — un campo más en el estado de `Facturacion.razor` (como hoy
  `_tipoFactura`), o algo que sobreviva si la pantalla se recarga (poco
  probable que haga falta, dado que hoy nada de la factura en curso persiste
  entre recargas)?
- Si el cliente tiene **más de un** tipo de bonificación disponible y elige uno
  al principio: ¿puede cambiar de tipo a mitad de la venta, o queda fijo hasta
  "Nueva factura"? No lo dice la especificación — asumir que queda fijo hasta
  limpiar la factura, salvo que se indique lo contrario, parece lo más simple
  y menos propenso a dejar líneas de grupos distintos mezcladas.
- Las preguntas de negocio #1 y #2 del documento del API (si el tipo del
  cliente exige que el artículo tenga ese mismo tipo asignado, y si puede haber
  más de un tipo activo por factura) cambian directamente el diseño del punto
  4.3.b — no conviene construir esa pantalla hasta tener esas dos respuestas.
