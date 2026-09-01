# SeePOS — migracion a .NET 10 / Blazor

Migracion de la SPA React `FrontEndPos2650App` (SUVESA SeePOS) a Blazor.
Este documento fija las decisiones ya tomadas para no volver a discutirlas en cada sesion.

## Estado

**Esta seccion se quedo describiendo la Ola 0 mucho despues de que el proyecto avanzo.**
No confiar en fechas/semanas de aqui abajo como estado actual; son historia de los
cimientos. El estado real: **la migracion de pantallas con funcion real esta
completa** (Olas 1 a 5), verificada contra el API real (`dotnet test
tests/SuvesaPosSitioAplicacion.E2E`, 33 pantallas cubiertas en
`PantallasMigradasTests`). Entre lo migrado:

- **Catalogos y consulta** (Ola 1-2): Bancos, Familias, Categorias, Presentaciones,
  Inventarios, Clientes, Proveedores (las tres ultimas con alta/edicion/cambio de
  estado completos — la deuda de "solo consulta" que se menciona mas abajo esta
  **cerrada**, aunque el comentario en cada Consulta.razor todavia no se actualizo).
- **Ventas y compras** (Ola 1-3): Proformas, Cuentas por Cobrar (solo listado — ver
  nota abajo), Cuentas por Pagar, Seguimiento de Cotizaciones, Documentos Emitidos,
  Consulta Albaranes, Consignaciones (seguimiento), Consulta Depositos, Compra,
  Facturacion, Devoluciones de venta y de compra, Orden de compra manual.
- **Ola 4 — escritura compleja**: Usuarios (alta/edicion), Roles (matriz de permisos
  por rol, con la misma compuerta de reconfirmar la propia clave que tiene el
  sistema actual), Sucursales y Empresas (alta del emisor electronico: identificacion,
  ubicacion en cascada, certificado de firma digital, actividades de Hacienda,
  cuentas bancarias).
- **Ola 5 — caja y dinero**: Apertura/Arqueo/Cierre de caja, Pre-depositos y
  Depositos, Configuracion (pronto pago), Entrega a Cuenta, Cobrar (cobro de
  preventas por ficha o cedula, con facturacion automatica si no es credito).
- **Ola 6 — pulido visual y "Consulta Estados Albaranes"**: mejoras de diseño
  (contraste de deshabilitado, colores de marca en `<code>`, campos requeridos,
  secciones en formularios largos, tildes) y el panel real de peticiones Qvet
  dentro de Modulo Inventario — ver mas abajo.

**Nota — "Cuentas por Cobrar" (`/sales/collect`, titulo de menu "Abono Cobrar") es
solo lectura.** El sistema actual SI tiene ahi un registro real de abono
(`/Cobros/InsertarCobro` vía la pantalla "Cobrar"), pero esa transaccion quedo
cubierta por la nueva pantalla `Cobrar` (`/initial/charge`), que cobra facturas de
credito por cedula del cliente. No se duplico el formulario de cobro dentro de
"Cuentas por Cobrar" porque ya existe en "Cobrar"; si en algun momento se decide que
hace falta cobrar directamente desde ese listado, es una extension menor, no un
vacio de datos.

**Fuera de alcance, verificado como mockup sin ninguna llamada real al API:**
Pedidos, Pedidos a Bodega, Ajuste Bodega, Solicitud Bodega, Toma, Pretoma, Pretoma
Fisica General, Movimientos de articulos, Gastos, Ajuste Inventario, Ajuste Pagar,
Prestamos, Agente de ventas, Ajuste Cobrar, Rifa, Etiquetador, Unificar codigos,
Asignar Codigo Cabys, Clientes Frecuentes, Asignar Ficha Por Usuarios, Tarifas,
Ubicaciones, Monedas, Denominacion monedas, Bodegas (parametros), Areas, Registro de
pantalla, Bloquea/Desbloquea bodega, Bloquea/desbloquea X Casa Comercial, Traslado
entre puntos de venta, Convertir Saco por Kilos, Categoria de accion, Condiciones de
Uso Firmado Contado, Modulo Farmacia completo, Registro/Facturacion de
Consignaciones (la funcion real de consignacion — aceptar/rechazar, generar venta —
esta cubierta por "Seguimiento de Consignaciones"). "Facturación" en `/sales/billing`
es un enlace del propio menu original sin ruta real detras; la pantalla que funciona
de verdad es `/initial/billing`, ya migrada.

**Modulo Reportes (`/moduloReportes`) — las 5 pestañas de React (`ReportsCompras`,
`ReportsVentas`, `ReportsInventarios`, `ReportsClientes`, `ReportsProveedor`) son
mockup puro:** sin `onClick`, sin `dispatch`, sin `fetch`, con filas de tabla
literalmente escritas como "Test". Ninguna de las 5 tiene nada real detras. La
pantalla de Blazor (`Reportes/Compras.razor`) construyo el reporte de Compras de
verdad — va MAS ALLA de lo que React ofrecia ahi — pero no hay nada que replicar
para Ventas/Inventarios/Clientes/Proveedores porque el original tampoco lo tiene.
Si en algun momento se decide construirlos, son pantallas nuevas, no una migracion.

El shell (menu, pestanas, convivencia con la SPA React via YARP) y el sistema de
diseno de la Ola 0 siguen como se describen abajo.

## Rediseño de seguridad V2 — SUPERSEDE lo de "Sesion y permisos" de abajo

Ver `docs/REDISENO_SEGURIDAD_USUARIOS_ROLES_WEB.md` (y su par en el repo del API).
Estado: implementado en `feature/seguridad-usuarios-roles-web` (API en
`DevSuvesaPosWeb`, rama `feature/seguridad-usuarios-roles-v2`), a la espera de
regenerar los contratos NSwag contra el API nuevo desplegado.

Cambios que ya mandan sobre el texto viejo de este archivo:

- **Los permisos casan por CÓDIGO de función** (`MODULO.SLUG`), no por rótulo. Cada
  nodo de `MenuSeePos.cs` lleva `Codigo` (generado por `tools/anotar_codigos_menu.py`
  con el mismo algoritmo que la semilla del API). El claim de permiso es
  `moduloCodigo|funcionCodigo|VER,CREAR,...` (`PermisoFuncion`, reemplaza a
  `PermisoPantalla`). Las Views que aún pasan el título siguen funcionando porque
  `ContextoSesion` lo resuelve con `MenuSeePos.ResolverCodigo`.
- **`NombrePantalla.cs` borrado.** El parche de comparar títulos sin tildes ya no
  hace falta; su normalizador de búsqueda vive ahora en `Helpers/Texto.cs`.
- **Acciones**: VER/CREAR/EDITAR/BORRAR/**ACTIVAR/EXPORTAR/IMPRIMIR** (`AccionPantalla`;
  `Modificar` es alias de `Editar`).
- **Perfil** (tipo de cuenta): `SUPER_ADMIN` / `ADMIN` / `USUARIO` (catálogo
  extensible). `SUPER_ADMIN` (`ClaimsSeePos.EsSuperAdministrador`, antes
  `administrador`) ve todo y no pasa por rol. `ADMIN` gestiona usuarios y **lee** la
  config de seguridad; sus permisos de negocio salen del rol, igual que `USUARIO`.
  Las capacidades CostaPets/AgenteCostaPets se **heredan del perfil**.
- **`SeePos:VerPantallasNoGobernadas` por defecto `false`**: el catálogo del API se
  genera del mismo árbol que el menú, así que una función que el rol no menciona es
  una denegación real.
- **Pantallas**: `Views/Parametros/RolesPermisos.razor` (3 pestañas: Roles + matriz,
  Catálogo, Acciones) reemplaza a `Roles.razor`. `Usuarios.razor` elige perfil desde
  `/seguridad/perfiles` y cambia perfil/rol desde la tabla.
- Proxies nuevos: `IRolesPermisos`, `IPerfiles`; `IRoles` retirado. Mientras no se
  regeneren los contratos, `ApiConexion/SeguridadApiCliente.cs` + `DTOs/Seguridad/*`
  son un cliente/DTOs escritos a mano que se sustituyen al correr
  `./tools/actualizar-contratos.sh` contra el API nuevo.

## Diseño visual — pase estilo panel de administracion

Sobre lo de la Ola 6, un pase de densidad y planitud en `tema.css`/`app.css`
(un solo sitio):

- **Tipografia base a `0.875rem`** (`--bs-body-font-size`, antes `0.9375rem`).
- **`.card` sin borde** (`--bs-card-border-width: 0`) — el relieve lo da una sombra
  suave de dos capas, no una linea. Regla base en `.card` + la de `.seepos-nivel-*`.
  Las pantallas con su propia sombra (facturacion, compras...) la conservan.
- Cabecera de tarjeta plana (sin degradado), `.card-header` con borde fino.
- Barra superior mas baja (`3.5rem`) y con sombra tenue; acento verde inferior mas fino.
- `AppModal` estandariza el aspecto de todo modal de contenido (ver "Puntos unicos").

## Diseño visual — pulido de Ola 6

Revision visual sobre capturas reales, aplicada en `tema.css`/`tema.js` (un solo
sitio, efecto en toda la app) y propagada a las pantallas con formularios largos
que ya se habian revisado (Nuevo cliente, Nuevo articulo, Nuevo proveedor,
Empresas):

- Deshabilitado (campos y botones) con contraste real, no un gris casi identico
  al fondo. `--bs-btn-disabled-bg` de `.btn-primary` ya NO es el mismo verde con
  opacidad — antes un boton "Emitir factura" sin datos se veia casi igual de listo
  que uno que si lo estaba.
- `--bs-code-color` fijado al tono tierra de la marca: Bootstrap pinta `<code>` en
  un magenta de libro de estilo (`#d63384`) que se colaba en codigos de articulo y
  en la clave fiscal sin que nadie lo hubiera elegido.
- Campos numericos seleccionan su contenido al enfocarse (`tema.js`): un campo que
  arranca en "0" real (existencias, cantidades) ya no obliga a borrar el cero a
  mano antes de escribir el valor de verdad.
- `AppCampoTexto` gano el parametro `Requerido` (asterisco en el rotulo). Aplicado
  en Cliente, Articulo, Proveedor y Empresa; falta propagarlo al resto de
  formularios si se decide que vale la pena.
- Clase `.seepos-form-seccion` para subtitulos dentro de formularios largos.
  Proveedores y Empresas ya tenian su propio patron de seccion (con nombre propio,
  `.seepos-proveedor-seccion` y tarjetas separadas) — no se toco, ya cumplia lo
  mismo.
- Buscar cliente en Facturacion ahora filtra al escribir (debounce de 400 ms,
  igual que el buscador de articulos) en vez de exigir clic en "Buscar". Proveedor
  (Compra, Orden de compra) ya filtraba al escribir de una lista precargada — no
  necesitaba el cambio.
- Boton "Quitar" con icono de basurero en toda la app (antes solo texto rojo).
- Copiar al portapapeles junto a la clave fiscal (Compra → importar XML,
  Documentos Emitidos).
- Tildes corregidas en el login, Usuarios, Empresas — con el ajuste espejo en los
  locators de Playwright que buscaban el texto viejo sin tilde
  (`GetByRole`/`GetByLabel` no hacen match de "sesion" contra "sesión").

### Consulta Estados Albaranes — de catalogo a panel real

`Modulo Inventario` (`/moduloInventario`) solo mostraba la lista de nombres de
estado. El sistema actual (`StateBody.jsx`) tiene ahi un panel real: gateado con
clave interna (reutiliza `AppDesbloqueoClave`, el mismo componente que usa
Consulta Depositos), trae las peticiones de pruebas medicas de Qvet pendientes de
facturar (`Qvet/ObtenerAlbaranesPendientesFacturarFiltrado`) y las deja filtrar
por personal/estado/prueba medica. Ahora Blazor hace lo mismo, con pestañas
("Consulta Estados Albaranes" / "Inventario QR") en vez de las dos tarjetas
lado a lado que tenia antes.

Proxy nuevo en `IAlbaranes`: `PendientesDeFacturarFiltrado()` y `PruebasMedicas()`,
ambos ya existian en el cliente generado (`IQvetApiCliente`) — solo faltaba
envolverlos. Sin usuario de pruebas con datos de Qvet reales todavia no se
verifico con datos reales, solo contra el build y las unitarias.

## Bonificación — Cliente, Artículo y Facturación

Portado de la rama `feature/bonificacion` del sistema actual (React), pero **no
tal cual** — esa rama, comparada contra el swagger real de `devapi.pos2650.com`
(hubo que descargarlo de nuevo; el cacheado en este repo estaba de antes de esta
funcionalidad), tiene dos problemas confirmados:

1. **Dos endpoints que React llama no existen en el API**:
   `POST /ClienteBonificacion/CreateArticulo` y `GET /ClienteBonificacion/GetArticulos`.
   Cualquier alta de "producto de bonificación" en la pantalla de Clientes del
   sistema actual falla con 404 ahora mismo. El DTO real
   (`ClienteBonificacionConfiguracionDTO`) ya trae `idArticulo`/`descripcionArticulo`
   en el mismo registro que el tipo de bonificación — no hace falta una lista
   separada de "productos". Blazor sigue el modelo real: **un solo formulario**
   (tipo + artículo juntos), no las dos listas de React.
2. **El modal de Facturación (`BillingBonificacionesModal.jsx`) esta a medio
   hacer**: sus handlers (`handleSaveCorreos`, `closeModal`, etc.) son un
   copy-paste del modal de correos de comprobante — el boton "Bonificar" no
   aplica nada a la factura. El campo que si existiria para eso
   (`FacturaDetallesDTO.esBonificacion`) no lo usa ninguna pantalla de React
   todavia. Blazor construyo la parte informativa (mostrar la bonificacion del
   cliente al elegirlo en Facturacion) y **no** inventa la logica de aplicar la
   bonificacion a la factura — decision explicita, no pendiente por olvido.

Lo que si se construyo, completo y contra los endpoints reales (`ArticuloBonificacion`,
`ClienteBonificacion`, `ConfiguracionBonificacion` — los tres ya en el swagger,
solo faltaba envolverlos):

- **Cliente** (`Clientes/Consulta.razor`): checkbox "Bonificado" (gateado a
  `Sesion.EsCostaPets`, igual que React) y pestaña "Bonificación" — visible solo
  editando un cliente ya guardado, porque la bonificacion necesita un `idCliente`
  real. CRUD completo (alta/edicion/baja reales, no solo en memoria como en React).
- **Artículo** (`Inventario/Consulta.razor`): mismo patron para "Tipos de
  bonificación" (`IArticuloBonificacion`, CRUD completo). Ademas, a diferencia
  del cliente, el articulo si tiene una segunda lista real y funcional:
  "Productos de regalo", que reutiliza el endpoint YA EXISTENTE de artículos
  relacionados (`articulosRelacionados/GetRelacionadosBonificacion` y
  `/putArticuloRelacionadoBonificacion`, agregados a `IArticulosRelacionados`)
  marcando `esRelacionBonificacion`. Ese si funciona en React tal cual, solo que
  ahi Editar/Eliminar son locales (no llaman al API); en Blazor son reales.
- **Facturación**: al elegir un cliente con `TieneBonificacion`, se muestra un
  bloque informativo con su configuracion. No agrega lineas ni descuentos a la
  factura — ver el punto 2 de arriba.

Contratos regenerados (`./tools/actualizar-contratos.sh`) para traer estos DTOs
y clientes; no existian en el `SeePosDtos.cs`/`SeePosApiClientes.cs` que ya
estaba en el repo. **Sin verificar contra datos reales de un cliente/articulo
bonificado** — solo contra build limpio y unitarias; falta un usuario de
pruebas con casos de bonificacion reales para revisar visualmente.

## Decisiones cerradas — no reabrir sin motivo nuevo

| Tema | Decision | Por que |
|---|---|---|
| Estructura | **Un solo proyecto**, carpetas de `FCRCASitioAplicacion` | Estandar de la casa, para mantenimiento uniforme |
| Modelo de hosting | **Blazor Server** (`InteractiveServer`) | Es lo unico que permite un solo proyecto conservando ApiConexion, Security y Services como capas de servidor |
| Componentes | **Bootstrap 5 + Havit.Blazor** (MIT) | Bootstrap 5 conserva la identidad visual. Havit da los componentes nativos sobre Bootstrap |
| Rejillas y selects | **HxGrid, HxAutosuggest, HxMultiSelect** | Cubren lo de DataTables y Select2 sin jQuery: esos plugins mutan el DOM y chocan con el renderizador de Blazor |
| Estado | Servicios con scope, **nunca Fluxor** | Portar 95 reducers uno a uno daria mas codigo del que hay hoy |
| Seguridad | Token en `IContextoSesion`, con scope de circuito | El navegador nunca ve el token. Hoy el JWT y los permisos viven en localStorage y son editables desde la consola |
| Aritmetica fiscal | `decimal`, en `Services/` | La app actual usa coma flotante en el navegador |
| Impresion | **Ninguna** | Sin tiquete, gaveta ni bascula. PDF en A4 que se ve o se descarga |
| Backend | **No se toca** | El API REST externo y sus endpoints quedan igual |

### Coste asumido de Blazor Server

Con el servidor en la nube y las cajas por internet, cada evento de interfaz es un viaje
de red. **En las pantallas de captura rapida (facturacion, toma fisica) hay que usar
`@bind:event="onchange"` y no `oninput`**, y evitar re-render por pulsacion. Si una
pantalla resulta inusable por latencia, se replantea esa pantalla, no la arquitectura.

## Estructura

Misma organizacion que `FCRCASitioAplicacion`, adaptada donde Blazor lo exige:

```
src/SuvesaPosSitioAplicacion/
  ApiConexion/
    ProxyInterface/   IXxx: lo que las Views pueden pedir
    ProxyClass/       Xxx: envuelve el cliente generado, traduce el envelope
    Generated/        clientes HTTP generados por NSwag. NO editar
  Class/              tipos transversales y enumeraciones
  Controllers/        endpoints MVC, solo si hacen falta
  DTOs/Generated/     tipos de datos generados por NSwag. NO editar
  Helpers/            Response, ResponseGeneric, EnvelopeApi, ApiAuthHeaderHandler
  Models/             ViewModels de pantalla
  Security/           IContextoSesion, permisos
  Services/           logica de aplicacion propia del sitio
  Views/              pantallas .razor por modulo, mas Shared
  wwwroot/            estaticos
tests/  unitarias y E2E con Playwright
tools/  generacion de contratos
```

**`Views/` en lugar de `Components/`**: mismo sentido que las Views de MVC, una carpeta
por modulo. `Views/Shared/` guarda App, Routes, MainLayout y las paginas comunes.

## Arquetipo de proxy

`ApiConexion/ProxyClass/Seguridad.cs` es el modelo. Los otros 50 se escriben igual:

1. Envuelve el cliente generado por NSwag; nunca construye `HttpClient` a mano.
2. Traduce el envelope del API con `EnvelopeApi.A(...)`.
3. Atrapa la excepcion ahi: **una View jamas debe ver una `ApiException`**.
4. Sin logica de negocio. Solo transporte y traduccion.

Verificado de punta a punta: `/diagnostico/apiconexion` (solo en desarrollo) llama al API
real y devuelve el fallo como `ResponseGeneric`, no como excepcion.

## Sesion y permisos

Flujo, todo en render estatico porque establecer la cookie exige `HttpContext`:

1. `/cuenta/ingresar` valida contra `/usuario/LoginNuevo` y firma la sesion sin sucursal.
2. `/cuenta/sucursal` lista los centros y anade la sucursal elegida a la sesion.
3. `MainLayout` manda a `/cuenta/sucursal` a quien tenga sesion pero no centro.

El ticket lleva el token del API y un claim por pantalla
(`menu|pantalla|ver|crear|modificar|borrar`, ver `PermisoPantalla.AClaim`).
**Ese ticket se guarda en servidor** (`AlmacenTickets`, sobre `IDistributedCache`);
la cookie del navegador solo lleva la llave. Motivo: ~82 permisos mas el token no
caben en 4 KB, y el token no tiene por que salir del servidor ni cifrado.

Autorizacion: `FallbackPolicy` exige sesion en todo. Lo anonimo se marca a mano
(`/cuenta/ingresar`, `/healthz`, estaticos).

LIMITE: el almacen es memoria del proceso, asi que las sesiones se pierden al
reiniciar y no se comparten entre instancias. Para varias instancias, cambiar el
`IDistributedCache` por Redis o SQL en `Program.cs`; `AlmacenTickets` no cambia.

## Espacio de trabajo por pestanas

`IEstadoEspacioTrabajo`, con scope de circuito. Semantica portada de `tabsReducer.js`:

- Una pestana por pantalla. Si ya esta abierta, se trae al frente.
- **Las ventas son la excepcion**: cada apertura crea una nueva, numerada
  (`Venta # 2`, ruta `/initial/billing/2`).
- Al cerrar la activa, pasa a la anterior. Cerrar una venta pide confirmacion.
- Se persiste en el navegador con `ProtectedLocalStorage`, detras de
  `IAlmacenEspacioTrabajo` para poder probar la logica sin navegador.

**Diferencia con el sistema actual**: alli las pestanas se identifican por nombre y se
buscan con `name.includes(...)`, asi que cerrar "Clientes" arrastraba tambien a
"Clientes Frecuentes" (de ahi los casos especiales del reducer). Aqui cada pestana
lleva un id estable. Mismo comportamiento visible, sin la fragilidad.

## Menu

`Class/MenuSeePos.cs`, base **portada** de `SidebarData.jsx` y despues reorganizada:
12 raices, 100 nodos, arbol de hasta cuatro niveles. Los titulos se conservan
literalmente porque, junto con el `Codigo`, **son la llave de permisos**. Si se toca
el arbol hay que correr `tools/anotar_codigos_menu.py` (sitio) y
`tools/generar_semilla_seguridad.py` (API) — mismo algoritmo de slug, no pueden
divergir; `MenuCodigosTests` y `FiltroMenuTests.ElMenuRealSeCargoCompleto` lo vigilan.

Reorganizacion respecto a React (a peticion del usuario): modulo nuevo **"Catálogos"**
(`CATALOGOS`, `bi-collection`) con los 19 catalogos de mantenimiento que estaban
sueltos en "Parametros" (categorias, monedas, presentaciones, tipos de factura/cobro/
identificacion/exoneracion, impuestos, formas de pago, monedas fiscales y sus
denominaciones, plazos, geografia fiscal, bancos, clientes frecuentes, tarifas,
ubicaciones, familias). "Parametros" queda con usuarios, roles, empresas, sucursales,
series, configuracion, emisores y las pantallas mockup de bodega/areas/bloqueos.
**"Caja"** (antes bajo "Inicio", `CAJA` / `bi-cash-stack`) y **"Presupuestos"** (antes
bajo "Ventas", `PRESUPUESTOS` / `bi-file-earmark-text`) pasaron a ser **modulo propio**
con todas sus funciones (`CAJA.APERTURA_CAJA`, `CAJA.DEPOSITOS.*`,
`PRESUPUESTOS.PROFORMAS_O_COTIZACION`, ...). Siguen gobernadas por el catalogo del API.
Los codigos viejos (`INICIO.CAJA*`, `VENTAS.PRESUPUESTOS*`) quedan en la BD como
inactivos tras re-sembrar; la pestaña Catálogo de `RolesPermisos` los oculta salvo que
se marque "Ver inactivos".

`Security/FiltroMenu` decide que se ve. Un grupo se muestra si algun descendiente se
muestra. **Mejora respecto al sistema actual**, donde el menu solo se filtra en la
variante CostaPets y el camino normal ensena todas las pantallas.

## Convivencia con la SPA React

YARP sirve la SPA bajo el mismo origen mientras queden pantallas sin migrar:

| Ruta | Destino | Autorizacion |
|---|---|---|
| `/legado/{**resto}` | SPA, quitando el prefijo | `default` (exige sesion) |
| `/assets/{**resto}` | SPA, sin tocar | `anonymous` (son bundles) |

`Views/Shared/PantallaLegado.razor` alberga el iframe y `wwwroot/js/legado.js` hace de
puente. Mismo origen a proposito: permite compartir `localStorage` y ocultar el cromo
de la SPA sin necesidad de `postMessage` para lo basico.

### Cambio pendiente en la SPA — bloquea esta ola

La SPA usa `BrowserRouter` sin `basename`, asi que bajo `/legado/` lee
`window.location.pathname` con el prefijo incluido, no encuentra ruta y redirige a
`/auth/login`. Comprobado: cargar `/legado/initial/billing/1` termina en la pantalla
de login del sistema actual.

Hacen falta dos cambios en `FrontEndPos2650App`, ambos guiados por variable de entorno
para que el despliegue normal no cambie:

```js
// vite.config.js
export default defineConfig({
  base: process.env.VITE_BASE ?? '/',
  plugins: [react()],
})
```

```jsx
// src/routes/AppRouter.jsx
<Router basename={import.meta.env.BASE_URL.replace(/\/$/, '')}>
```

Con eso se compila una variante embebible (`VITE_BASE=/legado/ yarn build`) sin tocar
el build de produccion actual. Los assets pasan a `/legado/assets/*`, con lo que la
ruta `legado-assets` del proxy deja de hacer falta.

### El token durante la convivencia

La SPA lee el token de `localStorage`, asi que mientras haya pantallas sin migrar el
navegador vuelve a verlo: `legado.js` lo siembra antes de crear el iframe. **No es una
regresion** —hoy es asi— pero significa que la ventaja del BFF solo se cobra entera
cuando se retire el ultimo iframe.

Alternativa si se quiere cerrar eso antes: compilar la SPA apuntando su
`VITE_API_URL` a una ruta del propio sitio y anadir una ruta de proxy que inyecte el
token en servidor. Es el mismo tipo de cambio (configuracion de build) y evita que el
token salga.

## Sistema de diseno

`wwwroot/css/tema.css` lleva la identidad, **extraida del SCSS actual, no inventada**:
azul `#1072a9` y naranja `#ee7519` de `base/_settings.scss`, los grises mas usados
medidos sobre el codigo, y Helvetica de `base/_base.scss`. Se expresa sobre las
variables de Bootstrap 5.3 para que los componentes de Havit lo hereden solos.

**No sobreescribir `--bs-secondary`.** En Bootstrap 5.3 es el gris neutro de
`.text-secondary` y compania, no un acento de marca: cambiarlo pone naranja todo el
texto atenuado de la aplicacion. El naranja se aplica donde toca (`.btn-secondary`).

Tema oscuro con `data-bs-theme`, que Bootstrap no deduce solo de
`prefers-color-scheme`: lo fija un script en el `head` antes de que cargue Blazor,
para que no haya destello de tema claro. La eleccion se recuerda por navegador.

### Puntos unicos — las Views no deben saltarselos

| Necesidad | Usar | Nunca |
|---|---|---|
| Confirmar, avisar, informar | `IServicioDialogos` | `IHxMessageBoxService`, `IHxMessengerService` |
| Respuesta fallida del API | `IManejadorRespuestas` | Comprobar `EsCorrecta` a mano en cada pantalla |
| Tabla de datos | `AppRejilla` | `HxGrid` directamente |
| Modal de contenido/edicion | `AppModal` (cabecera+subtitulo, cuerpo scrollable, pie fijo Cancelar+Guardar con estado) | `HxModal` directamente |
| Panel de filtros de una consulta | `AppFiltros` | Caja de busqueda a mano por pantalla |
| Campos | `AppCampoTexto`, `AppCampoNumero`, `AppCampoMoneda`, `AppCampoFecha` | `HxInput*` directamente |

`AppModal` (`Views/Shared/Componentes/`) envuelve `HxModal`; se abre/cierra con
`MostrarAsync()` / `OcultarAsync()`. Migrados hasta ahora: los 4 modales de
`RolesPermisos`. El resto de `HxModal` (~45) sigue pendiente de pasar. Para preguntas
si/no o avisos NO se usa `AppModal`: se usa `IServicioDialogos`.

`Views/Shared/Componentes` esta en `_Imports.razor`, asi que `App*` no necesita
`@using` por pantalla.

`IManejadorRespuestas` centraliza tambien la sesion caducada: ante un 401 avisa y
manda a reingresar, en vez de ensenar el error tecnico.

## Modos de render

**La aplicacion es interactiva por defecto** (`InteractiveServer`), decidido en
`App.razor` segun `HttpContext.AcceptsInteractiveRouting()`.

Las pantallas de sesion llevan `[ExcludeFromInteractiveRouting]` y se quedan en
estatico: establecer la cookie exige `HttpContext`, que un componente interactivo
ya no tiene.

**Un layout no puede llevar `@rendermode`**: su `Body` es un `RenderFragment` y no
se puede serializar a traves de la frontera. Falla en ejecucion, no al compilar.

**El circuito de SignalR (`/_blazor`) queda bajo la `FallbackPolicy`.** Consecuencia:
una pantalla interactiva marcada `[AllowAnonymous]` no arranca, y falla con un
`Failed to complete negotiation ... 401` que no dice de donde viene. No estorba
porque todas las pantallas interactivas van tras el login, pero conviene saberlo.

## Las pantallas migradas ocupan la ruta del menu

Una pantalla nueva **tiene que declarar la misma ruta que el menu**
(`/initial/customers`, no `/consulta/clientes`). Si no, el menu sigue llevando al
iframe de la SPA y la pantalla nueva queda inalcanzable. Paso con tres de la Ola 1 y
no se detecto hasta abrirlas a mano: las pruebas las visitaban por su ruta propia, no
por la del menu.

**Deuda cerrada.** Clientes, Inventarios y Proveedores llegaron a tener alta,
edicion y cambio de estado completos en la Ola 4 (con sus modales, cuentas
bancarias/lotes/relacionados segun corresponda a cada una). Ya no hace falta
entrar por la SPA React para mantenerlas.

## Rutas pendientes

`Views/Shared/PantallaPendiente.razor` tiene `@page "/{*Ruta}"` y recoge las rutas del
sistema actual que aun no se migran. Las rutas concretas ganan al comodin, asi que cada
pantalla migrada lo va vaciando sola.

**Solo muestra el iframe si la SPA React responde de verdad** (`ISondaLegado`, con
cache corta). Antes bastaba con que hubiera URL configurada, y si la SPA no estaba
levantada el usuario veia una pantalla **en blanco sin ninguna explicacion**. Ahora
sale un aviso con la URL que no responde y un boton de reintentar.

## Verificacion contra el API real

`tests/SuvesaPosSitioAplicacion.E2E` ejercita la sesion contra el API de verdad.
Las credenciales se leen del entorno, **nunca del codigo**, para que no acaben en el
repositorio ni en el historial. Sin ellas, esas pruebas se omiten en lugar de fallar.

```bash
export SEEPOS_USUARIO='...'
export SEEPOS_PASSWORD='...'
dotnet test tests/SuvesaPosSitioAplicacion.E2E --logger "console;verbosity=detailed"
```

Cubren: que el login devuelva token y permisos, que se listen las sucursales, y
**que los NombrePantalla del API coincidan con los titulos del menu**. Esto ultimo es
lo mas delicado: los permisos casan por titulo, no por ruta, asi que una diferencia
de un acento hace desaparecer esa pantalla para todo el que no sea administrador.
Esa prueba hoy solo informa del desfase; cuando se conozca, pasa a ser una asercion.

## Permisos y menu — medido y resuelto

Ejecutado contra el API real con `admin`. El catalogo de permisos **esta incompleto**:

| | |
|---|---|
| Titulos en el menu | 78 |
| Pantallas que el API menciona | 20 |
| No casaban por escritura | 2 |

**Resuelto — diferencias de escritura.** `NombrePantalla.Comparador` ignora tildes,
mayusculas y espacios sobrantes. Asi `Facturacion` del API casa con `Facturación`
del menu. Sin esto, esa pantalla desaparecia para todo rol no administrador.

**Resuelto — pantallas que el API no menciona.** Una pantalla no mencionada **no es
lo mismo que una denegada**. Tratarlas igual escondia 60 pantallas que hoy se ven.
`ContextoSesion.Puede` devuelve `true` para lo no gobernado, que es **paridad con el
sistema actual**, donde el menu no se filtra.

Se puede endurecer con `SeePos:VerPantallasNoGobernadas: false`, pero eso deja fuera
esas 60 hasta que el API complete su catalogo.

Esto **no relaja la seguridad real**: quien autoriza es el API, que responde 401 o
403 si el rol no puede. El menu solo decide que se ensena.

**Sin resolver — `Consignacion`.** El API concede permiso para una pantalla que
**no existe en el menu**. Las tres rutas (`/buys/consignment/register`, `/billing`,
`/following`) estan en el enrutador de React pero `SidebarData.jsx` no tiene ninguna
entrada que lleve a ellas. No es un fallo del portado: el menu actual no las incluye.
Hay que decidir si se anaden al menu o se llega a ellas desde otra pantalla.

## Pruebas de extremo a extremo

`tests/SuvesaPosSitioAplicacion.E2E` levanta la aplicacion en un puerto libre y la
conduce con Playwright sobre el **Chrome instalado** (`Channel = "chrome"`), en vez
de descargar los navegadores propios: son cientos de megas y aqui no se prueba
compatibilidad entre navegadores.

```bash
dotnet test tests/SuvesaPosSitioAplicacion.E2E
```

Dos grupos:

- **`CimientosE2ETests`** — corre siempre. Cubre los fallos de la Ola 0: estaticos
  que redirigian al login, consola sin errores, rutas del sistema actual que piden
  sesion en vez de dar 404, y el mensaje de error del API en el ingreso.
- **`SesionE2ETests`** — necesita `SEEPOS_USUARIO` y `SEEPOS_PASSWORD` en el entorno;
  sin ellas se omite. Cubre el modal de centro, el shell, la numeracion de las
  pestanas de venta y que sobrevivan a recargar.

### Por que existe esta suite

Los cuatro fallos que costaron la Ola 0 eran de **integracion**: politicas de
autorizacion aplicadas a endpoints que no debian, modos de render, ambitos de
inyeccion. Ninguna prueba unitaria los vio.

Comprobado por mutacion: reintroduciendo el bug de `MapStaticAssets` sin
`AllowAnonymous`, la prueba E2E falla. Reintroduciendo el del token que no llega al
handler, **las 44 unitarias siguen en verde** y solo lo atrapa la E2E con sesion.

**Antes de replicar una pantalla en serie, la suite tiene que estar verde.**

## Comparador fiscal — semana 8

`ComparadorFiscalTests` toma documentos reales del API, recalcula cada linea con
`CalculoDocumento` y compara importe a importe contra lo que el sistema actual dejo
guardado. Necesita usuario de pruebas.

```bash
dotnet test tests/SuvesaPosSitioAplicacion.E2E \
  --filter "FullyQualifiedName~ComparadorFiscal" \
  --logger "console;verbosity=detailed"
```

Informa cuantas discrepancias hay, en que campo y de que tamano. **Todavia no
afirma nada**: primero hay que ver el desfase real contra datos de produccion.

Alguna diferencia es esperable: el sistema actual calcula en el navegador con coma
flotante y el API guarda los importes en `double`. Lo que importa es **cuanta y
donde**. Cuando se conozca, hay que decidir si se replica el comportamiento antiguo
o se corrige —**decision de negocio, no tecnica**— y entonces la prueba pasa a ser
una asercion.

**La Ola 3 no deberia abrirse antes de eso.** Es la ola que emite documentos fiscales.

## Diagnosticos que se quedan

`/diagnostico/sesion`, solo en desarrollo. Se queda **contra lo que dije al empezar**:
fue lo que dio el dato decisivo en el fallo del token, y cuando alguien reporta un
problema es mas rapido que correr la suite. Nunca devuelve el token, solo su largo.

`/diagnostico/apiconexion` se retiro: la suite E2E hace lo mismo y mejor.

## Pendiente de verificar

Ya hay usuario de pruebas y todo el shell (login, sucursal, permisos, menu,
pestanas, atajos) esta verificado funcionando con datos reales — la version vieja
de esta seccion decia lo contrario, quedo obsoleta. Lo que sigue realmente
pendiente:

- **Licencia de QuestPDF.** La comunitaria (`LicenseType.Community` en
  `Program.cs`) es gratuita solo por debajo de cierto umbral de facturacion anual
  de la organizacion que la usa — no del cliente final. Falta confirmar si SUVESA
  ya lo supera antes de ir a produccion; si lo supera, el cambio queda aislado
  detras de `IGeneradorPdf` (una clase, no un rediseño).
- **`ComparadorFiscalTests` a escala.** Compara los calculos de `CalculoDocumento`
  contra documentos reales del API, pero la base de desarrollo solo tenia 1-2
  documentos historicos cuando se escribio. Informa diferencias, no las afirma
  todavia (ver "Comparador fiscal" mas abajo); hace falta mas volumen real para
  que decir "sin diferencias" signifique algo.
- **Build de la SPA React con `VITE_BASE=/legado/`.** El cambio de `basename` esta
  hecho en la rama `feature/convivencia-blazor` de la SPA pero nunca se compilo
  (no hay Node.js en este equipo) — sigue sin verificar que el bundle resultante
  funcione bajo ese prefijo.
- **Despliegue.** Deliberadamente fuera de alcance por ahora, a peticion expresa
  del usuario — no se ha decidido donde ni cuando.

## Trampas ya resueltas — no repetirlas

**Nunca inyectar un servicio con scope en `ApiAuthHeaderHandler`.** Es la trampa que
mas cara salio de toda la Ola 0.

`IHttpClientFactory` **no** resuelve sus handlers desde el ambito de la peticion:
crea el suyo y lo reutiliza unos minutos. Un `IContextoSesion` inyectado ahi llega
**vacio**, aunque la pantalla tenga la sesion perfectamente cargada. El sintoma es un
401 del API con todo aparentemente bien, y no hay nada en pantalla que lo explique.

Medido: con sesion valida y token de 512 caracteres, el handler veia
`largo del token=0, cabecera puesta=False`.

**Como se resuelve.** `ProxyBase.Ejecutar` es el unico sitio que toca esto:

1. Carga el contexto (el proxy si vive en el ambito correcto).
2. Deja el token en `ContextoLlamada`, que usa `AsyncLocal` y **si** atraviesa el
   limite porque va con el flujo asincrono de quien llama.
3. Contiene la excepcion, para que ninguna View vea una `ApiException`.

**Todos los proxies heredan de `ProxyBase`.** Uno que no lo haga fallara con 401 sin
explicacion. De paso desaparece el "hay que acordarse de llamar a `CargarAsync`".

`CargarAsync` tambien atrapa la excepcion de ambito y lo trata como "sin sesion" en
lugar de reventar: durante el login, efectivamente todavia no hay usuario.

**No ensenar al usuario el error tecnico del API.** Un
`The HTTP status code of the response was not expected (401)` no le sirve de nada y
no puede hacer nada con el. El detalle va al log; en pantalla, un mensaje accionable.

**`IHttpContextAccessor` e `ISession` no sirven en Blazor Server.** El HttpContext solo
existe durante el render inicial y desaparece al arrancar el circuito. Por eso el token
vive en `IContextoSesion` (scoped al circuito) y no en `ISession` como en
`FCRCASitioAplicacion`. Mismo sentido, distinta implementacion.

**`MapStaticAssets().AllowAnonymous()`, siempre.** La `FallbackPolicy` de
autorizacion se aplica a TODOS los endpoints, estaticos incluidos. Sin
`AllowAnonymous`, el CSS y el JS responden 302 al login y el navegador intenta
ejecutar el HTML del login como script: `SyntaxError: Unexpected token '<'`.
Lo mismo vale para `/healthz` y `/favicon.ico`.

**`MapStaticAssets`, nunca `UseStaticFiles`.** .NET 10 pone huella a los recursos
estaticos de las bibliotecas de componentes. El import-map de Blazor apunta al nombre con
huella, que solo existe en el manifiesto y no en disco.

**El JS de Bootstrap va antes que Blazor.** Los componentes de Havit llaman a la API JS de
Bootstrap. Sin `bootstrap.bundle.min.js` cargado antes, `HxMultiSelect` falla con
`bootstrap is not defined`. El CSS lo sirve el paquete de Havit; el JS lo servimos nosotros.

**Havit reparte el registro de servicios.** `AddHxServices()` no basta: hacen falta
`AddHxMessenger()` y `AddHxMessageBoxHost()`, y este ultimo vive en el namespace
`Havit.Blazor.Components.Web.Bootstrap`.

**El OpenAPI del API necesita saneado.** Llega con `maximum: 1.79e308` en propiedades
decimales, que no cabe en `System.Decimal` y hace fallar a NSwag. Lo limpia
`tools/sanear-openapi.py`, que ya corre dentro de `tools/actualizar-contratos.sh`.

## Regenerar los contratos

```bash
./tools/actualizar-contratos.sh                       # usa devapi.pos2650.com
./tools/actualizar-contratos.sh https://otra-url      # otro entorno
```

Produce `DTOs/Generated/SeePosDtos.cs` y `ApiConexion/Generated/SeePosApiClientes.cs`.
**Ninguno se edita a mano.**

## Reglas de trabajo

1. **Un lote es un caso de uso completo**: pantalla, proxy, servicio y prueba. Si no se
   puede revisar y desplegar en una semana, es demasiado grande.
2. **Si algo se repite en mas de tres pantallas, se resuelve en un solo sitio.**
3. **Paridad primero.** Nada de rediseno ni funcionalidad nueva hasta terminar un modulo.
4. **Las Views no llaman al API.** Siempre a traves de `ApiConexion/ProxyInterface`.
5. **Los permisos se comprueban en el servidor.** Esconder un boton no es autorizar.

## Niveles adaptables

| Nivel | Desde | Para |
|---|---|---|
| A — movil | 360 px | Consulta de inventario y precios, clientes, cotizaciones, reportes |
| B — tableta | 768 px | Toma y pretoma, pedidos a bodega, consignacion |
| C — escritorio | 1280 px | Facturacion, caja, arqueo, compras, tesoreria, parametros |

## Referencia

- Sistema actual: `../FrontEndPos2650App`. La logica de una pantalla esta repartida entre
  su componente, sus `actions/` y sus `reducers/`.
- Estandar de la casa: `AplicacionSitioWebGeneralFCRCA/FCRCASitioAplicacion`.

## Convenciones

- Nombres de codigo en ingles cuando son tecnicos; dominio y textos de interfaz en espanol.
- Los nombres de campos que vienen del API se respetan tal cual (`CodArticulo`, `Monto_Impuesto`).
- Nada de `float`/`double` para dinero. Siempre `decimal`.
