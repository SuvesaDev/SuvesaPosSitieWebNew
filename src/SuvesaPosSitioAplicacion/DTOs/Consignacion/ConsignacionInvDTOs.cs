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
    /// <summary>Centro al que queda ligada la bodega del cliente (el de la sesión).</summary>
    public int? IdSucursal { get; set; }
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
    /// <summary>true = conteo completo: el API exige que vengan todas las líneas de Existencia (§3.7).</summary>
    public bool Completo { get; set; }
    public List<ConteoConsignacionLineaEntrada> Lineas { get; set; } = new();
}

// --- Existencia de la bodega del cliente, para precargar el conteo (§3.7) ---

public sealed class ExistenciaConsignacionRequest
{
    public long IdCliente { get; set; }
}

public sealed class ExistenciaConsignacionLinea
{
    public long IdArticulo { get; set; }
    public string CodArticulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public bool ManejaLote { get; set; }
    public long? IdStockLote { get; set; }
    public string? NumeroLote { get; set; }
    public DateTime? Vencimiento { get; set; }
    public double Existencia { get; set; }
}

public sealed class ExistenciaConsignacion
{
    public long IdCliente { get; set; }
    public string NombreCliente { get; set; } = "";
    public int IdBodega { get; set; }
    public string NombreBodega { get; set; } = "";
    public bool BodegaAbierta { get; set; }
    public List<ExistenciaConsignacionLinea> Articulos { get; set; } = new();
}

// --- Bodega de consignación central + reposición (§3.5 / §6.5) ---

public sealed class BodegaCentralConsignacion
{
    public int IdBodega { get; set; }
    public string NombreBodega { get; set; } = "";
    public int? IdSucursal { get; set; }
    public double ExistenciaTotal { get; set; }
}

public sealed class AbrirBodegaCentralConsignacion
{
    public int? IdSucursal { get; set; }
}

public sealed class ReponerCentralConsignacionLineaRequest
{
    public long IdArticulo { get; set; }
    public long? IdStockLote { get; set; }
    public double Cantidad { get; set; }
}

public sealed class ReponerCentralConsignacionRequest
{
    public int? IdSucursal { get; set; }
    public int IdBodegaOrigen { get; set; }
    public string? Documento { get; set; }
    public string? Observaciones { get; set; }
    public List<ReponerCentralConsignacionLineaRequest> Lineas { get; set; } = new();
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

// --- Prefactura (es una Venta con estado) ---

public sealed class GenerarPrefacturaConsignacion
{
    public long IdConteo { get; set; }
    /// <summary>La arma el sitio con el contexto de facturación (empresa/sucursal/caja/moneda) y el detalle con bonificación.</summary>
    public SuvesaPosSitioAplicacion.DTOs.Generated.FacturaDTO Factura { get; set; } = new();
}

public sealed class EditarPrefacturaConsignacion
{
    public long IdPrefactura { get; set; }
    public SuvesaPosSitioAplicacion.DTOs.Generated.FacturaDTO Factura { get; set; } = new();
}

public sealed class FacturarPrefacturaConsignacion
{
    public long IdPrefactura { get; set; }
    /// <summary>1 = Contado, 2 = Crédito.</summary>
    public int Condicion { get; set; }
    public int? IdPlazo { get; set; }
}

public sealed class AnularPrefacturaConsignacion
{
    public long IdPrefactura { get; set; }
    public string Motivo { get; set; } = "";
}

public sealed class PrefacturaConsignacionLinea
{
    public long IdArticulo { get; set; }
    public string CodArticulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string? Lote { get; set; }
    public double Cantidad { get; set; }
    public double PrecioUnit { get; set; }
    public double Descuento { get; set; }
    public double MontoDescuento { get; set; }
    public double MontoImpuesto { get; set; }
    public double SubTotal { get; set; }
    public bool EsBonificacion { get; set; }
}

public sealed class PrefacturasConsignacionFiltro
{
    /// <summary>1 Editable, 2 Aprobada, 3 Facturada, 4 Anulada. null = todas.</summary>
    public int? Estado { get; set; }
    public long? IdCliente { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}

public sealed class PrefacturaConsignacionResumen
{
    public long Id { get; set; }
    public double NumFactura { get; set; }
    public long IdCliente { get; set; }
    public string NombreCliente { get; set; } = "";
    public DateTime Fecha { get; set; }
    public int Estado { get; set; }
    public string EstadoDescripcion { get; set; } = "";
    public double Total { get; set; }
}

public sealed class PrefacturaConsignacion
{
    public long Id { get; set; }
    public double NumFactura { get; set; }
    public long IdCliente { get; set; }
    public string NombreCliente { get; set; } = "";
    public long? IdConteoConsignacion { get; set; }
    public DateTime Fecha { get; set; }
    public int Estado { get; set; }
    public string EstadoDescripcion { get; set; } = "";
    public int Condicion { get; set; }
    public int? IdPlazo { get; set; }
    public double SubTotal { get; set; }
    public double Descuento { get; set; }
    public double ImpVenta { get; set; }
    public double Total { get; set; }
    public string? Observaciones { get; set; }
    public List<PrefacturaConsignacionLinea> Lineas { get; set; } = new();
}
