namespace SuvesaPosSitioAplicacion.DTOs.Bodega;

public sealed class ReciboFiltro
{
    public int? Estado { get; set; }
    public string? Texto { get; set; }
}

public sealed class ReciboResumen
{
    public long Id { get; set; }
    public string TipoOrigen { get; set; } = "";
    public long IdDevolucion { get; set; }
    public long IdVenta { get; set; }
    public int Estado { get; set; }
    public string? EstadoDescripcion { get; set; }
    public DateTime Fecha { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? NombreCliente { get; set; }
    public long CodCliente { get; set; }
    public int Lineas { get; set; }
    public decimal CantidadDevuelta { get; set; }
    public decimal CantidadRecibida { get; set; }
}

public sealed class ReciboLinea
{
    public long Id { get; set; }
    public long IdArticulo { get; set; }
    public string CodArticulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public int IdBodega { get; set; }
    public string? NombreBodega { get; set; }
    /// <summary>Lote devuelto (el de la nota de crédito).</summary>
    public long? IdStockLote { get; set; }
    public string? NumeroLote { get; set; }
    /// <summary>El artículo maneja lotes: al recibir hay que indicar el lote.</summary>
    public bool ManejaLote { get; set; }
    public List<ReciboLoteOpcion> LotesDisponibles { get; set; } = new();
    public long? IdStockLoteRecibido { get; set; }
    public string? NumeroLoteRecibido { get; set; }
    public string? MotivoCambioLote { get; set; }
    public decimal CantidadDevuelta { get; set; }
    public decimal CantidadRecibida { get; set; }
}

public sealed class ReciboLoteOpcion
{
    public long Id { get; set; }
    public string Numero { get; set; } = "";
    public DateTime? FechaVencimiento { get; set; }
}

public sealed class ReciboOrden
{
    public long Id { get; set; }
    public string TipoOrigen { get; set; } = "";
    public long IdDevolucion { get; set; }
    public long IdVenta { get; set; }
    public int Estado { get; set; }
    public string? EstadoDescripcion { get; set; }
    public DateTime Fecha { get; set; }
    public long CodCliente { get; set; }
    public string? NombreCliente { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Motivo { get; set; }
    public string? UsuarioCreacion { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaRecibo { get; set; }
    public string? UsuarioRecibo { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    public List<ReciboLinea> Lineas { get; set; } = new();
}

public sealed class ReciboLineaRecibida
{
    public long IdLinea { get; set; }
    public decimal CantidadRecibida { get; set; }
    /// <summary>Lote existente en el que se recibe (sin valor: el lote devuelto).</summary>
    public long? IdStockLoteRecibido { get; set; }
    /// <summary>Lote nuevo: número y fecha de vencimiento (obligatoria).</summary>
    public string? NuevoLoteNumero { get; set; }
    public DateTime? NuevoLoteVencimiento { get; set; }
    /// <summary>Obligatorio si el lote recibido es distinto al devuelto.</summary>
    public string? MotivoCambioLote { get; set; }
}
