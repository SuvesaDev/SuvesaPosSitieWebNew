namespace SuvesaPosSitioAplicacion.DTOs.Ventas;

/// <summary>Preventa pendiente de pago de un cliente (ABONO_COBRAR_PREVENTAS_WEB.md).</summary>
public sealed class PreventaResumenWebDTO
{
    public long Id { get; set; }
    public string? NumFactura { get; set; }
    public long CodCliente { get; set; }
    public string? Cliente { get; set; }
    public string? Cedula { get; set; }
    public DateTime Fecha { get; set; }
    public int CodMoneda { get; set; }
    public string? Moneda { get; set; }
    public double SubTotal { get; set; }
    public double Descuento { get; set; }
    public double Impuesto { get; set; }
    public double Total { get; set; }
    public int IdSerie { get; set; }
    public int Tipo { get; set; }
    public string? TipoFacturaDescripcion { get; set; }
    /// <summary>"01" = factura electrónica, "04" = tiquete, null = no electrónica.</summary>
    public string? CodigoFe { get; set; }
    public bool EsCredito { get; set; }
    public bool EmisionV44Habilitada { get; set; }
    public int Ficha { get; set; }
    public bool EsConsignacion { get; set; }

    /// <summary>Slug de impresión / tipo de comprobante según el CodigoFe de la serie.</summary>
    public string? SlugImpresion => CodigoFe switch
    {
        "04" => "tiquete-electronico",
        "01" => "factura-electronica",
        _ => null,
    };
}

/// <summary>Resultado de la emisión síncrona de un comprobante desde una venta POS.</summary>
public sealed class ResultadoEmisionWebDTO
{
    public bool EsValido { get; set; }
    public string? Estado { get; set; }
    public string? Clave { get; set; }
    public IReadOnlyList<string> Errores { get; set; } = Array.Empty<string>();
}
