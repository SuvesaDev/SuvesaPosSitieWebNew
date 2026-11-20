namespace SuvesaPosSitioAplicacion.DTOs.Ventas;

/// <summary>Panorama de crédito del cliente (SANEAMIENTO Fase 2).</summary>
public sealed class CreditoClienteWebDTO
{
    public long IdCliente { get; set; }
    public string? Nombre { get; set; }
    public decimal LimiteAprobado { get; set; }
    public int PlazoDias { get; set; }
    public decimal SaldoAbierto { get; set; }
    /// <summary>Cubierto por recibos pendientes de aprobación: no es saldo pendiente, pero ocupa el límite hasta que el banco confirme.</summary>
    public decimal EnTransito { get; set; }
    public int FacturasEnTransito { get; set; }
    public decimal CreditoAFavor { get; set; }
    public decimal Disponible { get; set; }
    public bool Bloqueado { get; set; }
    public string? MotivoBloqueo { get; set; }
    /// <summary>0 ninguna, 1 cliente inactivo (nada lo habilita), 2 marca de moroso, 3 facturas vencidas.</summary>
    public int CausaBloqueo { get; set; }
    public int FacturasVencidas { get; set; }
    public int DiasMoraMaximo { get; set; }
    public decimal SaldoVencido { get; set; }
    public bool DesbloqueadoPorAutorizacion { get; set; }
}

/// <summary>Factura de crédito con saldo abierto.</summary>
public sealed class FacturaCreditoWebDTO
{
    public long IdVenta { get; set; }
    public double NumFactura { get; set; }
    public string? ConsecutivoMh { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime? Vence { get; set; }
    public int CodMoneda { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal NotasCreditoAplicadas { get; set; }
    public decimal PagosAplicados { get; set; }
    public decimal SaldoActual { get; set; }
    public string? EstadoMh { get; set; }
}

/// <summary>Comando de cobro de facturas de crédito.</summary>
public sealed class AplicarCreditoAFavorWebDTO
{
    public string? ClaveIdempotencia { get; set; }
    public long IdCliente { get; set; }
    public long IdVenta { get; set; }
    /// <summary>null = lo que alcance: el menor entre el crédito disponible y el saldo de la factura.</summary>
    public decimal? Monto { get; set; }
    public string Usuario { get; set; } = "";
}

public sealed class AplicarCreditoAFavorResultadoWebDTO
{
    public long IdCobro { get; set; }
    public long NumeroRecibo { get; set; }
    public decimal MontoAplicado { get; set; }
    public decimal SaldoNuevoFactura { get; set; }
    public decimal CreditoRestante { get; set; }
}

public sealed class DevolverCreditoAFavorWebDTO
{
    public string? ClaveIdempotencia { get; set; }
    public long IdCliente { get; set; }
    /// <summary>null = todo el crédito a favor disponible.</summary>
    public decimal? Monto { get; set; }
    public long IdApertura { get; set; }
    public int IdSucursal { get; set; }
    public long? NumCaja { get; set; }
    public string? CedulaCajero { get; set; }
    public string Usuario { get; set; } = "";
    public string? Motivo { get; set; }
}

public sealed class DevolverCreditoAFavorResultadoWebDTO
{
    public long IdMovimientoCaja { get; set; }
    public decimal MontoDevuelto { get; set; }
    public decimal CreditoRestante { get; set; }
}

public sealed class CobroCreditoComandoWebDTO
{
    public string? ClaveIdempotencia { get; set; }
    public long IdCliente { get; set; }
    public long IdApertura { get; set; }
    public int IdSucursal { get; set; }
    public long? NumCaja { get; set; }
    public string? CedulaCajero { get; set; }
    public string Usuario { get; set; } = "";
    public List<CobroCreditoFacturaWebDTO> Facturas { get; set; } = new();
    public List<CobroCreditoFormaPagoWebDTO> FormasPago { get; set; } = new();
    public bool PermitirParcial { get; set; } = true;
}

public sealed class CobroCreditoFacturaWebDTO
{
    public long IdVenta { get; set; }
    public decimal? Monto { get; set; }
    /// <summary>Descuento por pronto pago: solo si la factura es elegible; el recibo debe cancelarla completa.</summary>
    public bool AplicarProntoPago { get; set; }
}

/// <summary>Si una factura de crédito puede recibir el descuento por pronto pago (hasta 8 días de su emisión, sin abonos ni notas).</summary>
public sealed class EvaluacionProntoPagoWebDTO
{
    public long IdVenta { get; set; }
    public double NumFactura { get; set; }
    public bool Elegible { get; set; }
    public string Motivo { get; set; } = "";
    public int DiasTranscurridos { get; set; }
    public int DiasMaximos { get; set; }
    public decimal Porcentaje { get; set; }
    public decimal Descuento { get; set; }
    public decimal TotalAPagar { get; set; }
}

public sealed class CobroCreditoFormaPagoWebDTO
{
    public string CodigoFormaPago { get; set; } = "";
    public decimal MontoRecibido { get; set; }
    public string? Referencia { get; set; }
    /// <summary>Solo forma de pago Cheque: número del cheque del cliente.</summary>
    public string? NumeroCheque { get; set; }
    /// <summary>Solo forma de pago Cheque: banco emisor (catálogo EntidadesBancaria.IdBanco).</summary>
    public int? IdBanco { get; set; }
    /// <summary>Fecha del depósito o cheque de esta línea.</summary>
    public DateTime? FechaPago { get; set; }
}

public sealed class CobroCreditoResultadoWebDTO
{
    public long IdCobro { get; set; }
    public long NumeroRecibo { get; set; }
    public decimal TotalAplicado { get; set; }
    public decimal Vuelto { get; set; }
    public bool FueReintento { get; set; }
    public bool PendienteConfirmacion { get; set; }
    public List<DescuentoProntoPagoWebDTO> DescuentosProntoPago { get; set; } = new();
}

public sealed class DescuentoProntoPagoWebDTO
{
    public long IdVenta { get; set; }
    public decimal Descuento { get; set; }
    public long IdDevolucion { get; set; }
}
