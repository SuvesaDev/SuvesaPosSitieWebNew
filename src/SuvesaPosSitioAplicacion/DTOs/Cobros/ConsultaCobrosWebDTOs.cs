namespace SuvesaPosSitioAplicacion.DTOs.Cobros;

/// <summary>Recibo de cobro emitido — espejo de <c>GET api/cobros/recibos</c> (SANEAMIENTO Fase 8.2).</summary>
public sealed class ReciboCobroResumenWebDTO
{
    public long Id { get; set; }
    public long NumeroRecibo { get; set; }
    public string? Prefijo { get; set; }
    public string NumeroFormateado { get; set; } = "";
    public long IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int IdSucursal { get; set; }
    public long NumApertura { get; set; }
    public int CodMonedaBase { get; set; }
    public decimal Total { get; set; }
    public decimal Vuelto { get; set; }
    public int Estado { get; set; }
    public string EstadoNombre { get; set; } = "";
    public DateTime FechaUtc { get; set; }
    public string Usuario { get; set; } = "";
    public int CantidadFacturas { get; set; }
    public string FormasPagoResumen { get; set; } = "";
    public string? MotivoAnulacion { get; set; }
    public DateTime? FechaAnulacionUtc { get; set; }
    public List<ReciboCobroAplicacionWebDTO> Aplicaciones { get; set; } = new();
    public List<ReciboCobroFormaPagoWebDTO> FormasPago { get; set; } = new();
}

public sealed class ReciboCobroAplicacionWebDTO
{
    public long IdVenta { get; set; }
    public double NumFactura { get; set; }
    public string? ConsecutivoMh { get; set; }
    public decimal MontoAplicado { get; set; }
    public int CodMoneda { get; set; }
    public decimal TipoCambio { get; set; }
}

public sealed class ReciboCobroFormaPagoWebDTO
{
    public int IdFormaPago { get; set; }
    public string CodigoFormaPago { get; set; } = "";
    public decimal MontoRecibido { get; set; }
    public decimal MontoAplicado { get; set; }
    public decimal Vuelto { get; set; }
    public int CodMoneda { get; set; }
    public decimal TipoCambio { get; set; }
    public string? Referencia { get; set; }
}

/// <summary>
/// Operación fallida — espejo de <c>GET api/cobros/operaciones-fallidas</c>:
/// comprobante electrónico rechazado por Hacienda cuya venta se cobró localmente.
/// El cobro nunca se borra (D10).
/// </summary>
public sealed class OperacionFallidaWebDTO
{
    public string Clave { get; set; } = "";
    public string TipoComprobante { get; set; } = "";
    public string? ConsecutivoMh { get; set; }
    public string EstadoHacienda { get; set; } = "";
    public string? CausaError { get; set; }
    public DateTime FechaActualizacionUtc { get; set; }
    public int IntentosEnvio { get; set; }
    public int? IdEmisor { get; set; }
    public string? OrigenTipo { get; set; }
    public long? OrigenId { get; set; }
    public double? NumFactura { get; set; }
    public long? IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public decimal? MontoFactura { get; set; }
    public long? NumApertura { get; set; }
    public bool TieneCobroLocal { get; set; }
    public decimal MontoCobradoLocal { get; set; }
    public List<long> IdsCobro { get; set; } = new();
    public string AccionSugerida { get; set; } = "";
}

/// <summary>
/// Perfil de emisión elegible — espejo de
/// <c>GET api/facturacion/perfiles-emision/elegibles</c> (SANEAMIENTO Fase 8.3).
/// </summary>
public sealed class PerfilEmisionElegibleWebDTO
{
    public int IdSerie { get; set; }
    public string Descripcion { get; set; } = "";
    public int NumeroTerminal { get; set; }
    public int? IdTipoFactura { get; set; }
    public string? CodigoFe { get; set; }
    public string? TipoNombre { get; set; }
    public bool EsCredito { get; set; }
    public bool EsRecibo { get; set; }
    public bool EsPago { get; set; }
    public bool EsConsignacion { get; set; }
    public bool EmisionV44Habilitada { get; set; }
    public bool Elegible { get; set; }
    public string? MotivoNoElegible { get; set; }
}
