namespace SuvesaPosSitioAplicacion.Class;

/// <summary>
/// Acciones que un rol puede tener sobre una funcion (rediseno de seguridad V2).
/// El nombre en MAYUSCULAS coincide con <c>Accion.Codigo</c> del API
/// (VER/CREAR/EDITAR/BORRAR/ACTIVAR/EXPORTAR/IMPRIMIR).
///
/// <c>Modificar</c> se conserva como alias de <see cref="Editar"/> mientras el sitio
/// termina de migrar del contrato viejo (AccionesDTO con Ver/Crear/Modificar/Borrar).
/// </summary>
public enum AccionPantalla
{
    Ver = 0,
    Crear = 1,
    Editar = 2,
    Modificar = 2,
    Borrar = 3,
    Activar = 4,
    Exportar = 5,
    Imprimir = 6
}

/// <summary>
/// Nivel al que pertenece cada pantalla, segun la estrategia adaptable acordada.
/// Determina desde que ancho tiene sentido abrirla.
/// </summary>
public enum NivelPantalla
{
    /// <summary>Movil, desde 360 px. Consulta de inventario, clientes, cotizaciones, reportes.</summary>
    Movil = 1,

    /// <summary>Tableta, desde 768 px. Toma fisica, pedidos a bodega, consignacion.</summary>
    Tableta = 2,

    /// <summary>Escritorio, desde 1280 px. Facturacion, caja, arqueo, compras, parametros.</summary>
    Escritorio = 3
}
