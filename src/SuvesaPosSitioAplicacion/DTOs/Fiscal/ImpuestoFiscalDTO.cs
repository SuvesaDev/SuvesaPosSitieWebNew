namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

public sealed class ImpuestoFiscalDTO
{
    public int IdImpuesto { get; set; }
    public string? Impuesto1 { get; set; }
    public string? CodigoImpuesto { get; set; }
    public string? CodigoTarifa { get; set; }
    public double Porcentaje { get; set; }
    public string? Simbolo { get; set; }
    public bool Inactivo { get; set; }
    public bool? Estado { get; set; } = true;
    public string? IdUsuarioCreacion { get; set; }
    public string? IdUsuarioModificacion { get; set; }
}
