namespace SuvesaPosSitioAplicacion.DTOs.Compras;

/// <summary>Alta de una orden de compra dirigida a un proveedor (espejo de <c>api/ordenes-compra</c>).</summary>
public sealed class CrearOrdenCompraWebDTO
{
    public int IdProveedor { get; set; }
    public int IdSucursal { get; set; }
    public int? IdEmisor { get; set; }
    public DateTime? Fecha { get; set; }
    public int CodMoneda { get; set; } = 1;
    public double TipoCambio { get; set; } = 1;
    public bool Credito { get; set; }
    public int Plazo { get; set; }
    public string? Observaciones { get; set; }
    public string? Entregar { get; set; }
    public List<LineaOrdenCompraWebDTO> Lineas { get; set; } = new();
}

public sealed class LineaOrdenCompraWebDTO
{
    public long CodArticulo { get; set; }
    public string? Descripcion { get; set; }
    public double Cantidad { get; set; }
    public double CostoUnitario { get; set; }
    public double PorcDescuento { get; set; }
    public double PorcImpuesto { get; set; }
}

public sealed class OrdenCompraFlujoWebDTO
{
    public long Orden { get; set; }
    public long? Consecutivo { get; set; }
    public int IdProveedor { get; set; }
    public string? NombreProveedor { get; set; }
    public string? CorreoProveedor { get; set; }
    public DateTime Fecha { get; set; }
    public int IdSucursal { get; set; }
    public int? IdEmisor { get; set; }
    public int CodMoneda { get; set; }
    public string MonedaNombre { get; set; } = "CRC";
    public double TipoCambio { get; set; } = 1;
    public bool Credito { get; set; }
    public int Plazo { get; set; }
    public double SubTotal { get; set; }
    public double Descuento { get; set; }
    public double Impuesto { get; set; }
    public double Total { get; set; }
    public double TotalColones { get; set; }
    public string? Observaciones { get; set; }
    public bool Anulado { get; set; }
    public int EstadoSeguimiento { get; set; }
    public string EstadoSeguimientoNombre { get; set; } = "";
    public DateTime? FechaEntrega { get; set; }
    public long? IdFacturaCompra { get; set; }
    public string? MotivoCierre { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string? UsuarioCierre { get; set; }
    public DateTime? CorreoEnviadoUtc { get; set; }
    public string? CorreoDestino { get; set; }
    public string Usuario { get; set; } = "";
    public List<LineaOrdenCompraDetalleWebDTO> Lineas { get; set; } = new();
}

public sealed class LineaOrdenCompraDetalleWebDTO
{
    public long Id { get; set; }
    public long CodArticulo { get; set; }
    public string Descripcion { get; set; } = "";
    public double Cantidad { get; set; }
    public double CostoUnitario { get; set; }
    public double PorcDescuento { get; set; }
    public double Descuento { get; set; }
    public double PorcImpuesto { get; set; }
    public double Impuesto { get; set; }
    public double TotalLinea { get; set; }
}

public sealed class ResultadoEnvioOrdenCompraWebDTO
{
    public bool Enviado { get; set; }
    public string? Destino { get; set; }
    public string? Error { get; set; }
}
