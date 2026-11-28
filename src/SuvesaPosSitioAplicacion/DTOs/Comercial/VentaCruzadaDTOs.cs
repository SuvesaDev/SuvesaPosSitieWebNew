namespace SuvesaPosSitioAplicacion.DTOs.Comercial;

// Espejo a mano de api/comercial/venta-cruzada (DTOs/VentaCruzadaDTOs.cs del API).

public static class TipoComplemento
{
    public const string Articulo = "Articulo";
    public const string Familia = "Familia";
    public const string Categoria = "Categoria";

    public static readonly (string Valor, string Texto)[] Todos =
    {
        (Articulo, "Artículo"),
        (Familia, "Familia completa"),
        (Categoria, "Categoría completa"),
    };
}

public sealed class ReglaVentaCruzadaDTO
{
    public int Id { get; set; }
    public string OrigenTipo { get; set; } = string.Empty;
    public long OrigenId { get; set; }
    public string OrigenNombre { get; set; } = string.Empty;
    public string ComplementoTipo { get; set; } = string.Empty;
    public long ComplementoId { get; set; }
    public string ComplementoNombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public string? Nota { get; set; }
    public bool EsRelacionExistente { get; set; }
}

public sealed class GuardarReglaVentaCruzadaRequest
{
    public string OrigenTipo { get; set; } = string.Empty;
    public long OrigenId { get; set; }
    public string ComplementoTipo { get; set; } = string.Empty;
    public long ComplementoId { get; set; }
    public string? Nota { get; set; }
}

public sealed class CambiarActivoReglaRequest
{
    public bool Activo { get; set; }
}

public sealed class OportunidadVentaCruzadaDTO
{
    public long IdCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public int IdRuta { get; set; }
    public string? NombreRuta { get; set; }
    public List<string> Compro { get; set; } = new();
    public List<string> Ofrecer { get; set; } = new();
    public DateTime? UltimaCompra { get; set; }
}

public sealed class OportunidadesVentaCruzadaDTO
{
    public int DiasVentana { get; set; }
    public bool EsSupervision { get; set; }
    public List<OportunidadVentaCruzadaDTO> Oportunidades { get; set; } = new();
}
