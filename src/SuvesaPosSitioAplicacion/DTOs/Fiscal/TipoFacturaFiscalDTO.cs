namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

/// <summary>Contrato estable del mantenimiento fiscal V4.4, aislado del cliente OpenAPI heredado.</summary>
public sealed class TipoFacturaFiscalDTO
{
    public int Id { get; set; }
    public string? Descripcion { get; set; }
    public int Codigo { get; set; }
    public bool Credito { get; set; }
    public bool Compra { get; set; }
    public bool Consignacion { get; set; }
    public string? CodigoFE { get; set; }
}
