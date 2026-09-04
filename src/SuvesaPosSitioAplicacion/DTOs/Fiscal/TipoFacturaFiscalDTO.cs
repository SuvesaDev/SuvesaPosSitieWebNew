namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

/// <summary>Propósito de un tipo de documento. Espejo del enum del API.</summary>
public enum UsoTipoDocumento
{
    Facturacion = 1,
    Devolucion = 2,
    Compra = 3,
    Consignacion = 4,
}

/// <summary>Contrato estable del mantenimiento fiscal V4.4, aislado del cliente OpenAPI heredado.
/// REDISENO_TIPOS_SERIES_CONDICION.md: el Tipo ya no lleva Contado/Credito/CodigoFE —
/// la condición de venta y el documento electrónico ahora viven en la Serie.</summary>
public sealed class TipoFacturaFiscalDTO
{
    public int Id { get; set; }
    public string? Descripcion { get; set; }
    public int Codigo { get; set; }
    public UsoTipoDocumento Uso { get; set; } = UsoTipoDocumento.Facturacion;
    public bool Activo { get; set; } = true;
}
