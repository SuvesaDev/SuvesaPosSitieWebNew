namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

/// <summary>Resultado de reemitir un comprobante rechazado (espejo de api/facturacion/reemitir-comprobante).</summary>
public sealed class ReemisionComprobanteWeb
{
    public string Clave { get; set; } = string.Empty;
    public string Consecutivo { get; set; } = string.Empty;
    public double NumFactura { get; set; }
    public DateTime FechaFiscal { get; set; }
}
