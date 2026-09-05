# Plan sitio web - Plantillas PDF más profesionales

Fecha: 2026-09-05. Estado: análisis y plan; no implementado.

Este documento parte de la comparación visual entre el PDF actual de SeePOS (`50604092600011278028600100150010000000021101002916.pdf`) y una referencia externa (`FE-50602092600310288677700100001010000001271147289284.pdf`). Los adjuntos sirven como muestra de resultado, no como instrucciones incorporadas al sistema.

Plan hermano de generación y datos: `DevSuvesaPosWeb/docs/MEJORA_DISENO_PDF_PROFESIONAL_API.md`.

## 1. Resultado deseado para el usuario

El sitio no debe intentar componer documentos fiscales en el navegador. Debe permitir que un administrador configure una plantilla profesional de manera segura, la previsualice con fidelidad, y que cajeros, agentes y personal administrativo abran/impriman el PDF correcto desde cada operación.

La experiencia objetivo es:

1. El administrador elige emisor, tipo de documento, serie cuando aplique y formato A4/térmico.
2. Selecciona un preset profesional y ajusta composición, bloques y campos permitidos; el logo ya viene del emisor seleccionado.
3. Visualiza un PDF de muestra antes de activar la plantilla, sin afectar documentos reales.
4. Define la plantilla predeterminada por ámbito.
5. El usuario de ventas abre una factura, tiquete, recibo o boleta y ve/descarga el PDF existente; reimprimir no vuelve a facturar ni a cobrar.
6. La pantalla explica si el PDF es interno, fiscal pendiente, fiscal aceptado, original o copia, sin confundir la representación gráfica con la emisión ante Hacienda.

## 2. Estado actual observado

| Elemento | Estado actual | Mejora necesaria |
|---|---|---|
| Página de plantillas | Existe en `Views/Parametros/PlantillasImpresion.razor` | Tiene estructura útil, pero expone configuración técnica extensa y no ofrece presets, tema visual, QR, monto en letras ni validación de diseño |
| Emisor | `EmisorFiscalDTO` y la pantalla de Emisores no tienen una propiedad ni carga de logo | Incorporar el logo como activo del emisor; no llevarlo ni duplicarlo en la plantilla |
| Editor | Zonas en acordeón: logo, receptor, meta, detalle, totales, pie, márgenes/fuente | Evolucionar a decisiones de negocio y diseño guiado; el bloque logo solo controla visibilidad/posición/tamaño y siempre obtiene la imagen del emisor elegido |
| Vista previa | Usa PDF en `<object>` tras guardar la plantilla | Permitir previsualización de borrador no guardado, datos de muestra representativos y revisión A4/térmico; mostrar alertas de accesibilidad/recortes |
| Configuración de tipo | `TiposImpresionUi` es un espejo manual de 15 tipos | Mantenerlo sincronizado con API mediante contrato/catálogo; evitar que un nuevo tipo se visualice como “Tipo 16” |
| Selección de formato | A4 y térmico están disponibles | Explicar que son composiciones distintas; no mostrar controles A4 irrelevantes para rollo ni suponer que 80 mm sirve para 58 mm |
| PDF de documentos | `/documentos/{tipo}/{id}/pdf` delega la generación al API | Conservar el BFF y el token en servidor; corregir UX de visualización/descarga, mensajes y estado, no duplicar render en Blazor |
| Botón reusable | `BotonImprimir.razor` abre PDF local en pestaña nueva | Añadir estado, formato, copia y accesibilidad de forma consistente; no usarlo para lanzar una segunda operación de venta |
| Vistas de negocio | Algunos recibos/órdenes usan `window.open`; otros muestran base64 | Unificar un visor/componente y aplicarlo a los tipos que ya soporta el motor |
| Seguridad | `AppPantalla` tiene permiso para plantilla y proxy conserva token del API | Mantener permisos de diseño separados de VER/IMPRIMIR; no exponer PDF, XML o token en URLs/JS |

El PDF actual permite ver por qué esta mejora importa: su tabla es demasiado densa, la identidad visual no se expresa y el total no atrae la mirada. La pantalla de plantilla ya resuelve parte de la configuración, por lo que el plan recomienda pulirla en vez de construir otro editor.

## 3. Diseño de la interfaz de plantillas

### W1. Presets primero

Al crear una plantilla, el flujo debe comenzar por un preset visual, no por un formulario vacío:

- **Corporativo A4**: logo, cabecera a dos columnas, cliente, detalle con cabecera contrastada, total destacado, pie legal y QR opcional.
- **Minimal A4**: monocromático, más compacto, apropiado para impresión láser económica.
- **Tiquete térmico 80 mm**: identidad reducida, detalle compacto, total prominente y clave/QR cuando corresponda.
- **Tiquete térmico 58 mm**: versión de muy pocas columnas, sin intentar escalar el 80 mm.

Al elegir uno, el editor carga parámetros seguros. El administrador modifica únicamente:

- Datos de marca autorizados; el logo se administra exclusivamente desde el emisor.
- Paleta dentro de colores permitidos y con indicador de contraste.
- Visibilidad y etiquetas de bloques/campos semánticos.
- Perfil de columnas recomendado por tipo y formato.
- Leyendas, notas bancarias y textos opcionales aprobados.
- Posición de QR, monto en letras, pie legal y original/copia cuando el tipo lo permita.

No ofrecer HTML, CSS, carga de fuentes arbitrarias, scripts, URLs externas ni un lienzo de arrastrar/soltar. El API conserva la validación final y el usuario mantiene un diseño útil sin riesgo de ocultar información fiscal.

### W1a. Logo administrado desde Emisores

La pantalla de Emisores (`Views/Parametros/EmisoresFiscal.razor`) será el único lugar para cargar, reemplazar, previsualizar o retirar la imagen corporativa.

- En “Datos públicos” del emisor, agregar una sección **Logo para documentos**: miniatura, estado “sin logo”, nombre/formato/dimensiones, fecha de actualización, acción de cargar/reemplazar y eliminación con confirmación.
- La carga envía el archivo al endpoint específico del emisor; la lista de emisores solo consume metadatos como `TieneLogo`, no binarios Base64. La miniatura se solicita con autorización y bajo demanda.
- Validar antes de enviar tamaño, formato y dimensiones permitidos; después mostrar el resultado validado por API. La interfaz no debe fingir que la imagen está guardada porque el navegador pudo leerla.
- La pantalla de Plantillas muestra “Logo del emisor: disponible/no configurado” y un enlace a administrar el emisor. Sus únicos controles son “Mostrar logo”, composición, alineación y altura; se elimina cualquier selector, carga u override de logo dentro de plantilla.
- Si una plantilla usa un emisor sin logo, permitir guardar el diseño, pero advertir claramente en vista previa y bloquear la activación solo si la política de la empresa exige logo obligatorio.
- El permiso de editar plantillas no concede cargar o reemplazar logo. La acción se rige por el permiso de editar el emisor y su ámbito; previsualizar una plantilla solo puede usar el logo del emisor autorizado.
- Cuando se cambie el logo, informar que aplica a nuevos documentos. Los documentos fiscales ya emitidos/reimpresos se resuelven desde su snapshot/PDF histórico definido por API, no desde una imagen local que el sitio conserve.

### W2. Editor por bloques, no por claves internas

Reemplazar progresivamente los listados de `clave`, `orden` y `anchoRel` por una interfaz con nombres visibles y ayudas:

| Bloque | Controles de interfaz |
|---|---|
| Marca y encabezado | Estado y miniatura del logo oficial del emisor, variante de cabecera, color de acento, razón social/contacto, alineación y vista de altura real. La imagen se administra en Emisores |
| Documento | Número, clave, fechas, condición, vencimiento, plazo, medios de pago, estado de copia y reglas de visibilidad |
| Cliente | Identificación, nombre, contacto, dirección, orden de compra y etiquetas |
| Detalle | Perfil “comercial”, “compacto”, “fiscal detallado” o térmico; columnas permitidas y advertencia al exceder el ancho |
| Totales | Filas visibles, color/estilo de total, moneda y monto en letras |
| Notas | Observaciones de la venta, instrucciones bancarias por emisor/moneda/serie, políticas comerciales; mostrar solo si existen |
| Verificación y pie | QR, resolución, versión, datos de consulta, numeración de páginas y marca original/copia |

El modo avanzado puede conservar reordenamiento y anchuras, pero debe mostrar una miniatura y límites claros. Cualquier cambio incompatible se señala antes de guardar: por ejemplo, ocho columnas visibles en 58 mm, bajo contraste, ausencia de total, logo desproporcionado o pie legal requerido desactivado.

### W3. Previsualización útil y segura

- Previsualizar el borrador actual sin guardar ni activar la plantilla. La solicitud llega al API con configuración temporal y vuelve como PDF; no persiste ni genera bitácora.
- Ofrecer escenarios: factura contado corta, factura crédito con vencimiento y transferencia, factura de varias páginas, tiquete, recibo y caso de carne con cantidad decimal/lote. Nunca cargar datos reales de otro cliente por defecto.
- Cambiar A4/térmico en la vista y presentar dimensiones físicas, corte de rollo y elementos que se omiten por formato.
- Permitir comparar “actual” vs “borrador” y restaurar preset o última versión guardada. No sobrescribir por accidente la predeterminada activa.
- Mostrar avisos de resultado: logo ausente en el emisor, columnas comprimidas, texto truncado, contraste bajo, QR sin destino válido o fuente no compatible.
- Mantener el PDF dentro de un visor con descarga y abrir en nueva pestaña. Probar el visor con bloqueadores y navegadores de tablet; ofrecer descarga si el navegador no incrusta PDF.

### W4. Activación, versiones y permisos

- Guardar como borrador, previsualizar y **activar** deben ser acciones separadas. La activación requiere confirmación e indica a qué emisor, serie, tipo y formato afecta.
- Mostrar historial: versión, usuario, fecha, preset, estado, dónde es predeterminada y cuántos documentos se generaron con ella. La reversión crea una nueva versión o reactiva una anterior de manera auditada.
- Una plantilla por serie prevalece sobre emisor/tipo, y la UI explica la cascada. Si no hay una, mostrar el preset embebido como fallback, no como si fuera una plantilla guardada.
- Permisos distintos: configurar/activar plantillas, previsualizar, ver documentos e imprimir/reimprimir. Ocultar acciones no autorizadas y respetar igualmente el rechazo del API.
- Logo/correo/datos bancarios se editan en el ámbito correspondiente. El logo se modifica desde Emisores y no desde la plantilla. La pantalla no debe permitir que un usuario de una sucursal configure la identidad de un emisor ajeno.

## 4. Plan de visualización e impresión en módulos

### W5. Un componente único de documento

Evolucionar `BotonImprimir` hacia un componente de resultado/visualización que reciba:

- tipo de documento, id, formato disponible, `copia`, título y estado de emisión;
- acción “Ver PDF”, “Descargar”, “Imprimir copia” y “Reintentar PDF” cuando sea aplicable;
- texto accesible que identifique documento y consecutivo, no solo un icono;
- manejo de carga/error sin crear una segunda operación.

El enlace local `/documentos/{tipo}/{id}/pdf` mantiene el token en servidor. La implementación debe garantizar una respuesta que el navegador pueda mostrar en línea; si el encabezado fuerza descarga, usar una acción explícita para descargar y otra para abrir visor. No añadir tokens, claves privadas ni XML a la URL.

### W6. Puntos de integración

Aplicar el componente de forma consistente a los documentos ya soportados: factura, tiquete, nota de crédito, recibos de cobro/pago, presupuesto, consignación, inventarios, traslado, toma física, órdenes, liquidaciones, boleta de trámite y devolución interna.

- Facturación/tiquete: tras confirmación mostrar una tarjeta de resultado recuperable. “PDF disponible” no equivale a “Hacienda aceptó”; indicar el estado real.
- Cobrar: ofrecer recibo existente y PDF de factura/tiquete ya creado; no encadenar impresión con nuevo cobro/facturación.
- Crédito: PDF incorpora vencimiento, días y medio de pago con el texto configurado; el enlace a estado de cuenta es otro documento/proceso.
- Bandeja y reimpresiones: pedir `copia=true`, conservar el formato fiscal A4 y no consumir consecutivos.
- POS: permitir seleccionar térmico 80/58 solo si hay plantilla/capacidad. Abrir PDF no es impresión silenciosa de impresora; Bluetooth, gaveta y dispositivos siguen siendo alcance separado.

### W7. Usabilidad y responsive

- La configuración es de escritorio, pero el visor, reimpresión y tiquete deben funcionar en tablet.
- Botones táctiles, indicadores de carga y retorno a la venta después de cerrar visor; no perder el resultado si una ventana emergente es bloqueada.
- Evitar transportar PDFs grandes por base64 a través del circuito de Blazor para documentos normales. El endpoint local en streaming es la ruta preferida; base64 queda solo para previsualización contenida y con límites.
- Cargar y paginar historial/listas de plantillas; las miniaturas/previews se solicitan bajo demanda.

## 5. Dependencias del API y contrato

El sitio depende de las fases P1-P4 del plan API. Antes de modificar el editor, el API debe publicar:

1. Logo del emisor como activo propio: metadatos en el DTO de emisor, carga/reemplazo/eliminación y miniatura mediante endpoints autorizados; sin `LogoOverride` en plantillas.
2. Esquema versionado de preset/tema/bloques y catálogo por tipo/formato.
3. Validaciones por campo con mensajes de negocio y advertencias de diseño.
4. Vista previa temporal de configuración no guardada.
5. Versiones, estado borrador/activa, auditoría y resolución de cascada.
6. Datos de resultado de documento: disponibilidad de PDF, formato, consecutivo, copia, estado fiscal e impresión autorizada.
7. QR/monto en letras/notas y perfiles de columnas provistos como información semántica, no armados por la UI.

Después se regeneran los clientes con `tools/actualizar-contratos.sh`; los archivos Generated no se editan manualmente. Crear DTOs manuales solo cuando el contrato no pueda regenerarse temporalmente y con fecha de retiro documentada.

## 6. Pruebas necesarias

### Unitarias y de componentes

- Presets y mapeos tipo/slug/formato, incluidos nuevos tipos; ninguna etiqueta “Tipo X” para documentos conocidos.
- Carga de logo por emisor: PNG/JPEG válido, imagen corrupta, límite de tamaño, permisos, sustitución, eliminación, vista de metadatos y ausencia de Base64 en listados.
- Serialización/deserialización de config v1 y v2, edición sin pérdida de campos y restauración de versión.
- Reglas UI: A4 vs térmico, serie aplicable, campos fiscales protegidos, contraste, columnas excedidas, QR inválido y activación sin vista previa.
- Sincronización de modelo/textareas y borrador: preview no llama guardar ni cambia la plantilla activa.
- Permisos, errores 401/403, emisor/sucursal no autorizados y mensajes visibles al usuario.
- `BotonImprimir`/visor: URL codificada, copia/formato, estados, error y no duplicación al doble clic.

### E2E y revisión visual

1. Cargar el logo de un emisor, crear borrador corporativo, previsualizar sin persistir, guardar, activar y verificar que todas sus series obtienen ese logo, sin override desde plantilla.
2. Abrir PDF de documentos cortos/largos, A4 y térmico; probar descarga, nueva pestaña y bloqueo de popup.
3. Verificar el resultado contra renders golden aportados por API: logo, contraste, encabezado de tabla repetido, total, pie/QR y ausencia de solapes/cortes.
4. Casos de abarrotes/carnes: moneda CRC/USD, 0,375 kg, descuentos, bonificación a cero explicada, cliente con dirección larga y factura de crédito.
5. Imprimir una copia y confirmar que no cambia venta, cobro, clave ni estado de Hacienda.
6. Probar teléfono/tablet y lector de PDF; confirmar que el documento se puede recuperar después de reconexión o refresco.
7. Recorrer todos los módulos soportados y permisos, no solo factura.

No marcar la función como completa por una vista previa satisfactoria. La aceptación requiere PDF real desde cada módulo, el renderizador API, correo fiscal y reimpresión auditada.

## 7. Fases web

| Fase | Entrega web |
|---|---|
| W0 | Aprobar presets, guía de marca y decisiones QR/datos bancarios/monto en letras |
| W1 | Logo en Emisores, contrato/proxy autorizado, migración de datos heredados y eliminación de override del editor |
| W2 | Consumir contrato v2, actualizar tipos/DTOs/proxies y migrar editor sin romper plantillas v1 |
| W3 | Selector de preset, editor guiado, preview de borrador y validaciones de diseño |
| W4 | Versionado, activación/auditoría y explicación de cascada por serie/emisor |
| W5 | Visor/componente único en ventas, caja, compras, consignación e inventario |
| W6 | E2E, tablet, accesibilidad y piloto con un emisor/serie antes de generalizar |

## 8. Decisiones pendientes

1. Aprobar un logo oficial único, colores, fuentes y variante monocromática de cada emisor.
2. Confirmar quién puede editar y quién puede activar una plantilla.
3. Definir QR y fuente de consulta autorizada.
4. Confirmar cuándo mostrar datos bancarios, monto en letras y políticas comerciales.
5. Decidir si el PDF inicial abre en visor, pestaña nueva o descarga, y qué dispositivos térmicos se soportan realmente.
6. Definir cuánto tiempo se guardan snapshots/PDFs y qué diseño se usa en reimpresiones históricas. Recomendación: siempre el snapshot original en documentos fiscales.

La calidad esperada no es copiar la factura externa: es lograr un documento propio, consistente, legible, verificable y configurable sin comprometer datos fiscales ni el proceso de venta/cobro.
