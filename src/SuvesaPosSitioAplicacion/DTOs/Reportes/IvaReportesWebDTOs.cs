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
    public decimal? PorcentajeDefinitivo { get; set; }
    public string OrigenDefinitivo { get; set; } = "";
    public decimal IvaSujetoProrrataAcumulado { get; set; }
    public decimal CreditoProvisionalAcumulado { get; set; }
    public decimal CreditoSegunAcumulado { get; set; }
    public decimal? CreditoDefinitivo { get; set; }
    public decimal? Regularizacion { get; set; }
    public List<string> Advertencias { get; set; } = new();
}

public sealed class PorcentajeProrrata
{
    public decimal Porcentaje { get; set; }
    public string Origen { get; set; } = "";
    public int Anio { get; set; }
}
