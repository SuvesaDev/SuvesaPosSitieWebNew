namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

public sealed class SerieFacturacionFiscalDTO
{
    public int IdSerie { get; set; }
    public long Secuencia { get; set; }
    public int NumeroTerminal { get; set; }
    public int IdSucursal { get; set; }
    public int IdEmisor { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int? IdTipoFactura { get; set; }
    public bool? EsCredito { get; set; }
    public bool? EsRecibo { get; set; }
    public bool? EsPago { get; set; }
    public bool? EsConsignacion { get; set; }
    public bool EmisionV44Habilitada { get; set; }

    // ---- Derivados (solo lectura, los llena el API) ----
    public string? CodigoFE { get; set; }
    public string? NumeroSucursalFE { get; set; }
    public string? EmisorNombre { get; set; }
    public string? EmisorIdentificacion { get; set; }
    public string? SucursalNombre { get; set; }
    public bool SucursalFEValida { get; set; }
    public string? TipoFacturaDescripcion { get; set; }
    public int? TipoFacturaCodigo { get; set; }
    public bool EsFiscal { get; set; }
    public bool CompatibleV44 { get; set; }
    public string? UsoDescripcion { get; set; }
    public bool TieneDocumentos { get; set; }
    public string? ProximoConsecutivoEjemplo { get; set; }
}

public sealed class SeriesFacturacionCatalogosFiscalDTO
{
    public List<SerieCatalogoEmisorFiscalDTO> Emisores { get; set; } = new();
    public List<SerieCatalogoSucursalFiscalDTO> Sucursales { get; set; } = new();
    public List<SerieCatalogoTipoFacturaFiscalDTO> TiposFactura { get; set; } = new();
}

public sealed class SerieCatalogoEmisorFiscalDTO
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Identificacion { get; set; }
}

public sealed class SerieCatalogoSucursalFiscalDTO
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? NumeroFE { get; set; }
    public bool FEValida { get; set; }
}

public sealed class SerieCatalogoTipoFacturaFiscalDTO
{
    public int Id { get; set; }
    public int Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string? CodigoFE { get; set; }
    public bool EsFiscal { get; set; }
    public bool CompatibleV44 { get; set; }
    /// <summary>"facturacion" | "devolucion" | "compra" | "consignacion" — para agrupar el selector.</summary>
    public string? Uso { get; set; }
    public bool Contado { get; set; }
    public bool Credito { get; set; }
}
