# Rediseño de seguridad — Perfiles, Roles y Permisos (lado sitio Blazor)

> Documento de diseño e implementación. **Par**: `DevSuvesaPosWeb/ApiSuvesaPos/docs/REDISENO_SEGURIDAD_USUARIOS_ROLES.md`
> (API). La sección **Contrato de API** (§5) es idéntica en los dos.
>
> Alcance: `src/SuvesaPosSitioAplicacion` — `Security/`, `Services/ServicioAutenticacion`,
> `Class/MenuSeePos.cs`, `Models/ItemMenu.cs`, proxies de `ApiConexion`, pantallas de
> `Views/Parametros` (Usuarios, Roles) y pruebas.
>
> Estado: propuesta. No hay código escrito todavía.

---

## 1. Qué hace hoy el sitio (medido sobre el código)

### 1.1 Piezas

| Archivo | Rol actual |
|---|---|
| `Security/ClaimsSeePos.cs` | Tipos de claim: `Token`, `Administrador`, `IdRol`, `NombreRol`, `CostaPets`, `AgenteCostaPets`, `AceptaConsignacion`, y **`Permiso`** (uno por pantalla). |
| `Security/PermisoPantalla.cs` | Record `(Menu, Pantalla, Ver, Crear, Modificar, Borrar)`. `AClaim()` serializa a `menu|pantalla|1|0|1|0`; `DesdeClaim()` invierte. |
| `Security/NombrePantalla.cs` | **Parche**: comparador que ignora tildes/mayúsculas/espacios para que `Facturacion` (API) case con `Facturación` (menú). |
| `Security/IContextoSesion.cs` + `ContextoSesion.cs` | Lee los claims del ticket. `Puede(pantalla, accion)`, `PuedeVer`, `EstaGobernada`. Bandera `SeePos:VerPantallasNoGobernadas` (default `true`) porque el catálogo del API está incompleto (20 de 78 títulos). |
| `Security/FiltroMenu.cs` | `EsVisible(item, sesion)`: admin ve todo; grupo visible si algún hijo visible; hoja visible si `sesion.PuedeVer(item.Titulo)`. |
| `Class/MenuSeePos.cs` | Árbol de menú (8 raíces, 82 nodos, 4 niveles). **El `Titulo` es la llave de permisos.** Generado desde `SidebarData.jsx`. |
| `Models/ItemMenu.cs` | `Titulo`, `Ruta?`, `Icono?`, `Hijos`. **No tiene código estable.** |
| `Services/ServicioAutenticacion.cs` | `ConstruirClaims(Autenticacion)`: aplana `auth.Rol.Permisos` a claims `seepos:permiso`. Firma cookie; el ticket vive en servidor (`AlmacenTickets`). |
| `ApiConexion/ProxyInterface/IRoles.cs` + `IUsuarios.cs` + `ISeguridad.cs` | `IRoles`: `Buscar`, `Modulos`, `Pantallas(idModulo)` (devuelve `Ventanas`), `ObtenerUno`, `Crear`, `Editar`, `ValidarPasswordActual`. `IUsuarios`: `Buscar`, `ObtenerUno`, `Crear` (`UsuariosDTO`), `Editar` (`UsuarioDTO`), `ObtenerPerfiles`, `ObtenerRoles`. |
| `Views/Parametros/Usuarios.razor` | Alta/edición. Perfil = desplegable de `Perfil`. Rol solo se fija al **crear** (el API no lo admite en edición). Clave siempre en el cuerpo. |
| `Views/Parametros/Roles.razor` | Matriz por rol. Compuerta: reconfirmar la **propia** clave (`ValidarPasswordActual`). Filas `(Modulo, Pantalla, Ver/Crear/Modificar/Borrar)`. |

### 1.2 Límites que arrastra

1. **Permisos por texto** → parche `NombrePantalla`; frágil ante cambios de rótulo.
2. **Solo CRUD.** No hay *Activar/Exportar/Imprimir*.
3. **Menú sin identidad estable.** `FiltroMenu` casa por `Titulo`.
4. **Perfil no gobierna nada** en el sitio (solo se envía al API). `EsAdministrador` sale
   del claim `Administrador`, no del perfil.
5. **`VerPantallasNoGobernadas = true`** porque el catálogo del API está incompleto:
   efectivamente el menú casi no se filtra para no‑administradores.

---

## 2. Objetivo

Consumir el modelo nuevo del API (Perfiles con capacidades + Roles con matriz
Módulo→Función→Acción de 7 acciones), **casando por `Codigo` y no por rótulo**, con:

- Un mantenimiento único de **Roles y Permisos** que además administra el catálogo
  (módulos, funciones, acciones).
- `Usuarios` con selección de perfil desde la tabla y **compuerta Super Administración**.
- Menú y catálogo del API **sincronizados por construcción** (misma semilla).
- Retirar el parche `NombrePantalla` y poder poner `VerPantallasNoGobernadas = false`.

---

## 2 bis. Decisiones resueltas (confirmadas por el usuario, 2026-08-31)

Cierran §10. Mandan sobre cualquier "confirmar" del resto del documento.

- **`SUPER_ADMIN`** (`perfil.esSuperAdministracion = true`): acceso total, **no** pasa
  por rol, `permisos` llega vacío. En el sitio, `EsSuperAdministrador ⇒ Puede(...) = true`
  siempre. Solo respeta `CostaPets` / `AgenteCostaPets` del perfil. Único que **escribe**
  la config de seguridad y único que ve la opción `SUPER_ADMIN` en el desplegable de
  perfiles.
- **`ADMIN`**: sus permisos de negocio **salen del rol** (igual que `USUARIO`). Extra:
  `perfil.gestionaUsuarios = true` ⇒ puede gestionar usuarios y **leer** (no editar) la
  pantalla `RolesPermisos`.
- **`USUARIO`**: permisos 100 % del rol.
- **Nombre de la bandera de "acceso total"**: `EsSuperAdministrador` (no `AccesoTotal`).
  En todo este documento, donde diga `AccesoTotal`/`EsAccesoTotal`, léase
  **`EsSuperAdministrador`**. `EsAdministrador` (nombre viejo) queda como alias de
  `EsSuperAdministrador`.
- **Editar catálogo / roles / perfiles**: solo `EsSuperAdministrador`. `gestionaUsuarios`
  habilita solo lectura.
- **Rol obligatorio** al crear/editar usuario **salvo** perfil `SUPER_ADMIN`.
- **Capacidades**: solo lectura en `Usuarios.razor` (vienen del perfil, sin override).
- **`NombrePantalla.cs`**: se elimina. `Pantalla`/`Accione`/`Ventanas` desaparecen del API.
- **Claims**: `seepos:esSuperAdmin` sustituye a `seepos:administrador`.

---

## 3. Cambios de diseño en el sitio

### 3.1 `AccionPantalla` (enum, `Class/`)

```csharp
public enum AccionPantalla { Ver, Crear, Editar, Borrar, Activar, Exportar, Imprimir }
```

(`Modificar` → `Editar` para alinear con el `Codigo` `EDITAR` del API; alias de
compatibilidad si hace falta durante la transición.)

### 3.2 `PermisoPantalla` → por códigos y acciones abiertas

```csharp
public sealed record PermisoFuncion(
    string ModuloCodigo,
    string FuncionCodigo,
    IReadOnlySet<string> Acciones)   // {"VER","CREAR","IMPRIMIR"}
{
    public bool Permite(AccionPantalla a) => Acciones.Contains(a.ToString().ToUpperInvariant());

    // Claim en texto plano (uno por función, ~82 por sesión):
    //   moduloCodigo|funcionCodigo|VER,CREAR,IMPRIMIR
    public string AClaim() => $"{ModuloCodigo}|{FuncionCodigo}|{string.Join(',', Acciones)}";

    public static PermisoFuncion? DesdeClaim(string v) { /* split '|' en 3; acciones por ',' */ }
}
```

`Menu`/`Pantalla` (rótulos) dejan de ser la llave. Se pueden conservar como campos
informativos si alguna UI los muestra, pero **no** para comparar.

### 3.3 `ClaimsSeePos`

```csharp
public const string PerfilCodigo          = "seepos:perfilCodigo";
public const string EsSuperAdministrador  = "seepos:esSuperAdmin";
public const string AccesoTotal           = "seepos:accesoTotal";   // reemplaza "administrador"
public const string Permiso               = "seepos:permiso";        // ahora formato §3.2
// CostaPets / AgenteCostaPets / AceptaConsignacion: se mantienen (valor efectivo del API)
```

### 3.4 `ServicioAutenticacion.ConstruirClaims`

- Lee `auth.perfil` → claims `PerfilCodigo`, `EsSuperAdministrador`, `AccesoTotal`.
- `CostaPets`/`AgenteCostaPets`/`AceptaConsignacion` desde `auth.perfil.*` (el API ya
  manda el valor efectivo; se puede seguir leyendo el nivel raíz por compat).
- Recorre `auth.permisos` (lista aplanada nueva) → un claim `Permiso` por función con
  `PermisoFuncion.AClaim()`.
- Si `auth.perfil.accesoTotal == true` y `permisos` viene vacío: no se generan claims de
  permiso (el contexto concede todo por `AccesoTotal`).

### 3.5 `IContextoSesion` / `ContextoSesion`

```csharp
bool EsAccesoTotal { get; }          // antes EsAdministrador (nombre viejo como alias)
bool EsSuperAdministrador { get; }
string? PerfilCodigo { get; }

bool PuedeVer(string funcionCodigo);
bool Puede(string funcionCodigo, AccionPantalla accion);
bool EstaGobernada(string funcionCodigo);
```

- `Puede`: si `EsAccesoTotal` → `true`. Si hay `PermisoFuncion` para ese `funcionCodigo`
  → `Permite(accion)`. Si no está gobernada → `VerPantallasNoGobernadas` (que, una vez la
  semilla del menú alimente el catálogo del API, se podrá poner en `false`).
- El diccionario interno pasa a estar **cuentado por `FuncionCodigo`** (string exacto,
  `OrdinalIgnoreCase`), sin `NombrePantalla.Comparador`.

### 3.6 `ItemMenu` + `MenuSeePos` — código estable y generación

```csharp
public sealed class ItemMenu
{
    public required string Titulo { get; init; }
    public string? Codigo { get; init; }     // NUEVO: MODULO o MODULO.FUNCION; null en la raíz "solo agrupa"
    public string? Ruta { get; init; }
    public string? Icono { get; init; }
    public IReadOnlyList<ItemMenu> Hijos { get; init; } = Array.Empty<ItemMenu>();
    public bool EsGrupo => Hijos.Count > 0;
}
```

- `MenuSeePos.cs` se **regenera** anotando cada nodo con su `Codigo`, con el **mismo
  algoritmo de slug** que usa el seeder del API (quitar tildes → MAYÚSCULAS → separadores
  a `_`; función = `<MODULO>.<SLUG>`).
- Fuente única: el `seed-seguridad.json` que produce `ApiSuvesaPos/tools/`. Se añade
  `tools/regenerar-menu.*` en este repo que consume ese JSON (o su versión reducida) y
  reescribe `MenuSeePos.cs`. Así **el menú del sitio y el catálogo del API nunca
  divergen**.

### 3.7 `FiltroMenu`

`EsVisible`: admin/acceso total ve todo; grupo visible si algún hijo visible; hoja
visible si `sesion.PuedeVer(item.Codigo)` (antes `item.Titulo`). Si `Codigo` es `null`
(nodo puramente agrupador sin pantalla) se trata como grupo.

### 3.8 Retiro de `NombrePantalla.cs`

Una vez todo casa por `Codigo`, el comparador sin tildes deja de tener sentido. Se borra
el archivo y sus usos (`ContextoSesion`, pruebas). Queda registrado en el `CLAUDE.md` del
sitio como deuda cerrada.

---

## 4. Pantallas

### 4.1 `Views/Parametros/RolesPermisos.razor` (reemplaza `Roles.razor`)

Ruta `/parameters/role`. Mantiene la **compuerta de reconfirmar la propia clave**
(`IRolesPermisos.ValidarPasswordActual`) antes de mostrar nada. Tres pestañas
(`HxTabPanel`), nivel Escritorio:

1. **Catálogo** — árbol Módulo → Función.
   - CRUD módulo (`Codigo`, `Nombre`, `Orden`, `Icono`, `Activo`).
   - CRUD función (`Codigo`, `Nombre`, `Ruta`, `IdFuncionPadre`, `Orden`, `Activo`).
   - Por función: multiselección de **acciones disponibles** (`FuncionAccion`).
   - Solo visible/editable si `Sesion.EsAccesoTotal` (o Super Admin, según §7 del doc API).
2. **Acciones** — CRUD del catálogo global (`VER … IMPRIMIR`). Alta rara; se muestra pero
   con aviso de que afecta a todo el sistema.
3. **Roles** — lista (`AppRejilla`). Al editar/crear un rol:
   - Datos: `Nombre`, `Descripcion`, `Activo`.
   - **Matriz**: filas = funciones agrupadas por módulo (colapsables); columnas = las
     acciones **disponibles** de esa función; checkbox = concesión. "Marcar todo el
     módulo" / "solo VER" como atajos.
   - Guardar → `PUT /seguridad/roles/{id}/permisos` (reemplazo total).

Sustituye el patrón actual "agregar fila por (módulo, pantalla)" por la matriz completa,
que es más rápida de revisar y no deja pantallas fuera por olvido.

### 4.2 `Views/Parametros/Usuarios.razor` (ajustes)

- **Perfil**: desplegable desde `GET /seguridad/perfiles`. La opción cuyo
  `Codigo == "SUPER_ADMIN"` **solo aparece si `Sesion.EsSuperAdministrador`**.
- **Cambiar perfil desde la tabla** (pedido 2): acción "Perfil" por fila → modal pequeño
  → `PUT /seguridad/usuarios/{id}/perfil`. Si el destino es `SUPER_ADMIN` y el usuario en
  sesión no es Super Admin, el botón no se muestra; y si aun así se intenta, el API
  responde error y `IManejadorRespuestas` lo enseña.
- **Rol**: editable al crear (obligatorio si el perfil no es `AccesoTotal`); en edición se
  puede exponer `PUT /seguridad/usuarios/{id}/rol` como acción aparte.
- **Capacidades** (`CostaPets`, `AgenteCostaPets`, `AceptaConsignacion`): **ya no se
  editan por usuario**; se muestran de solo lectura con la nota "definido por el perfil
  X". (Si el API mantiene override nullable — §7 doc API — se añade un tri‑estado
  "Heredar / Sí / No"; decisión pendiente.)
- La edición de **perfiles** (crear/editar, marcar capacidades — pedido 8) vive en la
  pestaña **Catálogo** de `RolesPermisos.razor` o en una mini‑pantalla
  `Views/Parametros/Perfiles.razor`. Recomendado: sub‑sección en `RolesPermisos` para no
  multiplicar pantallas.

---

## 5. Contrato de API (idéntico al del documento del API)

Prefijo `/seguridad`. Envelope `ResponseGeneric<T>`. Todo `[Authorize]`.

### 5.1 Login — `Autenticacion` extendido

```jsonc
{
  "token": "…", "expiracion": "…", "usuario": "achaves", "cantidadVentas": 0,
  "administrador": true, "costaPets": false, "agenteCostaPets": false, "aceptaConsignacion": false,
  "perfil": {
    "codigo": "ADMIN", "nombre": "Administrador",
    "esSuperAdministracion": false, "accesoTotal": true,
    "costaPets": false, "agenteCostaPets": false, "aceptaConsignacion": false
  },
  "rol": { "idRol": 3, "nombre": "Cajero" },
  "permisos": [
    { "moduloCodigo": "INICIO", "moduloNombre": "Inicio",
      "funcionCodigo": "INICIO.FACTURACION", "funcionNombre": "Facturación",
      "acciones": ["VER", "CREAR", "IMPRIMIR"] }
  ]
}
```

Si `perfil.accesoTotal == true`, `permisos` puede venir vacío y el cliente concede todo.

### 5.2 Perfiles

| Método | Ruta | Notas |
|---|---|---|
| GET | `/seguridad/perfiles` | catálogo |
| POST | `/seguridad/perfiles` | crear; requiere llamante `AccesoTotal`; marca capacidades (pedido 8) |
| PUT | `/seguridad/perfiles/{id}` | editar; `SUPER_ADMIN`/`ADMIN`/`USUARIO` no cambian `Codigo` ni se desactivan |

### 5.3 Usuarios

| Método | Ruta | Notas |
|---|---|---|
| POST | `/seguridad/usuarios` | `IdPerfil` obligatorio; asignar `SUPER_ADMIN` exige llamante Super Admin; `IdRol` obligatorio si el perfil no es `AccesoTotal` |
| PUT | `/seguridad/usuarios/{id}` | datos; sin perfil ni rol |
| PUT | `/seguridad/usuarios/{id}/perfil` | `{ idPerfil }`; compuerta Super Admin; auditoría |
| PUT | `/seguridad/usuarios/{id}/rol` | `{ idRol }` |
| POST | `/seguridad/usuarios/{id}/activar` · `/anular` | igual que hoy |
| POST | `/seguridad/usuarios/buscar` · GET `/seguridad/usuarios/{id}` | igual que hoy |

### 5.4 Catálogo (mantenimiento)

| Método | Ruta | Cuerpo |
|---|---|---|
| GET | `/seguridad/modulos` | árbol con funciones anidadas + acciones disponibles |
| POST/PUT/DELETE | `/seguridad/modulos[/{id}]` | `ModuloDTO` (DELETE = baja lógica) |
| GET | `/seguridad/funciones?idModulo=` | lista plana |
| POST/PUT/DELETE | `/seguridad/funciones[/{id}]` | `FuncionDTO` (`idFuncionPadre`) |
| PUT | `/seguridad/funciones/{id}/acciones` | `["VER","EDITAR",…]` reescribe `FuncionAccion` |
| GET | `/seguridad/acciones` | catálogo global |
| POST/PUT/DELETE | `/seguridad/acciones[/{id}]` | `AccionDTO` |

### 5.5 Roles + matriz

| Método | Ruta | Cuerpo / respuesta |
|---|---|---|
| GET | `/seguridad/roles` | `RolResumenDTO[]` |
| GET | `/seguridad/roles/{id}` | `RolDetalleDTO` (matriz: por función `accionesDisponibles[]` + `accionesConcedidas[]`) |
| POST | `/seguridad/roles` | `RolDetalleDTO` |
| PUT | `/seguridad/roles/{id}` | `RolDetalleDTO` (datos) |
| PUT | `/seguridad/roles/{id}/permisos` | `[{ funcionCodigo, acciones:[...] }]` reemplazo total |

### 5.6 DTOs (los usará NSwag; nombres orientativos)

`PerfilDTO`, `ModuloDTO`, `FuncionDTO`, `AccionDTO`, `RolResumenDTO`,
`RolFuncionPermisoDTO`, `RolDetalleDTO`, `PermisoLoginDTO`. Ver el documento del API §5.6
para las firmas.

---

## 6. Proxies del sitio

### 6.1 `IRolesPermisos` (reemplaza `IRoles`)

```csharp
Task<ResponseGeneric<ICollection<RolResumenDTO>>> Roles();
Task<ResponseGeneric<RolDetalleDTO>>              Rol(int idRol);
Task<ResponseGeneric<bool>>                       CrearRol(RolDetalleDTO dto);
Task<ResponseGeneric<bool>>                       EditarRol(int idRol, RolDetalleDTO dto);
Task<ResponseGeneric<bool>>                       GuardarPermisos(int idRol, IEnumerable<RolFuncionPermisoDTO> filas);

Task<ResponseGeneric<ICollection<ModuloDTO>>>     Modulos();          // árbol con funciones
Task<ResponseGeneric<bool>>  GuardarModulo(ModuloDTO m);
Task<ResponseGeneric<bool>>  GuardarFuncion(FuncionDTO f);
Task<ResponseGeneric<bool>>  GuardarAccionesFuncion(int idFuncion, IEnumerable<string> acciones);
Task<ResponseGeneric<ICollection<AccionDTO>>>     Acciones();
Task<ResponseGeneric<bool>>  GuardarAccion(AccionDTO a);

Task<ResponseGeneric<Usuario>> ValidarPasswordActual(string contrasena);  // se conserva
```

### 6.2 `IPerfiles` (nuevo) y ajustes a `IUsuarios`

```csharp
// IPerfiles
Task<ResponseGeneric<ICollection<PerfilDTO>>> Listar();
Task<ResponseGeneric<bool>> Crear(PerfilDTO p);
Task<ResponseGeneric<bool>> Editar(int id, PerfilDTO p);

// IUsuarios (+)
Task<ResponseGeneric<bool>> CambiarPerfil(long id, int idPerfil);
Task<ResponseGeneric<bool>> CambiarRol(long id, int idRol);
// ObtenerPerfiles() del interfaz actual -> delega en IPerfiles.Listar()
```

Todos heredan de `ProxyBase` y traducen el envelope con `EnvelopeApi.A(...)` (arquetipo
de la casa; una View nunca ve una `ApiException`).

---

## 7. Pruebas

### 7.1 Unitarias (`tests/SuvesaPosSitioAplicacion.Tests`)

- `FiltroMenuTests` (ya existe): reescribir para códigos. Casos: hoja sin permiso oculta;
  grupo con un hijo visible se muestra; `AccesoTotal` ve todo; nodo `Codigo == null`
  agrupador.
- `PermisoFuncionTests`: `AClaim()`/`DesdeClaim()` ida y vuelta; acción no listada → `false`;
  claim mal formado → `null`.
- `ContextoSesionTests`: `Puede` con/ sin `AccesoTotal`; `EsSuperAdministrador` desde claim;
  `EstaGobernada` por código.
- `MenuVsCatalogoTests`: cada `ItemMenu.Codigo` no nulo cumple el formato del slug y es
  único. (Comparación contra el JSON de semilla si está disponible como recurso.)

### 7.2 E2E (`tests/SuvesaPosSitioAplicacion.E2E`)

- **Actualizar** la prueba que hoy compara `NombrePantalla` del API con títulos de menú:
  ahora compara **`funcionCodigo` del API** con **`ItemMenu.Codigo`** del sitio. Debe
  pasar de "informa el desfase" a **aserción** (era el objetivo pendiente del `CLAUDE.md`).
- `SesionE2ETests` (necesita `SEEPOS_USUARIO`/`PASSWORD`): login trae `perfil` + `permisos`;
  un rol no‑admin ve el menú filtrado; el administrador ve todo.
- Nueva `RolesPermisosE2ETests` (con credenciales de admin): compuerta de clave;
  crear rol → conceder `VER`+`CREAR` en una función → releer y ver los checks; quitar
  `VER` oculta la función en el menú de ese rol.
- Nueva `SuperAdminGateE2ETests`: con usuario no‑super, la opción `SUPER_ADMIN` no está en
  el desplegable y `PUT …/perfil` a `SUPER_ADMIN` devuelve error mostrado por
  `IManejadorRespuestas`.

**Regla de la casa**: la suite E2E tiene que estar verde antes de replicar pantallas en
serie.

---

## 8. Plan de trabajo (lado sitio)

Cada fase = rama, PR ≤ 1 semana, `dotnet build` + unitarias verdes. Las fases W depende de
que el API tenga listas las suyas (ver documento del API §9): **W1 necesita A1–A6**.

| Fase | Entregable | Detalle |
|---|---|---|
| **W1** | Regenerar contratos NSwag | `./tools/actualizar-contratos.sh` contra el API con `/seguridad/*`. Aparecen los DTOs nuevos en `DTOs/Generated`. |
| **W2** | Núcleo de permisos por código | `AccionPantalla` (+3 acciones); `PermisoFuncion` (reemplaza `PermisoPantalla`); `ClaimsSeePos` (`PerfilCodigo`, `EsSuperAdministrador`, `AccesoTotal`); `ServicioAutenticacion.ConstruirClaims` lee `perfil` + `permisos`. |
| **W3** | `ContextoSesion` + menú con código | `IContextoSesion`/`ContextoSesion` por `funcionCodigo`; `ItemMenu.Codigo`; `tools/regenerar-menu.*` + `MenuSeePos.cs` regenerado; `FiltroMenu` por código. Borrar `NombrePantalla.cs`. |
| **W4** | Proxies | `IRolesPermisos` + `Roles`/`RolesPermisos` proxy; `IPerfiles` + proxy; ajustes `IUsuarios`. Registro en DI. |
| **W5** | Pantalla `RolesPermisos.razor` | 3 pestañas (Catálogo / Acciones / Roles) + matriz. Compuerta de clave conservada. Sub‑sección Perfiles (o `Perfiles.razor`). Retira `Roles.razor`. |
| **W6** | `Usuarios.razor` | Desplegable de perfil (oculta `SUPER_ADMIN` salvo Super Admin); acción "Perfil" por fila; capacidades solo lectura; rol obligatorio al crear según perfil. |
| **W7** | Pruebas | §7: unitarias reescritas + E2E nuevas; convertir la comparación menú↔API en aserción. |
| **W8** | Endurecer | `SeePos:VerPantallasNoGobernadas = false` por defecto; actualizar `CLAUDE.md` (deuda `NombrePantalla` cerrada, permisos por código, 7 acciones). |

Dependencias: W1→W2→W3→W4→(W5,W6)→W7→W8.

---

## 9. Índice de archivos que se tocarán (sitio)

```
src/SuvesaPosSitioAplicacion/
  Class/
    AccionPantalla.cs        (+ Editar/Activar/Exportar/Imprimir)
    MenuSeePos.cs            REGENERADO (con Codigo por nodo)
  Models/
    ItemMenu.cs             (+ Codigo)
  Security/
    ClaimsSeePos.cs         (+ PerfilCodigo, EsSuperAdministrador, AccesoTotal)
    PermisoPantalla.cs      -> PermisoFuncion.cs (por códigos, acciones abiertas)
    NombrePantalla.cs       BORRADO
    IContextoSesion.cs / ContextoSesion.cs   (API por funcionCodigo; EsSuperAdministrador)
    FiltroMenu.cs           (casa por Codigo)
  Services/
    ServicioAutenticacion.cs   (ConstruirClaims: perfil + permisos aplanados)
  ApiConexion/ProxyInterface/
    IRolesPermisos.cs  NUEVO (reemplaza IRoles.cs)
    IPerfiles.cs       NUEVO
    IUsuarios.cs       (+ CambiarPerfil / CambiarRol)
  ApiConexion/ProxyClass/
    RolesPermisos.cs   NUEVO (reemplaza Roles.cs)
    Perfiles.cs        NUEVO
    Usuarios.cs        (métodos nuevos)
  Views/Parametros/
    RolesPermisos.razor  NUEVO (reemplaza Roles.razor)
    Perfiles.razor       NUEVO (opcional; o sub-sección en RolesPermisos)
    Usuarios.razor       (perfil desde grid, gate Super Admin, capacidades solo lectura)
  Program.cs             (DI de proxies nuevos)
tools/
  regenerar-menu.*      NUEVO (consume seed-seguridad.json del API)
tests/
  SuvesaPosSitioAplicacion.Tests/   (FiltroMenuTests reescrito; PermisoFuncionTests; ContextoSesionTests)
  SuvesaPosSitioAplicacion.E2E/     (menú↔API como aserción; RolesPermisos; SuperAdminGate)
docs/
  REDISENO_SEGURIDAD_USUARIOS_ROLES_WEB.md   (este archivo)
CLAUDE.md               (actualizar: permisos por código, 7 acciones, NombrePantalla cerrado)
```

---

## 10. Preguntas a confirmar (mismas que el documento del API)

1. Pedido 1: ¿el Perfil es el único selector al crear el usuario, o "solo un admin puede
   crear usuarios", o ambas?
2. ¿`ADMIN` y `SUPER_ADMIN` comparten `AccesoTotal` (ven todo) y solo cambian en la
   compuerta + edición de catálogo?
3. ¿Editar catálogo lo puede cualquier `AccesoTotal` o solo `SUPER_ADMIN`?
4. Capacidades: ¿herencia pura del perfil, o override por usuario (tri‑estado en la UI)?
5. ¿`Pantalla`/`Accione`/`Ventanas` se retiran o algún otro consumidor las necesita?
6. Enforcement server‑side del API: ¿ahora o fase posterior? (cambia cuánto puede
   simplificarse `ContextoSesion`).
7. ¿El perfil `USUARIO` siempre exige rol?
```
