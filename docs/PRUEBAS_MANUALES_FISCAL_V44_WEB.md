# Pruebas manuales WEB — flujo fiscal V4.4

## Precondiciones

- Usuario autenticado con permisos `Ver`, `Crear` y `Modificar` en las pantallas correspondientes.
- API con las migraciones V4.4 aplicadas y datos fiscales de prueba.
- Emisor, sucursal y serie de prueba configurados antes de ejecutar la bandeja.

## Catálogos

- Tipos de factura, identificación, impuesto, formas de pago, tipos de cobro y tipos de exoneración: crear, editar y deshabilitar cuando aplique; confirmar que el registro se refresca.
- Monedas y denominaciones: crear, editar y deshabilitar; validar valores no negativos, moneda y tipo de denominación.
- Configuración de plazos: crear, editar y deshabilitar; confirmar límites de 1 a 3650 días.

## Configuración de emisión

- Sucursales: crear y editar, validando que `NumeroSucursalFE` tenga exactamente tres dígitos.
- Emisores: editar datos públicos y confirmar que el listado no muestra secretos. Actualizar credenciales con certificado `.p12` y verificar que el formulario queda vacío después de guardar.
- Series: crear y editar; confirmar que el API rechaza una disminución de secuencia o cambios estructurales si ya existen documentos.

## Bandeja fiscal V4.4

- Abrir `Bandeja Fiscal V4.4`, filtrar por clave y estado, y recorrer páginas.
- Consultar detalle e historial de eventos.
- Consultar XML firmado y respuesta de Hacienda bajo el permiso `Ver`.
- Comprobar que `Reintentar` aparece únicamente para `ErrorTecnico` y requiere permiso `Modificar`.

## Bloqueo conocido

No se debe habilitar edición WEB de provincia, cantón o distrito todavía. El API expone crear/editar con `CodigoFE`, pero sus listados históricos no devuelven dicho código; hace falta un endpoint de listado de mantenimiento que incluya identificador, padre, descripción y `CodigoFE`.
