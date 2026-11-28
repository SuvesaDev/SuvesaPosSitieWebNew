namespace SuvesaPosSitioAplicacion.DTOs.Comercial;

// Espejo a mano de api/comercial/catalogos (DTOs/CatalogosComercialesDTOs.cs del API).

/// <summary>Catálogos editables del módulo comercial. El valor de la ruta del API sale de <see cref="CatalogoComercialRutas"/>.</summary>
public enum TipoCatalogoComercial
{
    Canales = 1,
    Especies = 2,
    FuncionesTerapeuticas = 3,
}

public static class CatalogoComercialRutas
{
    public static string Segmento(TipoCatalogoComercial tipo) => tipo switch
    {
        TipoCatalogoComercial.Canales => "canales",
        TipoCatalogoComercial.Especies => "especies",
        TipoCatalogoComercial.FuncionesTerapeuticas => "funciones-terapeuticas",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
    };
}

/// <summary>Canales, marcas y especies para los filtros comerciales de los reportes de ventas.</summary>
public sealed class OpcionesReportesDTO
{
    public List<OpcionFiltroDTO> Canales { get; set; } = new();
    public List<OpcionFiltroDTO> Marcas { get; set; } = new();
    public List<OpcionFiltroDTO> Especies { get; set; } = new();
}

/// <summary>Un valor de un catálogo comercial.</summary>
public sealed class CatalogoComercialDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}
