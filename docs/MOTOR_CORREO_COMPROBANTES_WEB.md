# Motor de correo de comprobantes — Análisis y plan de trabajo (Sitio Web)

> Parte de sitio del motor de correo. El API (repo `DevSuvesaPosWeb`, rama
> `feature/bonificaciones`) hace el envío automático; la web aporta **3
> pantallas**: (1) configuración SMTP por emisor, (2) bandeja de **envíos de
> correo** de comprobantes, (3) bandeja de **alertas del administrador**
> (rechazos de Hacienda y fallos de envío).
>
> Repo web, rama `feature/ola-0-cimientos`. **Solo análisis y plan; no toca código.**
> Documento hermano (API): `../DevSuvesaPosWeb/docs/MOTOR_CORREO_COMPROBANTES_API.md`.

---

## 1. Estado actual (web)

### 1.1 Lo que hay
- **No existe** pantalla de configuración de correo ni de alertas.
- Bandeja unificada de documentos: `Views/Documentos/Bandeja.razor` +
  `ApiConexion/ProxyClass/BandejaDocumentos.cs` (patrón `ProxyBase`,
  `IHttpClientFactory.CreateClient("SeePosApi")`, `Envelope<T>`,
  `JsonSerializerOptions(JsonSerializerDefaults.Web)`). Muestra estado en Hacienda
  y mensaje de rechazo por documento — **buen anclaje visual** para “este
  comprobante ya se envió por correo / falló / se omitió”.
- Emisores: `Views/Parametros/EmisoresFiscal.razor(.cs)` — modal por secciones;
  ya oculta credenciales Hacienda al reabrir (patrón *blank-on-reopen*).
- Series: `Views/Parametros/SeriesFacturacionFiscal.razor`.
- `IServicioDialogos` (`ConfirmarAsync`, `Exito`, `ErrorAsync`, …),
  `IManejadorRespuestas.CorrectaAsync` (pinta `validationErrors`).
- Sin sistema de notificaciones/toasts push ni SignalR (solo toasts de Bootstrap
  para feedback inmediato).
- Menú `Class/MenuSeePos.cs`; códigos ⊆ `tests/…/Fixtures/seed-seguridad.json`
  (byte-idéntico al del API). Tests: `MenuCodigosTests`, `FiltroMenuTests`
  (`Assert.Equal(12, MenuSeePos.Items.Count)` + conteo total —
  **hoy 78**, subir al añadir hojas), `SeedSeguridadTests` (`>= 60`).
- PDF: `Services/IGeneradorPdf` + `GeneradorPdfQuestPdf` (QuestPDF 2026.8.0),
  hoy solo reportes tabulares.

### 1.2 Lo que falta (mapa de vacíos, se resuelven abajo)
| # | Vacío | Resolución |
|---|---|---|
| W1 | No hay UI para capturar SMTP host/puerto/SSL/usuario/contraseña por emisor. | Pantalla `ConfiguracionCorreoFiscal.razor` (§3). |
| W2 | La contraseña no debe volver del API ni mostrarse. | Campo password vacío al abrir; “dejar en blanco = no cambiar” (igual que Hacienda). |
| W3 | No hay forma de probar la configuración. | Botón **“Enviar correo de prueba”** → `POST …/probar`. |
| W4 | El usuario no ve el estado de envío de cada comprobante. | Bandeja de envíos + columna en la Bandeja unificada. |
| W5 | Las alertas de Hacienda/SMTP no llegan a ningún lado en la web. | Bandeja de alertas + **badge** en el layout para perfil administrador. |
| W6 | El administrador no tiene un “campana/contador”. | `GET /api/alertas-administrador/conteo` cada N min → badge en `App.razor`. |
| W7 | Reenvío manual. | Botón **“Reenviar”** en la bandeja de envíos (gateado por permiso). |

---

## 2. Requerimiento (parte web)
2. La **configuración de email** (SMTP, puerto, si requiere SSL, usuario,
   contraseña) **se realiza desde el sitio web**.
4. Que **llegue alerta al usuario administrador** sobre los problemas reportados
   por Hacienda (rechazos) — visible en la web.

### 2.bis Decisión confirmada (2026-09-03)
- **D3** — el correo lleva **PDF adjunto** (además de los 2 XML) desde la fase 1.
  En la web: la acción **“Ver detalle”** de la bandeja de envíos lista siempre los
  3 adjuntos (XML firmado · XML respuesta · PDF) y marca cuáles se incluyeron
  (`AdjuntoPdf = 0` si el render falló, con su alerta).

---

## 3. Pantalla 1 — Configuración de correo por emisor

**Ruta** `/parameters/mail-settings` · **Código menú**
`PARAMETROS.CONFIGURACION_CORREO` · bajo **Parámetros**.
Componente `Views/Parametros/ConfiguracionCorreoFiscal.razor(.cs)`.

- Selector de **Emisor** (reutiliza catálogo de emisores). Al elegir →
  `GET /api/configuracion-correo/{idEmisor}`.
- Formulario (`AppCampoTexto` / switches Havit):
  - **Servidor SMTP** (`SmtpHost`) · **Puerto** (`SmtpPuerto`, número).
  - Switch **“Requiere SSL/TLS”** (`UsaSsl`).
  - **Usuario** (`Usuario`) · **Contraseña** (`type=password`, placeholder
    “•••• (sin cambios)”, vacío = no cambiar; muestra “contraseña configurada”
    si `ContrasenaAsignada`).
  - **Nombre remitente** (`RemitenteNombre`) · **Correo remitente**
    (`RemitenteCorreo`).
  - **Copia oculta (BCC)** (`CopiaOculta`, lista separada por `;`).
  - Switch **“Motor de correo habilitado”** (`Habilitado`).
  - Switch **“Notificar rechazos también por correo al emisor”**
    (`AlertarPorCorreo`), opcional.
  - Colapsable **“Personalizar asunto y cuerpo”**: `AsuntoPlantilla`,
    `CuerpoPlantilla` (textarea), con ayuda de tokens disponibles
    (`{tipo}`, `{consecutivo}`, `{clave}`, `{emisor}`, `{receptor}`).
- Botones: **Guardar** (`PUT`), **Enviar correo de prueba** (abre mini-modal
  pidiendo destino → `POST …/probar` → `IServicioDialogos.Exito` / `ErrorAsync`
  con el detalle).
- Validación cliente mínima (host no vacío, puerto 1..65535, correo remitente con
  formato) + `IManejadorRespuestas.CorrectaAsync` para los `validationErrors` del
  API.
- Gate: `Sesion.EsAdministrador` (o permiso `PARAMETROS.CONFIGURACION_CORREO` con
  acción Modificar).

**Proxy** `IConfiguracionCorreo` / `ConfiguracionCorreo : ProxyBase`:
`Obtener(idEmisor)`, `Guardar(dto)`, `Probar(idEmisor, destino)`.
**DTO** `ConfiguracionCorreoFiscalDTO` en `DTOs/Fiscal/` (partial + `[JsonPropertyName]`
si hay choque con generados; **no** regenerar NSwag completo).

---

## 4. Pantalla 2 — Bandeja de envíos de correo

**Ruta** `/documents/mail-outbox` · **Código** `DOCUMENTOS.ENVIOS_CORREO`
(o pestaña nueva dentro de `Bandeja.razor`). Componente
`Views/Documentos/EnviosCorreo.razor`.

- Filtros (`AppFiltros`): estado (`Pendiente/Enviando/Enviado/Fallido/Omitido sin
  destinatario/Omitido por rechazo`), emisor, rango de fechas, clave/consecutivo.
- Rejilla (`AppRejilla`, server-side): Fecha creación · Tipo · Consecutivo ·
  Clave · Destinatarios · Estado (badge de color) · Intentos · Último error ·
  Fecha de envío.
- Acción **“Reenviar”** (solo estados `Fallido` / `Omitido…`, gateada) →
  `POST /api/envios-correo/comprobantes/{clave}/reenviar` → refresca fila.
- Acción **“Ver detalle”** (offcanvas): adjuntos incluidos (XML firmado / XML
  respuesta / PDF), historial de intentos.
- **Proxy** `IEnviosCorreo` → `Listar(filtro)`, `Reenviar(clave)`.

**Integración con la Bandeja unificada** (`Bandeja.razor`, pestañas Facturas y
Notas de Crédito): añadir columna **“Correo”** (estado del envío: —, Pendiente,
Enviado ✓, Fallido ✗, Omitido) resuelta con el mismo `Listar` (o un endpoint
`?claves=` batch). Así el usuario ve en un solo lugar Hacienda + correo.

---

## 5. Pantalla 3 — Alertas del administrador

**Ruta** `/initial/alerts` · **Código** `INICIO.ALERTAS` · bajo **Inicio**.
Componente `Views/Inicio/Alertas.razor`.

- Rejilla: Fecha · Tipo (`Comprobante rechazado` / `Envío de correo fallido` /
  `Configuración de correo inválida`) · Emisor · Clave · Título · Detalle ·
  Leída.
- Filtro **“Solo no leídas”** (por defecto ON).
- Acción **“Marcar como leída”** (fila) y **“Marcar todas”** →
  `POST /api/alertas-administrador/{id}/marcar-leida`.
- Fila de **comprobante rechazado** con enlace a la Bandeja unificada filtrada
  por esa clave (para ver el mensaje de Hacienda completo y decidir reemisión).
- Gate: `Sesion.EsAdministrador`.
- **Proxy** `IAlertasAdministrador` → `Listar(soloNoLeidas, idEmisor)`,
  `Conteo()`, `MarcarLeida(id)`.

### 5.1 Badge en el layout
En `App.razor` (o el header del layout), para `Sesion.EsAdministrador`:
un ícono `bi-bell` con contador que llama `GET /api/alertas-administrador/conteo`
al cargar y con un `PeriodicTimer` / `System.Timers.Timer` cada 2–5 min
(sin SignalR). Click → navega a `/initial/alerts`.

---

## 6. Menú y semilla

`Class/MenuSeePos.cs` — añadir hojas:
- **Parámetros → Configuración de correo** (`PARAMETROS.CONFIGURACION_CORREO`).
- **Inicio → Alertas** (`INICIO.ALERTAS`).
- **Documentos → Envíos de correo** (`DOCUMENTOS.ENVIOS_CORREO`) *o* pestaña en
  Bandeja (sin código nuevo).

`tests/SuvesaPosSitioAplicacion.Tests/Fixtures/seed-seguridad.json` — añadir las
funciones (edición **quirúrgica**, byte-idéntico al seed del API). Subir el
conteo en `FiltroMenuTests` (`Contar(...)`), revisar `MenuCodigosTests`,
`SeedSeguridadTests`.

---

## 7. Checklist Web

- [ ] **1. Proxies** `IConfiguracionCorreo`, `IEnviosCorreo`,
      `IAlertasAdministrador` (`: ProxyBase`, `Ejecutar`/`Leer<T>`,
      `CreateClient("SeePosApi")`), registrados en `Program.cs` (`AddScoped`).
- [ ] **2. DTOs** en `DTOs/Fiscal/` y `DTOs/Correo/` (partials con
      `[JsonPropertyName]`; sin regen NSwag completo).
- [ ] **3. Pantalla configuración** `ConfiguracionCorreoFiscal.razor(.cs)` +
      modal de prueba + validación + blank-on-reopen de la contraseña.
- [ ] **4. Bandeja de envíos** `EnviosCorreo.razor` (o pestaña en
      `Bandeja.razor`) + acción Reenviar.
- [ ] **5. Columna “Correo”** en las pestañas Facturas / Notas de Crédito de
      `Bandeja.razor`.
- [ ] **6. Pantalla de alertas** `Alertas.razor` + marcar leída(s).
- [ ] **7. Badge** de alertas en el layout para administrador (polling, sin
      SignalR).
- [ ] **8. Menú + semilla** (§6) + ajustar tests de menú/semilla.
- [ ] **9. Permisos**: gates `Sesion.EsAdministrador` / `Sesion.Puede(...)` en
      las 3 pantallas y en las acciones.
- [ ] **10. Pruebas**
      `dotnet test tests/SuvesaPosSitioAplicacion.Tests/...` (hoy 72) — sumar
      casos de proxy (deserialización `Envelope<T>`, manejo de error) y el conteo
      de menú.
- [ ] **11. Build** `dotnet build src/SuvesaPosSitioAplicacion/... -v q`.
- [ ] **12. Docs** — actualizar este archivo con lo decidido; referencia cruzada
      desde `docs/BANDEJA_DOCUMENTOS_WEB.md`.

---

## 8. Preguntas abiertas
1. ¿Bandeja de envíos como **pantalla nueva** o **pestaña** dentro de la Bandeja
   unificada? (El plan soporta ambas; recomendado: pestaña + columna “Correo”.)
2. Frecuencia del polling del badge de alertas (2 min / 5 min).
3. ¿El selector de emisor en la config de correo debe respetar la sucursal/emisor
   de la sesión o permitir cualquiera al administrador?
4. ¿Se quiere edición del **asunto/cuerpo** en esta entrega o se deja para cuando
   exista el motor de plantillas (que podría también plantillar el correo)?
