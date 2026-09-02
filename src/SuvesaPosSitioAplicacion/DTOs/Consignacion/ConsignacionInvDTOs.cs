namespace SuvesaPosSitioAplicacion.DTOs.Consignacion;

// Contratos a mano espejo de ConsignacionInventarioDTO.cs del API
// (CONSIGNACION_API.md §3). Se eliminan al regenerar NSwag.
// El cliente serializa con JsonSerializerDefaults.Web (camelCase, lectura
// insensible a mayúsculas), así que no hacen falta [JsonPropertyName].

// --- Bodega de consignación ---

public sealed class AbrirBodegaConsignacion
{
    public long IdCliente { get; set; }
    /// <summary>0 = crear una bodega física nueva dedicada al cliente.</summary>
    public int IdBodega { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class BodegasConsignacionFiltro
{
    public string? Texto { get; set; }
    public bool SoloCerradas { get; set; }
}

public sealed class BodegaConsignacionResumen
{
    public long Id { get; set; }
    public long IdCliente { get; set; }
    public string Cedula { get; set; } = "";
    public string NombreCliente { get; set; } = "";
    public int IdBodega { get; set; }
    public string NombreBodega { get; set; } = "";
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public double ExistenciaTotal { get; set; }
    public string Estado { get; set; } = "Activa";
}

// --- Boletas de ingreso / salida ---

public sealed class BoletaConsignacionLineaEntrada
{
    public long IdArticulo { get; set; }
    public long? IdStockLote { get; set; }
    public double Cantidad { get; set; }
}

public sealed class BoletaConsignacionRequest
{
    /// <summary>1 = Ingreso, 2 = Salida / retiro.</summary>
    public int Tipo { get; set; }
    public long IdCliente { get; set; }
    public string? Documento { get; set; }
    public string? Motivo { get; set; }
    public bool CierreTotal { get; set; }
    public string? Observaciones { get; set; }
    public List<BoletaConsignacionLineaEntrada> Lineas { get; set; } = new();
}

public sealed class AnularBoletaConsignacion
{
    public long IdBoleta { get; set; }
    public string Motivo { get; set; } = "";
}

public sealed class BoletaConsignacionLinea
{
    public long IdArticulo { get; set; }
    public string CodArticulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public long? IdStockLote { get; set; }
    public string? NumeroLote { get; set; }
    public double Cantidad { get; set; }
    public double CostoUnitario { get; set; }
    public double CostoLinea { get; set; }
}

public sealed class BoletaConsignacion
{
    public long Id { get; set; }
    public int Tipo { get; set; }
    public string TipoDescripcion { get; set; } = "";
    public long IdCliente { get; set; }
    public string NombreCliente { get; set; } = "";
    public int IdBodega { get; set; }
    public DateTime Fecha { get; set; }
    public string? Usuario { get; set; }
    public string? Documento { get; set; }
    public string? Motivo { get; set; }
    public bool CierreTotal { get; set; }
    public string? Observaciones { get; set; }
    public double CostoTotal { get; set; }
    public bool Anulada { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public string? UsuarioAnulacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    public List<BoletaConsignacionLinea> Lineas { get; set; } = new();
}

// --- Inventario físico / conteo ---

public sealed class ConteoConsignacionLineaEntrada
{
    public long IdArticulo { get; set; }
    public long? IdStockLote { get; set; }
    public double Fisico { get; set; }
    public double? PrecioUnitario { get; set; }
}

public sealed class ConteoConsignacionRequest
{
    public long IdCliente { get; set; }
    public string? Agente { get; set; }
    public string? Observaciones { get; set; }
    public List<ConteoConsignacionLineaEntrada> Lineas { get; set; } = new();
}

public sealed class ConteoConsignacionLinea
{
    public long IdArticulo { get; set; }
    public string CodArticulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public long? IdStockLote { get; set; }
    public string? NumeroLote { get; set; }
    public double Consignado { get; set; }
    public double Fisico { get; set; }
    public double Vendido { get; set; }
    public double Sobrante { get; set; }
    public double PrecioUnitario { get; set; }
}

public sealed class ConteoConsignacion
{
    public long Id { get; set; }
    public long IdCliente { get; set; }
    public string NombreCliente { get; set; } = "";
    public int IdBodega { get; set; }
    public DateTime Fecha { get; set; }
    public string? Agente { get; set; }
    public string? Usuario { get; set; }
    public int Estado { get; set; }
    public string EstadoDescripcion { get; set; } = "";
    public string? Observaciones { get; set; }
    public List<ConteoConsignacionLinea> Lineas { get; set; } = new();
}

// --- Kardex ---

public sealed class KardexConsignacionFiltro
{
    public long IdCliente { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}

public sealed class KardexConsignacionMovimiento
{
    public long IdStock { get; set; }
    public DateTime Fecha { get; set; }
    public int Movimiento { get; set; }
    public string MovimientoDescripcion { get; set; } = "";
    public string? Documento { get; set; }
    public string? Usuario { get; set; }
    public long IdArticulo { get; set; }
    public string CodArticulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public long? IdStockLote { get; set; }
    public string? NumeroLote { get; set; }
    public double ExistenciaAnterior { get; set; }
    public double Cantidad { get; set; }
    public double ExistenciaNueva { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class KardexConsignacion
{
    public long IdCliente { get; set; }
    public string NombreCliente { get; set; } = "";
    public double ExistenciaTotal { get; set; }
    public string Estado { get; set; } = "Activa";
    public List<KardexConsignacionMovimiento> Movimientos { get; set; } = new();
}
