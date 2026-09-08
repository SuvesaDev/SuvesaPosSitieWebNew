namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

/// <summary>Tipo de cambio oficial CRC/USD (espejo de TipoCambioOficial en la API).</summary>
public sealed class TipoCambioOficialDTO
{
    public DateTime Fecha { get; set; }
    public decimal Compra { get; set; }
    public decimal Venta { get; set; }
    public string Fuente { get; set; } = string.Empty;
}
