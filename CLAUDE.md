# SeePOS — migracion a .NET 10 / Blazor

Migracion de la SPA React `FrontEndPos2650App` (SUVESA SeePOS) a Blazor.
Este documento fija las decisiones ya tomadas para no volver a discutirlas en cada sesion.

## Estado

Ola 0 — Cimientos. **No se migra ninguna pantalla de negocio todavia.**
Semana 1 completada: proyecto unico con la estructura de la casa, Bootstrap 5 +
Havit.Blazor, contratos del API generados (321 operaciones, 386 DTOs, 51 clientes),
capa ApiConexion verificada contra `devapi.pos2650.com`, y CI.

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

## Trampas ya resueltas — no repetirlas

**`IHttpContextAccessor` e `ISession` no sirven en Blazor Server.** El HttpContext solo
existe durante el render inicial y desaparece al arrancar el circuito. Por eso el token
vive en `IContextoSesion` (scoped al circuito) y no en `ISession` como en
`FCRCASitioAplicacion`. Mismo sentido, distinta implementacion.

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
