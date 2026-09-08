namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

public sealed class DenominacionMonedaFiscalDTO
{
    public long IdDenominacion { get; set; }
    public int CodMoneda { get; set; }
    public int Denominacion { get; set; }
    public string? Tipo { get; set; }
    public bool Activo { get; set; } = true;
}
