namespace SuvesaPosSitioAplicacion.DTOs.Reportes;

/// <summary>Espejo de los DTOs de IVA del API (api/reportes-iva).</summary>
public sealed class IvaTarifaFila
{
    public string CodigoTarifa { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal Porcentaje { get; set; }
    public string TipoOperacion { get; set; } = "";
    public decimal BaseVentas { get; set; }
    public decimal IvaVentas { get; set; }
    public decimal BaseNotasCredito { get; set; }
    public decimal IvaNotasCredito { get; set; }
    public decimal BaseNeta { get; set; }
    public decimal IvaNeto { get; set; }
}

public sealed class IvaOperaciones
{
    public decimal BaseGravada { get; set; }
    public decimal BaseExentaConDerecho { get; set; }
    public decimal BaseExentaSinDerecho { get; set; }
    public decimal BaseNoSujeta { get; set; }
    public decimal BaseSinClasificar { get; set; }
    public decimal BaseExonerada { get; set; }
    public decimal BaseGravadaParcial { get; set; }
    public decimal BaseExoneradaSinRegla { get; set; }
}

public sealed class IvaDetalleFila
{
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Clave { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string CodigoTarifa { get; set; } = "";
    public decimal Porcentaje { get; set; }
    public decimal Base { get; set; }
    public decimal Iva { get; set; }
    public bool MonedaExtranjera { get; set; }
    /// <summary>Id de la venta (factura/tiquete) o de la devolución (nota de crédito).</summary>
    public long IdDocumento { get; set; }
    public bool EsNotaCredito { get; set; }
    public long CodCliente { get; set; }
    public string Cedula { get; set; } = "";
    public string NombreFantasia { get; set; } = "";
    public string Ubicacion { get; set; } = "";
}

/// <summary>Artículo de un documento. Importes en colones y con signo (la nota de crédito resta). Costo y utilidad solo para Super Administración.</summary>
public class IvaLinea
{
    public string CodArticulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
    public decimal PorcentajeImpuesto { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public bool EsBonificacion { get; set; }
    public decimal? Costo { get; set; }
    public decimal? Utilidad { get; set; }
}

public sealed class IvaDocumentoDetalle
{
    public string Tipo { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Clave { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = "";
    public string Cedula { get; set; } = "";
    public string NombreFantasia { get; set; } = "";
    public string Ubicacion { get; set; } = "";
    public bool MonedaExtranjera { get; set; }
    public bool IncluyeCosto { get; set; }
    public List<IvaLinea> Lineas { get; set; } = new();
    public decimal TotalSubtotal { get; set; }
    public decimal TotalDescuento { get; set; }
    public decimal TotalImpuesto { get; set; }
    public decimal TotalVenta { get; set; }
    public decimal? TotalCosto { get; set; }
    public decimal? TotalUtilidad { get; set; }
}

public sealed class IvaLineaPeriodo : IvaLinea
{
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string Cedula { get; set; } = "";
}

public sealed class IvaLineasPeriodo
{
    public bool IncluyeCosto { get; set; }
    public bool Truncado { get; set; }
    public List<IvaLineaPeriodo> Lineas { get; set; } = new();
}

public sealed class IvaRepercutidoResultado
{
    public int IdEmpresa { get; set; }
    public string Emisor { get; set; } = "";
    public string Identificacion { get; set; } = "";
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public List<IvaTarifaFila> Tarifas { get; set; } = new();
    public IvaOperaciones Operaciones { get; set; } = new();
    public decimal TotalBaseNeta { get; set; }
    public decimal TotalIvaNeto { get; set; }
    public decimal IvaBonificacionesAsumido { get; set; }
    public decimal IvaSegunEncabezados { get; set; }
    public List<string> Advertencias { get; set; } = new();
    public List<IvaDetalleFila> Detalle { get; set; } = new();
}

public sealed class IvaSoportadoFila
{
    public DateTime Fecha { get; set; }
    public string Origen { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Proveedor { get; set; } = "";
    public string Clave { get; set; } = "";
    public string Condicion { get; set; } = "";
    public string Estado { get; set; } = "";
    public decimal IvaTotal { get; set; }
    public decimal IvaAcreditable { get; set; }
    public decimal IvaSujetoProrrata { get; set; }
    public decimal IvaNoAcreditable { get; set; }
    public decimal IvaPendiente { get; set; }
    public bool MonedaExtranjera { get; set; }
    /// <summary>compra | importacion | devolucion.</summary>
    public string Clase { get; set; } = "";
    public long IdDocumento { get; set; }
    public int CodigoProveedor { get; set; }
    public string Cedula { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Telefono { get; set; } = "";
}

/// <summary>Artículo de un documento del IVA soportado. Importes en colones y con signo (la devolución resta).</summary>
public class IvaSoportadoLinea
{
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
    public decimal PorcentajeImpuesto { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
}

public sealed class IvaSoportadoDocumento
{
    public string Origen { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Clave { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string Proveedor { get; set; } = "";
    public string Cedula { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Motivo { get; set; } = "";
    public string Nota { get; set; } = "";
    public bool MonedaExtranjera { get; set; }
    public List<IvaSoportadoLinea> Lineas { get; set; } = new();
    public decimal TotalDescuento { get; set; }
    public decimal TotalSubtotal { get; set; }
    public decimal TotalImpuesto { get; set; }
    public decimal TotalDocumento { get; set; }
}

public sealed class IvaSoportadoLineaPeriodo : IvaSoportadoLinea
{
    public DateTime Fecha { get; set; }
    public string Origen { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Proveedor { get; set; } = "";
    public string Cedula { get; set; } = "";
}

public sealed class IvaSoportadoLineasPeriodo
{
    public bool Truncado { get; set; }
    public List<IvaSoportadoLineaPeriodo> Lineas { get; set; } = new();
}

public sealed class IvaSoportadoOrigen
{
    public string Origen { get; set; } = "";
    public int Documentos { get; set; }
    public decimal IvaTotal { get; set; }
    public decimal IvaAcreditable { get; set; }
    public decimal IvaSujetoProrrata { get; set; }
    public decimal IvaNoAcreditable { get; set; }
    public decimal IvaPendiente { get; set; }
}

public sealed class IvaSoportadoResultado
{
    public int IdEmpresa { get; set; }
    public string Emisor { get; set; } = "";
    public string Identificacion { get; set; } = "";
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public List<IvaSoportadoOrigen> PorOrigen { get; set; } = new();
    public decimal IvaTotal { get; set; }
    public decimal IvaAcreditable { get; set; }
    public decimal IvaBienesCapital { get; set; }
    public decimal IvaSujetoProrrata { get; set; }
    public decimal IvaNoAcreditable { get; set; }
    public decimal IvaPendiente { get; set; }
    public decimal IvaDevolucionesCompra { get; set; }
    public decimal IvaAcreditableNeto { get; set; }
    public List<string> Advertencias { get; set; } = new();
    public List<IvaSoportadoFila> Detalle { get; set; } = new();
}

public sealed class IvaProrrataParametro
{
    public int IdEmpresa { get; set; }
    public int Anio { get; set; }
    public decimal? PorcentajeProvisional { get; set; }
    public decimal? PorcentajeDefinitivo { get; set; }
    public bool IncluirNoSujetasEnDenominador { get; set; }
    public bool RedondearEnteroSuperior { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class IvaProrrataMes
{
    public int Mes { get; set; }
    public decimal BaseConDerecho { get; set; }
    public decimal BaseSinDerecho { get; set; }
    public decimal BaseNoSujeta { get; set; }
    public decimal IvaSujetoProrrata { get; set; }
    public decimal CreditoProvisional { get; set; }
}

public sealed class IvaProrrataResultado
{
    public int IdEmpresa { get; set; }
    public string Emisor { get; set; } = "";
    public string Identificacion { get; set; } = "";
    public int Anio { get; set; }
    public int HastaMes { get; set; }
    public IvaProrrataParametro Parametros { get; set; } = new();
    public decimal PorcentajeProvisional { get; set; }
    public string OrigenProvisional { get; set; } = "";
    public bool Aplica { get; set; }
    public List<IvaProrrataMes> Meses { get; set; } = new();
    public decimal BaseConDerecho { get; set; }
    public decimal BaseSinDerecho { get; set; }
    public decimal BaseNoSujeta { get; set; }
    public decimal PorcentajeAcumulado { get; set; }
    /// <summary>SIMPLE | TARIFAS_DIFERENCIADAS.</summary>
    public string Metodo { get; set; } = "SIMPLE";
    public List<IvaCreditoVenta> VentasPorClase { get; set; } = new();
    public List<IvaCompraTarifa> ComprasPorTarifa { get; set; } = new();
    public decimal IvaAlCostoGasto { get; set; }
    public List<IvaProrrataDeclarado> Declarados { get; set; } = new();
    public decimal CreditoAplicado { get; set; }
    public bool AplicadoEsDeclarado { get; set; }
    public int VersionLiquidacion { get; set; }
    public decimal? PorcentajeDefinitivo { get; set; }
    public string OrigenDefinitivo { get; set; } = "";
    public decimal IvaSujetoProrrataAcumulado { get; set; }
    public decimal CreditoProvisionalAcumulado { get; set; }
    public decimal CreditoSegunAcumulado { get; set; }
    public decimal? CreditoDefinitivo { get; set; }
    public decimal? Regularizacion { get; set; }
    public List<string> Advertencias { get; set; } = new();
}

public sealed class IvaCreditoVenta
{
    public string Clase { get; set; } = "";
    public decimal PorcentajeTarifa { get; set; }
    public decimal Base { get; set; }
    public decimal Participacion { get; set; }
}

public sealed class IvaCompraTarifa
{
    public decimal PorcentajeTarifa { get; set; }
    public decimal Iva { get; set; }
    public decimal Credito { get; set; }
    public decimal PorcentajeCredito { get; set; }
    public bool Supuesta { get; set; }
}

public sealed class IvaProrrataDeclarado
{
    public int Mes { get; set; }
    public decimal CreditoDirecto { get; set; }
    public decimal CreditoIndirecto { get; set; }
}

public sealed class IvaProrrataDeclarados
{
    public int IdEmpresa { get; set; }
    public int Anio { get; set; }
    public List<IvaProrrataDeclarado> Meses { get; set; } = new();
}

public sealed class IvaProrrataLiquidacion
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public int Anio { get; set; }
    public int Version { get; set; }
    public string Metodo { get; set; } = "";
    public decimal PorcentajeDefinitivo { get; set; }
    public decimal IvaSujetoProrrata { get; set; }
    public decimal CreditoDefinitivo { get; set; }
    public decimal CreditoAplicado { get; set; }
    public bool AplicadoEsDeclarado { get; set; }
    public decimal Diferencia { get; set; }
    public decimal IvaAlCostoGasto { get; set; }
    public string? Observaciones { get; set; }
    public string? Usuario { get; set; }
    public DateTime FechaUtc { get; set; }
}

public sealed class CongelarLiquidacion
{
    public int IdEmpresa { get; set; }
    public int Anio { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class PorcentajeProrrata
{
    public decimal Porcentaje { get; set; }
    public string Origen { get; set; } = "";
    public int Anio { get; set; }
}
