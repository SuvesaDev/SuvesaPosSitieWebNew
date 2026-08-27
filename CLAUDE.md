# SeePOS — migracion a .NET 10 / Blazor

Migracion de la SPA React `FrontEndPos2650App` (SUVESA SeePOS) a Blazor.
Este documento fija las decisiones ya tomadas para no volver a discutirlas en cada sesion.

## Estado

Ola 0 — Cimientos. **No se migra ninguna pantalla de negocio todavia.**

- Semana 1: proyecto unico con la estructura de la casa, Bootstrap 5 + Havit.Blazor,
  contratos generados (321 operaciones, 386 DTOs, 51 clientes), CI.
- Semana 2: sesion, sucursal y permisos en servidor. Verificado contra el API real
  hasta el rechazo de credenciales.
- Semana 3: shell completo. Menu lateral de 82 nodos portado desde SidebarData.jsx,
  espacio de trabajo por pestanas, atajos y ruta comodin para las 78 pantallas
  pendientes. 25 pruebas cubren la semantica de pestanas y el filtrado del menu.
- Semana 4: convivencia. YARP, anfitrion de iframe y puente JS listos y verificados
  del lado del sitio. El cambio de `basename` esta hecho en la rama
  `feature/convivencia-blazor` de la SPA, **sin compilar** (no hay Node en el equipo).
- Semana 5: sistema de diseno. Tema con la identidad extraida del SCSS actual,
  fachada de dialogos, manejo unico de errores del API, campos y rejilla envueltos,
  y pagina de muestra en `/diseno`.

**Falta un usuario de pruebas** para ejercitar todo lo que hay detras del login
(ver "Pendiente de verificar").

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

`Class/MenuSeePos.cs`, **generado** desde `SidebarData.jsx`: 8 raices, 82 nodos, arbol
de hasta cuatro niveles. Los titulos se conservan literalmente porque **son la llave de
permisos**: el API responde por `Menu` (titulo de la raiz) y `NombrePantalla` (titulo
de la hoja), no por ruta. Si el menu cambia en React, conviene regenerarlo.

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
| Campos | `AppCampoTexto`, `AppCampoNumero`, `AppCampoMoneda`, `AppCampoFecha` | `HxInput*` directamente |

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

## Rutas pendientes

`Views/Shared/PantallaPendiente.razor` tiene `@page "/{*Ruta}"` y recoge las 78 rutas
del sistema actual que aun no se migran. Las rutas concretas ganan al comodin, asi que
cada pantalla migrada lo va vaciando sola. En la semana 4 este componente pasa a
albergar el iframe con la pantalla React equivalente.

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

## Pendiente de verificar

Sin un usuario de pruebas contra `devapi.pos2650.com` no se ha podido ejercitar nada
de lo que hay detras del login: seleccion de sucursal, permisos reales, tamano del
ticket con ~82 pantallas, el 403 a un usuario sin permiso, y el shell entero
(menu, pestanas, atajos) en pantalla. La logica esta cubierta por pruebas; lo que
falta es verla funcionando con datos reales.

## Trampas ya resueltas — no repetirlas

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
