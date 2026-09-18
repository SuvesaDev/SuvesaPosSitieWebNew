namespace SuvesaPosSitioAplicacion.DTOs.Comisiones;

public sealed class FiltroComisionesDTO
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? IdSucursal { get; set; }
    public string? EstadoLiquidacion { get; set; }
}

public sealed class ComisionMovimientoDTO
{
    public long Id { get; set; }
    public long IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public string Beneficiario { get; set; } = string.Empty;
    public string TipoBeneficiario { get; set; } = string.Empty;
    public string Articulo { get; set; } = string.Empty;
    public decimal Base { get; set; }
    public decimal Porcentaje { get; set; }
    public decimal Importe { get; set; }
    public string EstadoFiscal { get; set; } = string.Empty;
    public string EstadoCobro { get; set; } = string.Empty;
    public string EstadoLiquidacion { get; set; } = string.Empty;
    public long? IdCorte { get; set; }
}

public sealed class ResumenComisionDTO
{
    public string Beneficiario { get; set; } = string.Empty;
    public string TipoBeneficiario { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public int Movimientos { get; set; }
}

public sealed class ComisionConsultaDTO
{
    public List<ComisionMovimientoDTO> Movimientos { get; set; } = new();
    public List<ResumenComisionDTO> Resumen { get; set; } = new();
}

public sealed class CrearCorteComisionesDTO
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int? IdSucursal { get; set; }
}

public sealed class CorteComisionesDTO
{
    public long IdCorte { get; set; }
    public int Movimientos { get; set; }
    public decimal Total { get; set; }
}

public sealed class ArchivoComisionesDTO
{
    public byte[] Contenido { get; set; } = Array.Empty<byte>();
    public string Nombre { get; set; } = "comisiones.xlsx";
    public string TipoContenido { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
