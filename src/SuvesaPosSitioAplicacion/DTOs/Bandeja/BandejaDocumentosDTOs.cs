namespace SuvesaPosSitioAplicacion.DTOs.Bandeja;

// Espejo a mano de los contratos de BandejaDocumentosController del API
// (BANDEJA_DOCUMENTOS_API.md). El regen completo de contratos rompe ~50 proxies,
// así que se declara aquí. STJ (JsonSerializerDefaults.Web): escribe camelCase,
// lee sin distinguir mayúsculas.

public sealed class BandejaDocumentosFiltro
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public string? Texto { get; set; }
    public string? EstadoHacienda { get; set; }
    public int? IdSucursal { get; set; }
    public int? IdEmisor { get; set; }
    public bool IncluirAnulados { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 25;
}

public class DocumentoBandeja
{
    public long Id { get; set; }
    public string Clase { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Consecutivo { get; set; } = string.Empty;
    public string? Cliente { get; set; }
    public string? Sucursal { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public bool Anulado { get; set; }
    public string? TipoDescripcion { get; set; }
    public string? EstadoDescripcion { get; set; }
}

public sealed class DocumentoFiscalBandeja : DocumentoBandeja
{
    public string? NumeroFacturaElectronica { get; set; }
    public string? ClaveFacturaElectronica { get; set; }
    public string? EstadoHacienda { get; set; }
    public bool EnviadoHacienda { get; set; }
    public string? MensajeRechazoHacienda { get; set; }
}

public sealed class BandejaDocumentosResultado<T>
{
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public List<T> Registros { get; set; } = new();
}

public sealed class DocumentoBandejaLinea
{
    public string CodArticulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
}

public sealed class FacturaBandejaDetalle
{
    public DocumentoFiscalBandeja Encabezado { get; set; } = new();
    public string? Cajero { get; set; }
    public string? Moneda { get; set; }
    public string? Observaciones { get; set; }
    public decimal SubtotalGravado { get; set; }
    public decimal SubtotalExento { get; set; }
    public decimal Descuento { get; set; }
    public bool PuedeDevolver { get; set; }
    public List<DocumentoBandejaLinea> Lineas { get; set; } = new();
}

public sealed class NotaCreditoBandejaDetalle
{
    public DocumentoFiscalBandeja Encabezado { get; set; } = new();
    public long IdFactura { get; set; }
    public string? NumeroFactura { get; set; }
    public string? Notas { get; set; }
    public decimal SubtotalGravado { get; set; }
    public decimal SubtotalExento { get; set; }
    public decimal Descuento { get; set; }
    public List<DocumentoBandejaLinea> Lineas { get; set; } = new();
}
