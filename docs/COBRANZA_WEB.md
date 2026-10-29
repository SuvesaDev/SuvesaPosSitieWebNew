# Cobranza — pantallas del sitio (Blazor)

Cliente Blazor del módulo de **Gestión y seguimiento de cobranzas** del API
(`DevSuvesaPosWeb/ApiSuvesaPos/docs/MODULO_COBRANZAS_API.md` y `PLAN_MODULO_COBRANZAS_API.md`).
Las reglas de negocio viven en el API; el sitio solo captura, valida lo evidente y muestra.

## Pantallas

Módulo de menú **Cobranza** (`COBRANZA`, ícono `bi-telephone-outbound`), tres hojas:

| Menú | Ruta | Código de permiso | Qué hace |
|---|---|---|---|
| Seguimiento | `/collections/follow-up` | `COBRANZA.SEGUIMIENTO` | Pestaña **Facturas vencidas** (filtros: mora desde/hasta, cliente, cobrador, gestión) para abrir un seguimiento con una o varias facturas del mismo cliente; pestaña **Seguimientos** (estado, "solo los míos"). |
| — (detalle) | `/collections/follow-up/{id}` | `COBRANZA.SEGUIMIENTO` | Resumen, **nueva interacción**, **promesa de pago**, facturas, promesas y **bitácora en orden cronológico inverso**. Reasignar si tiene `COBRANZA.GERENCIAL` + Editar. |
| Dashboard | `/collections/dashboard` | `COBRANZA.DASHBOARD` | Tareas del día, promesas pendientes e incumplidas del cobrador autenticado. |
| Gerencial | `/collections/management` | `COBRANZA.GERENCIAL` | KPIs por colaborador en un rango de fechas. |

### Comportamiento

- **Registrar una interacción en menos de 3 clics:** el tipo (Llamada) y el resultado (Contactado) vienen
  preseleccionados; el cobrador escribe la observación y pulsa *Registrar*. El botón se habilita al llegar a
  10 caracteres (`PresentacionCobranza.MinimoObservacion`, el mismo mínimo del API).
- **Observación obligatoria** en cada interacción y en la apertura del seguimiento (lo conversado con el cliente).
- **Rojo para lo incumplido:** en el listado, la fila de una factura cuyo seguimiento tuvo una promesa
  incumplida (`table-danger` + distintivo); en el detalle, la promesa incumplida; en el dashboard, la tarjeta
  y la tabla de incumplidas.
- **Una factura, un seguimiento activo:** las que ya tienen seguimiento no se pueden seleccionar y muestran
  quién las tiene (enlace al detalle). Si otro cobrador la toma mientras tanto, el API responde con el
  conflicto, la pantalla lo muestra y recarga el listado.
- **Solo gestiona quien tiene asignado el seguimiento:** los demás ven un aviso con el nombre del responsable.
  La reasignación (administración) queda en la bitácora.
- **Selección múltiple** con las mismas reglas del API (mismo cliente y misma moneda), avisadas antes de enviar.
- Los dashboards se recalculan en el API como máximo cada 5 minutos; *Actualizar* no fuerza el cálculo.

## Capas

| Pieza | Archivo |
|---|---|
| DTOs a mano + reglas de presentación | `DTOs/Cobranza/CobranzaWebDTOs.cs` (`PresentacionCobranza`) |
| Interfaz y proxy | `ApiConexion/ProxyInterface/ICobranza.cs`, `ApiConexion/ProxyClass/Cobranza.cs` (registrado en `Program.cs`) |
| Pantallas | `Views/Cobranza/` (`Seguimiento`, `DetalleSeguimiento`, `DashboardCobrador`, `Gerencial`) |
| Menú | `Class/MenuSeePos.cs` (nodo `Cobranza`) |
| Pruebas | `CobranzaProxyTests.cs`, `PresentacionCobranzaTests`; `FiltroMenuTests` actualizado (12 módulos, 128 nodos) |

El proxy sigue el patrón de `CertificadosRegistro`: cliente HTTP a mano (`SeePosApi`) + `LecturaEnvelope`,
sin lanzar excepciones hacia las vistas. Los DTOs son un espejo **a mano** porque los contratos NSwag no
incluyen todavía `api/cobranza`; cuando se regeneren (`./tools/actualizar-contratos.sh`), se pueden sustituir.
El API serializa los enums como **número**; los enums del sitio llevan los mismos valores (hay una prueba).

## Permisos y semilla

Los códigos `COBRANZA`, `COBRANZA.SEGUIMIENTO`, `COBRANZA.DASHBOARD` y `COBRANZA.GERENCIAL` están en la
semilla del API (`SecuritySystem/Seed/seed-seguridad.json`), con los **rótulos iguales a los títulos del menú**
("Seguimiento", "Dashboard", "Gerencial"), porque `tools/anotar_codigos_menu.py` deriva el código del título.
`MenuCodigosTests` comprueba que el menú y la semilla no diverjan (la copia de la semilla se refresca al
compilar las pruebas). Falta **asignar los permisos a los roles** de cobro y de gerencia desde *Roles y permisos*.

## Pendiente

- **Verificación visual con datos reales:** compilado y probado con pruebas unitarias, **pero no se recorrieron
  las pantallas en un navegador** contra un API con la migración aplicada. Es lo primero a hacer en pruebas.
- **Agregar facturas a un seguimiento existente:** el API lo soporta (`POST seguimientos/{id}/facturas`); la
  pantalla todavía no lo ofrece.
- **Filtro de cliente por nombre:** hoy es por identificación numérica.
- **Corrección de notas** (`IdCorrige`): el API lo soporta y la bitácora lo muestra; no hay botón "corregir".
- **Fechas** de la bitácora: se muestran en la hora del servidor del sitio (las marcas vienen en UTC).
- **E2E (Playwright):** sin cubrir; requiere usuario y datos de prueba.
