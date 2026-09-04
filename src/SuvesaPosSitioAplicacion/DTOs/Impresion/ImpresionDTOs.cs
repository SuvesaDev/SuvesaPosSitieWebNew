namespace SuvesaPosSitioAplicacion.DTOs.Impresion;

/// <summary>Resumen de plantilla para el listado.</summary>
public sealed class PlantillaImpresionResumenDTO
{
    public int Id { get; set; }
    public int IdEmisor { get; set; }
    public int TipoDocumento { get; set; }
    public string TipoDocumentoSlug { get; set; } = "";
    public int? IdSerie { get; set; }
    public string Nombre { get; set; } = "";
    public bool EsPredeterminada { get; set; }
    public int Formato { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaActualizacionUtc { get; set; }
}

/// <summary>Plantilla completa. La configuración de zonas viaja como JSON.</summary>
public sealed class PlantillaImpresionDTO
{
    public int Id { get; set; }
    public int IdEmisor { get; set; }
    public int TipoDocumento { get; set; }
    public int? IdSerie { get; set; }
    public string Nombre { get; set; } = "";
    public bool EsPredeterminada { get; set; }
    public int Formato { get; set; } = 1;
    public byte? AnchoRolloMm { get; set; }
    public bool Activa { get; set; } = true;
    public string ConfiguracionJson { get; set; } = "";
    public string? LogoOverrideBase64 { get; set; }
}

/// <summary>Una columna del catálogo de detalle de un tipo.</summary>
public sealed class ColumnaCatalogoImpresionDTO
{
    public string Clave { get; set; } = "";
    public string Etiqueta { get; set; } = "";
    public double AnchoRel { get; set; }
    public string Alineacion { get; set; } = "izquierda";
    public bool VisibleEnTermico { get; set; }
}

/// <summary>Catálogo de campos/columnas de un tipo + configuraciones por defecto.</summary>
public sealed class CatalogoPlantillaImpresionDTO
{
    public int TipoDocumento { get; set; }
    public string TipoDocumentoSlug { get; set; } = "";
    public bool UsaSerie { get; set; }
    public bool EsElectronico { get; set; }
    public IReadOnlyList<ColumnaCatalogoImpresionDTO> Columnas { get; set; } = Array.Empty<ColumnaCatalogoImpresionDTO>();
    public IReadOnlyList<string> CamposReceptor { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> CamposMeta { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> FilasTotales { get; set; } = Array.Empty<string>();
    public string ConfiguracionPorDefectoA4Json { get; set; } = "";
    public string ConfiguracionPorDefectoTermicoJson { get; set; } = "";
}

/// <summary>Cuerpo de la previsualización: config y formato opcionales.</summary>
public sealed class PrevisualizarPlantillaDTO
{
    public string? ConfiguracionJson { get; set; }
    public int? Formato { get; set; }
}

/// <summary>Utilidades de presentación de los tipos de impresión (espejo del enum del API).</summary>
public static class TiposImpresionUi
{
    public static string Nombre(int tipo) => tipo switch
    {
        1 => "Factura electrónica",
        2 => "Tiquete electrónico",
        3 => "Nota de crédito",
        4 => "Recibo de pago",
        5 => "Recibo de cobro",
        6 => "Presupuesto",
        7 => "Boleta de consignación",
        8 => "Inventario · toma general",
        9 => "Inventario · ajuste",
        10 => "Traslado de bodega",
        11 => "Toma física",
        12 => "Orden de compra",
        _ => $"Tipo {tipo}",
    };

    public static string Slug(int tipo) => tipo switch
    {
        1 => "factura-electronica",
        2 => "tiquete-electronico",
        3 => "nota-credito",
        4 => "recibo-pago",
        5 => "recibo-cobro",
        6 => "presupuesto",
        7 => "consignacion-boleta",
        8 => "inventario-toma-general",
        9 => "inventario-ajuste",
        10 => "traslado-bodega",
        11 => "toma-fisica",
        12 => "orden-compra",
        _ => "",
    };

    public static bool UsaSerie(int tipo) => tipo is 1 or 2 or 3;

    public static string Formato(int f) => f == 2 ? "Térmico 80 mm" : "A4";

    public static IReadOnlyList<(int Valor, string Nombre)> Todos { get; } =
        Enumerable.Range(1, 12).Select(v => (v, Nombre(v))).ToList();
}
