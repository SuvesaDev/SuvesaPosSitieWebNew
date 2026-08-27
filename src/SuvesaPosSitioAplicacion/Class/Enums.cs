namespace SuvesaPosSitioAplicacion.Class;

/// <summary>
/// Acciones que un rol puede tener sobre una pantalla.
/// Espejo exacto de AccionesDTO, que es lo que devuelve el API en rol.permisos.
/// </summary>
public enum AccionPantalla
{
    Ver = 0,
    Crear = 1,
    Modificar = 2,
    Borrar = 3
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
