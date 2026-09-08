namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

// REDISENO_TIPOS_SERIES_CONDICION.md: la Serie lleva la condición de venta
// (EsCredito) y el documento electrónico (RequiereDocumentoElectronico +
// CodigoFE) — antes vivían en el Tipo de Factura. EsRecibo/EsPago/
// EsConsignacion se eliminan: el Tipo de Documento (vía su Uso) ya indica
// para qué se usa la serie.
public sealed class SerieFacturacionFiscalDTO
{
    public int IdSerie { get; set; }
    public long Secuencia { get; set; }
    public int NumeroTerminal { get; set; }
    public int IdSucursal { get; set; }
    public int IdEmisor { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int? IdTipoFactura { get; set; }
    /// <summary>Condición de venta: false = Contado, true = Crédito. Solo aplica
    /// cuando el Tipo ligado tiene Uso = Facturación.</summary>
    public bool EsCredito { get; set; }
    /// <summary>Switch: si esta serie requiere emitir documento electrónico.</summary>
    public bool RequiereDocumentoElectronico { get; set; }
    public bool EmisionV44Habilitada { get; set; }
    /// <summary>Código de comprobante electrónico de esta serie (01/03/04). Se manda
    /// al guardar cuando RequiereDocumentoElectronico es true.</summary>
    public string? CodigoFE { get; set; }

    /// <summary>Modalidad Tiquete del tipo ligado (solo lectura, la deriva el API).
    /// PLAN_TIQUETE_RUTAS_FACTURACION_WEB.md W2: dato informativo, no editable en la serie.</summary>
    public bool EsTiquete { get; set; }
    /// <summary>"Electronica" | "Interna" (solo lectura). Deriva de RequiereDocumentoElectronico.</summary>
    public string? Naturaleza { get; set; }

    // ---- Derivados (solo lectura, los llena el API) ----
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
    /// <summary>"facturacion" | "devolucion" | "compra" | "consignacion" — para agrupar el selector.</summary>
    public string? Uso { get; set; }
    /// <summary>El tipo está marcado como Tiquete (W2: informa la modalidad al elegir tipo).</summary>
    public bool EsTiquete { get; set; }
}
