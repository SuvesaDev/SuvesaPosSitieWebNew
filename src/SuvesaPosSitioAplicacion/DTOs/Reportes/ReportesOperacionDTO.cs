namespace SuvesaPosSitioAplicacion.DTOs.Reportes;

public sealed class FiltroReporteOperacionWebDTO
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public DateTime? FechaReferencia { get; set; }
    public int? IdSucursal { get; set; }
    public int? IdEmpresa { get; set; }
    public long? IdCliente { get; set; }
    public int? IdProveedor { get; set; }
    public long? IdArticulo { get; set; }
    public int? IdBodega { get; set; }
    public string? NumeroLote { get; set; }
    public string? Texto { get; set; }
    public string? IdAgente { get; set; }
    public int? IdFamilia { get; set; }
    /// <summary>"01" contado / "02" crédito.</summary>
    public string? TipoVenta { get; set; }
    public bool IncluirAnuladas { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 25;
}

public sealed class ReporteOperacionWebDTO
{
    public string Titulo { get; set; } = string.Empty;
    public string Fuente { get; set; } = string.Empty;
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public List<IndicadorReporteOperacionWebDTO> Indicadores { get; set; } = new();
    public List<FilaReporteOperacionWebDTO> Filas { get; set; } = new();
    public int TotalRegistros { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public bool Limitado { get; set; }
}

public sealed class IndicadorReporteOperacionWebDTO { public string Etiqueta { get; set; } = string.Empty; public decimal Valor { get; set; } public string? Formato { get; set; } }
public sealed class FilaReporteOperacionWebDTO
{
    public DateTime Fecha { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Sucursal { get; set; } = string.Empty;
    public string NombreFantasia { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal Monto { get; set; }
    public decimal Saldo { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string Estado { get; set; } = string.Empty;
    /// <summary>Clave del comprobante electrónico del proveedor (solo compras importadas
    /// de un XML de Hacienda la tienen).</summary>
    public string ClaveHacienda { get; set; } = string.Empty;
    /// <summary>Último estado del Mensaje Receptor enviado por esta compra — vacío si
    /// nunca se envió.</summary>
    public string EstadoHacienda { get; set; } = string.Empty;
    /// <summary>Condición del impuesto (01-05) enviada en el Mensaje Receptor, en texto.</summary>
    public string CondicionImpuestoHacienda { get; set; } = string.Empty;
}
