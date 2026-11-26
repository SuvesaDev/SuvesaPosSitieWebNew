namespace SuvesaPosSitioAplicacion.DTOs.Ventas;

// Espejo de los DTO de api/notas-credito-externas (ApiSuvesaPos/DTOs/NotaCreditoExternaDTOs.cs). A mano mientras los contratos NSwag
// no se regeneren contra el API que ya los trae.

public sealed class ValidarNotaCreditoExternaWebComando
{
    public int IdEmisor { get; set; }
    public int IdSucursal { get; set; }
    public string? XmlFactura { get; set; }
    public string? XmlAcuse { get; set; }
}

public sealed class LineaNotaCreditoExternaWebDTO
{
    public int NumeroLinea { get; set; }
    public string CodigoCabys { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tarifa { get; set; }
    public decimal MontoImpuesto { get; set; }
    public decimal MontoTotalLinea { get; set; }
}

public sealed class ClienteCandidatoNotaCreditoExternaWebDTO
{
    public long IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    /// <summary>false = inactivo (p. ej. unificado en otro): no se puede elegir.</summary>
    public bool Activo { get; set; }
    public string? Nota { get; set; }
}

public sealed class VentaCandidataNotaCreditoExternaWebDTO
{
    public long IdVenta { get; set; }
    public double NumFactura { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public long IdCliente { get; set; }
    public bool TieneDevolucionInterna { get; set; }
    /// <summary>"Alta" si coinciden número y total; "Media" si solo uno de los dos.</summary>
    public string Coincidencia { get; set; } = string.Empty;
}

public sealed class ValidacionNotaCreditoExternaWebDTO
{
    public bool Valido { get; set; }
    public List<string> Errores { get; set; } = new();
    /// <summary>La clave ya es un comprobante electrónico de este sistema: su nota se hace desde Devoluciones.</summary>
    public string? Bloqueo { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string Consecutivo { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public string ReceptorIdentificacion { get; set; } = string.Empty;
    public string ReceptorNombre { get; set; } = string.Empty;
    public decimal TotalOrigen { get; set; }
    public decimal TotalImpuesto { get; set; }
    public decimal AcreditadoPrevio { get; set; }
    public decimal Disponible { get; set; }
    public List<LineaNotaCreditoExternaWebDTO> Lineas { get; set; } = new();
    public List<ClienteCandidatoNotaCreditoExternaWebDTO> ClientesCandidatos { get; set; } = new();
    public List<VentaCandidataNotaCreditoExternaWebDTO> VentasCandidatas { get; set; } = new();
}

public sealed class CrearNotaCreditoExternaWebComando
{
    public string? ClaveIdempotencia { get; set; }
    public int IdEmisor { get; set; }
    public int IdSucursal { get; set; }
    public string? XmlFactura { get; set; }
    public string? XmlAcuse { get; set; }
    public long IdCliente { get; set; }
    public long? IdVenta { get; set; }
    /// <summary>null = todo lo disponible del documento.</summary>
    public decimal? Monto { get; set; }
    public string? Razon { get; set; }
    public bool SoloFiscal { get; set; }
    public string? MotivoSoloFiscal { get; set; }
}

public sealed class NotaCreditoExternaWebDTO
{
    public long Id { get; set; }
    public string? NombreCliente { get; set; }
    public string? Razon { get; set; }
    public string? MensajeMh { get; set; }
    public string ClaveOrigen { get; set; } = string.Empty;
    public string Clavedgt { get; set; } = string.Empty;
    public string Consecutivodgt { get; set; } = string.Empty;
    public string Estadodgt { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public long IdCliente { get; set; }
    public long? IdVenta { get; set; }
    public bool EsSoloFiscal { get; set; }
    public string? MotivoSoloFiscal { get; set; }
    public bool FueReintento { get; set; }
}

public sealed class PaginaNotasCreditoExternasWebDTO
{
    public int Total { get; set; }
    public List<NotaCreditoExternaWebDTO> Items { get; set; } = new();
}
