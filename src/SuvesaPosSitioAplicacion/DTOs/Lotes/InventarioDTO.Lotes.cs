using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// §3 lotes — tipo de artículo y lote único (API MEJORA_LOTES §3.1).
/// TEMPORAL: al regenerar contratos NSwag el generado ya trae estos campos.
/// </summary>
public partial class InventarioDTO
{
    /// <summary>1=Normal, 2=Materia prima, 3=Producto terminado.</summary>
    [JsonPropertyName("tipoArticulo")]
    public int TipoArticulo { get; set; } = 1;

    [JsonPropertyName("loteUnico")]
    public bool LoteUnico { get; set; }

    /// <summary>Sólo lectura: fijado el lote único, tipo/lote único no se editan.</summary>
    [JsonPropertyName("loteUnicoFijado")]
    public bool LoteUnicoFijado { get; set; }

    [JsonPropertyName("esPresentacionEmpaquetada")]
    public bool EsPresentacionEmpaquetada { get; set; }

    /// <summary>Si el costo no es colones, A/B/C/D/Promo se venden × TipoCambioVenta.</summary>
    [JsonPropertyName("conviertePrecioColones")]
    public bool ConviertePrecioColones { get; set; }

    /// <summary>Copia de Monedas.ValorVenta congelada en el artículo. No fluctúa en la venta.</summary>
    [JsonPropertyName("tipoCambioVenta")]
    public double TipoCambioVenta { get; set; } = 1;
}
