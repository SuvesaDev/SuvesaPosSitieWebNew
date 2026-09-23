namespace SuvesaPosSitioAplicacion.DTOs.Compras;

public sealed class CrearImportacionWebDTO
{
    public string FacturaExtranjera { get; set; } = "";
    public string? Dua { get; set; }
    public string? Embarque { get; set; }
    public int IdProveedorExtranjero { get; set; }
    public int IdBodega { get; set; }
    public int? IdEmpresa { get; set; }
    public int? IdEmisor { get; set; }
    public int? IdSerieMensajeReceptor { get; set; }
    public int CodMoneda { get; set; } = 2;
    public decimal TipoCambioDua { get; set; }
    public DateTime FechaTipoCambioDua { get; set; } = DateTime.Today;
    public string? Observaciones { get; set; }
    public List<ImportacionLineaWebDTO> Lineas { get; set; } = new();
}

public class ImportacionLineaWebDTO
{
    public long IdArticulo { get; set; }
    public string CodigoProveedor { get; set; } = "";
    public string DescripcionProveedor { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal CostoProveedorMonedaUnitario { get; set; }
    public decimal TributoDuaUnitario { get; set; }
    public decimal? PrecioAAplicado { get; set; }
    public List<ImportacionLoteWebDTO> Lotes { get; set; } = new();
}

public sealed class ActualizarTributoDuaImportacionWebDTO
{
    public long IdImportacionLinea { get; set; }
    public decimal TributoDuaUnitario { get; set; }
}

public sealed class ImportacionLoteWebDTO
{
    public string Numero { get; set; } = "";
    public DateOnly? FechaVencimiento { get; set; }
    public decimal Cantidad { get; set; }
}

public sealed class ImportacionCostoWebDTO
{
    public int Categoria { get; set; }
    public string Descripcion { get; set; } = "";
    public decimal MontoCapitalizable { get; set; }
    public decimal IvaAcreditable { get; set; }
}

public sealed class ImportacionDocumentoCargaWebDTO
{
    public int Tipo { get; set; }
    public string NombreArchivo { get; set; } = "";
    public string TipoMime { get; set; } = "application/octet-stream";
    public byte[] Contenido { get; set; } = Array.Empty<byte>();
    public string? ClaveFiscal { get; set; }
    public int? IdProveedorLocal { get; set; }
}

public sealed class ImportacionDocumentoResumenWebDTO
{
    public long IdImportacionDocumento { get; set; }
    public int Tipo { get; set; }
    public string NombreArchivo { get; set; } = "";
    public string? ClaveFiscal { get; set; }
    public string? EstadoFiscal { get; set; }
    public string? DetalleFiscal { get; set; }
    public int IntentosEnvioFiscal { get; set; }
    public int? IdProveedorLocal { get; set; }
    public string? NombreProveedorLocal { get; set; }
    public long? IdCompraGenerada { get; set; }
}

public sealed class ActualizarEstadoFiscalImportacionWebDTO
{
    public string EstadoFiscal { get; set; } = "";
    public string? DetalleFiscal { get; set; }
}

public sealed class CerrarImportacionWebDTO
{
    public DateTime FechaRecepcion { get; set; } = DateTime.Today;
}

public sealed class FiltroReporteImportacionWebDTO
{
    public string? Dua { get; set; }
    public int? IdProveedor { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}

public sealed class ReporteImportacionWebDTO
{
    public decimal CostoNacionalizado { get; set; }
    public decimal PorcentajeLogistico { get; set; }
    public decimal DiasPromedioNacionalizacion { get; set; }
    public int DocumentosPendientes { get; set; }
    public decimal MargenEsperado { get; set; }
    public List<ReporteImportacionFilaWebDTO> Filas { get; set; } = new();
}

public sealed class ReporteImportacionFilaWebDTO
{
    public long IdImportacion { get; set; }
    public string FacturaExtranjera { get; set; } = "";
    public string? Dua { get; set; }
    public decimal CostoNacionalizado { get; set; }
    public decimal PorcentajeLogistico { get; set; }
    public decimal DiasNacionalizacion { get; set; }
    public int DocumentosPendientes { get; set; }
    public decimal MargenEsperado { get; set; }
}

public sealed class ImportacionResumenWebDTO
{
    public long IdImportacion { get; set; }
    public string FacturaExtranjera { get; set; } = "";
    public string? Dua { get; set; }
    public string? Embarque { get; set; }
    public int Estado { get; set; }
    public decimal Unidades { get; set; }
    public decimal CostoMercanciaCrc { get; set; }
    public decimal CostosImportacionCrc { get; set; }
    public decimal TributosDuaCrc { get; set; }
    public decimal CostoNacionalizadoCrc { get; set; }
    public List<ImportacionCostoWebDTO> Costos { get; set; } = new();
    public List<ImportacionDocumentoResumenWebDTO> Documentos { get; set; } = new();
    public List<ImportacionLineaResultadoWebDTO> Lineas { get; set; } = new();
    public List<ImportacionCompraGeneradaWebDTO> ComprasGeneradas { get; set; } = new();
}

public sealed class ImportacionCompraGeneradaWebDTO
{
    public long IdCompra { get; set; }
    public string Proveedor { get; set; } = "";
    public string Factura { get; set; } = "";
    public decimal TotalFactura { get; set; }
    public int CodMoneda { get; set; }
}

public sealed class ImportacionLineaResultadoWebDTO : ImportacionLineaWebDTO
{
    public long IdImportacionLinea { get; set; }
    public decimal CostoProveedorCrcUnitario { get; set; }
    public decimal CostoImportacionUnitario { get; set; }
    public decimal CostoNacionalizadoUnitario { get; set; }
    public string? DescripcionArticulo { get; set; }
}
