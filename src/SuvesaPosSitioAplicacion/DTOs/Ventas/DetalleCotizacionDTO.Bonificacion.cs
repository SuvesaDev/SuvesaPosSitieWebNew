using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (mismo patrón que FacturaDTO.IdSerie.cs). El API ya trae estas columnas
// en el detalle de la proforma (migración CotizacionDetalleBonificacion); al regenerar los contratos este archivo se borra.
//
// Una proforma puede llevar bonificaciones igual que una factura. Se guardan las mismas marcas que en VentasDetalle
// para que Facturación cargue la proforma tal cual, grupos de regalo incluidos.

public partial class DetalleCotizacionDTO
{
    /// <summary>Línea de regalo (precio de venta 0; el impuesto se calcula sobre el precio de lista).</summary>
    [JsonPropertyName("esBonificacion")]
    public bool EsBonificacion { get; set; }

    [JsonPropertyName("idGrupoBonificacion")]
    public int? IdGrupoBonificacion { get; set; }

    [JsonPropertyName("idConfiguracionBonificacion")]
    public int? IdConfiguracionBonificacion { get; set; }

    /// <summary>La única línea del grupo que se puede eliminar (borra el grupo completo).</summary>
    [JsonPropertyName("esLineaPrincipalBonificacion")]
    public bool? EsLineaPrincipalBonificacion { get; set; }

    /// <summary>Precio de lista de la línea de regalo (su precio de venta es 0).</summary>
    [JsonPropertyName("precioReferenciaBonificacion")]
    public double PrecioReferenciaBonificacion { get; set; }
}
