namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

/// <summary>Propósito de un tipo de documento. Espejo del enum del API.</summary>
public enum UsoTipoDocumento
{
    Facturacion = 1,
    Devolucion = 2,
    Compra = 3,
    Consignacion = 4,
}

/// <summary>Contrato estable del mantenimiento fiscal V4.4, aislado del cliente OpenAPI heredado.</summary>
public sealed class TipoFacturaFiscalDTO
{
    public int Id { get; set; }
    public string? Descripcion { get; set; }
    public int Codigo { get; set; }
    public UsoTipoDocumento Uso { get; set; } = UsoTipoDocumento.Facturacion;
    public bool Credito { get; set; }
    public bool Contado { get; set; }
    public bool Activo { get; set; } = true;
    public string? CodigoFE { get; set; }
}

/// <summary>Un código FE del catálogo cerrado (01-04) y si ya lo usa otro tipo.</summary>
public sealed class CodigoFEDisponibleFiscalDTO
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int? EnUsoPorId { get; set; }
    public string? EnUsoPorDescripcion { get; set; }
}
