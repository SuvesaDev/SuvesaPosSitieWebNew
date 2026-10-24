namespace SuvesaPosSitioAplicacion.DTOs.Bodega;

public sealed class AlistamientoFiltro
{
    public int? Estado { get; set; }
    public string? Texto { get; set; }
    public int? IdSucursal { get; set; }
    /// <summary>1 = alta (crédito), 0 = normal. Sin valor: todas.</summary>
    public int? Prioridad { get; set; }
    /// <summary>true = solo tiquetes, false = solo facturas. Sin valor: todos.</summary>
    public bool? EsTiquete { get; set; }
}

public sealed class AlistamientoResumen
{
    public long Id { get; set; }
    public long IdVenta { get; set; }
    /// <summary>Boleta de consignación (envío de mercadería al cliente); null si viene de una venta.</summary>
    public long? IdBoleta { get; set; }
    public bool EsConsignacionEnvio { get; set; }
    public int Estado { get; set; }
    public string? EstadoDescripcion { get; set; }
    public int Prioridad { get; set; }
    public DateTime Fecha { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? NombreCliente { get; set; }
    public long CodCliente { get; set; }
    public bool EsCredito { get; set; }
    public bool EsPreventa { get; set; }
    public bool EsTiquete { get; set; }
    public DateTime? FechaAceptacion { get; set; }
    public string? UsuarioAlistamiento { get; set; }
    public int Lineas { get; set; }
    public int LineasCompletas { get; set; }
    public int Bultos { get; set; }
    public decimal CantidadFacturada { get; set; }
    public decimal CantidadAlistada { get; set; }
}

public sealed class AlistamientoOrden
{
    public long Id { get; set; }
    public long IdVenta { get; set; }
    /// <summary>Boleta de consignación (envío de mercadería al cliente); null si viene de una venta.</summary>
    public long? IdBoleta { get; set; }
    public bool EsConsignacionEnvio { get; set; }
    public int Estado { get; set; }
    public string? EstadoDescripcion { get; set; }
    public int Prioridad { get; set; }
    public DateTime Fecha { get; set; }
    public string? NumeroDocumento { get; set; }
    public long CodCliente { get; set; }
    public string? NombreCliente { get; set; }
    public bool EsCredito { get; set; }
    public bool EsPreventa { get; set; }
    public string? UsuarioAlistamiento { get; set; }
    public string? Observaciones { get; set; }
    public string? MotivoFaltante { get; set; }
    public string? RecibidoPor { get; set; }
    public string? FirmaPng { get; set; }
    public int? DiasMinimosCaducidad { get; set; }
    public List<AlistamientoLinea> Lineas { get; set; } = new();
    public List<AlistamientoScan> Scans { get; set; } = new();
    public List<BultoDespacho> Bultos { get; set; } = new();
}

public sealed class AlistamientoLinea
{
    public long Id { get; set; }
    public long IdArticulo { get; set; }
    public string? CodArticulo { get; set; }
    public string? Descripcion { get; set; }
    public decimal CantidadFacturada { get; set; }
    public decimal CantidadAlistada { get; set; }
    public string? LoteFacturado { get; set; }
    public DateTime? VencimientoFacturado { get; set; }
    public int Estado { get; set; }
    public string? EstadoDescripcion { get; set; }
    public bool AutorizoFaltante { get; set; }
}

public sealed class AlistamientoScan
{
    public long Id { get; set; }
    public long IdLinea { get; set; }
    public string? CodigoCrudo { get; set; }
    public string? Lote { get; set; }
    public DateTime? Vencimiento { get; set; }
    public decimal Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public string? Usuario { get; set; }
    public bool LoteDistintoAlFacturado { get; set; }
    public bool IngresoManual { get; set; }
    public string? UsuarioAutorizo { get; set; }
}

public sealed class BultoDespacho
{
    public long Id { get; set; }
    public long IdOrden { get; set; }
    public int Numero { get; set; }
    public string? Codigo { get; set; }
    public int Estado { get; set; }
    public string? EstadoDescripcion { get; set; }
    public DateTime Fecha { get; set; }
    public decimal PesoKg { get; set; }
    public string? NombreCliente { get; set; }
    public string? NumeroDocumento { get; set; }
    /// <summary>Zona del cliente: ayuda para armar la ruta, no obliga.</summary>
    public int? IdProvincia { get; set; }
    public int? IdCanton { get; set; }
    public int? IdDistrito { get; set; }
    public string? Zona { get; set; }
    public string? DireccionCliente { get; set; }
}

public sealed class RutaDespacho
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Observaciones { get; set; }
    public bool Activa { get; set; } = true;
    public List<RutaZona> Zonas { get; set; } = new();

    /// <summary>¿El cliente de este bulto cae dentro de las zonas de la ruta? Sin zonas definidas no se puede saber.</summary>
    public bool CubreA(BultoDespacho b) => Zonas.Any(z =>
        b.IdProvincia == z.IdProvincia
        && (z.IdCanton is not > 0 || b.IdCanton == z.IdCanton)
        && (z.IdDistrito is not > 0 || b.IdDistrito == z.IdDistrito));
}

/// <summary>Zona de una ruta: provincia completa, o un cantón o distrito concreto.</summary>
public sealed class RutaZona
{
    public int IdProvincia { get; set; }
    public int? IdCanton { get; set; }
    public int? IdDistrito { get; set; }
    public string? Descripcion { get; set; }
}

public sealed class VehiculoDespacho
{
    public int Id { get; set; }
    public string Placa { get; set; } = "";
    public string? Descripcion { get; set; }
    public int? IdRutaDespacho { get; set; }
    public string? NombreRuta { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class ChoferDespacho
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Identificacion { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class ManifiestoDespacho
{
    public long Id { get; set; }
    public DateTime Fecha { get; set; }
    public int Estado { get; set; }
    public string? EstadoDescripcion { get; set; }
    public int? IdRutaDespacho { get; set; }
    public string? NombreRuta { get; set; }
    public int? IdVehiculo { get; set; }
    public string? Placa { get; set; }
    public int? IdChofer { get; set; }
    public string? NombreChofer { get; set; }
    public string? Observaciones { get; set; }
    public List<BultoDespacho> Bultos { get; set; } = new();
}
