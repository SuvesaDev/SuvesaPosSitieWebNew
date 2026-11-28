namespace SuvesaPosSitioAplicacion.DTOs.Comercial;

// Espejo a mano de api/comercial/faltantes (DTOs/FaltantesDTOs.cs del API).

public static class EstadoSolicitudFaltante
{
    public const string Pendiente = "Pendiente";
    public const string EnOrdenCompra = "EnOrdenCompra";
    public const string Abastecido = "Abastecido";
    public const string VentaConcretada = "VentaConcretada";
    public const string Cancelada = "Cancelada";

    public static bool EstaAbierta(string estado) => estado is Pendiente or EnOrdenCompra or Abastecido;

    public static string Texto(string estado) => estado switch
    {
        Pendiente => "Pendiente de revisión",
        EnOrdenCompra => "En orden de compra",
        Abastecido => "Abastecido en bodega",
        VentaConcretada => "Venta concretada",
        Cancelada => "Cancelada",
        _ => estado,
    };

    public static string Clase(string estado) => estado switch
    {
        Pendiente => "text-bg-secondary",
        EnOrdenCompra => "text-bg-info",
        Abastecido => "text-bg-primary",
        VentaConcretada => "text-bg-success",
        Cancelada => "text-bg-dark",
        _ => "text-bg-light",
    };
}

public sealed class CrearSolicitudFaltanteRequest
{
    public long? IdInventario { get; set; }
    public string? Descripcion { get; set; }
    public decimal Cantidad { get; set; }
    public long? IdCliente { get; set; }
    public string? Origen { get; set; }
    public string? Nota { get; set; }
}

public sealed class SolicitudFaltanteDTO
{
    public long Id { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public string IdUsuarioSolicitante { get; set; } = string.Empty;
    public string? NombreSolicitante { get; set; }
    public long? IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int? IdRuta { get; set; }
    public string? NombreRuta { get; set; }
    public long? IdInventario { get; set; }
    public string? CodArticulo { get; set; }
    public string Producto { get; set; } = string.Empty;
    public bool EsCatalogado { get; set; }
    public string? Marca { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CantidadAbastecida { get; set; }
    public decimal CantidadPendiente { get; set; }
    public decimal? PrecioLista { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string? Nota { get; set; }
    public string? NumeroOrdenCompra { get; set; }
    public DateTime? FechaEstimadaArribo { get; set; }
    public DateTime? FechaAbastecidaUtc { get; set; }
    public long? IdVenta { get; set; }
    public DateTime? FechaCierreUtc { get; set; }
    public string? MotivoCancelacion { get; set; }
}

public sealed class GrupoAbastecimientoDTO
{
    public long? IdInventario { get; set; }
    public string? CodArticulo { get; set; }
    public string Producto { get; set; } = string.Empty;
    public bool EsCatalogado { get; set; }
    public string? Marca { get; set; }
    public List<string> Zonas { get; set; } = new();
    public decimal UnidadesDemandadas { get; set; }
    public decimal UnidadesPorAbastecer { get; set; }
    public int ClientesEsperando { get; set; }
    public List<string> Solicitantes { get; set; } = new();
    public List<string> OrdenesCompra { get; set; } = new();
    public DateTime? FechaEstimadaArribo { get; set; }
    public decimal ExistenciaActual { get; set; }
    public List<long> IdsSolicitud { get; set; } = new();
}

public sealed class VincularOrdenCompraRequest
{
    public List<long> IdsSolicitud { get; set; } = new();
    public string NumeroOrdenCompra { get; set; } = string.Empty;
    public DateTime? FechaEstimadaArribo { get; set; }
}

public sealed class ConcretarSolicitudRequest { public long IdVenta { get; set; } }

public sealed class CancelarSolicitudRequest { public string? Motivo { get; set; } }

public sealed class FiltroHistorialFaltantesDTO
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? IdRuta { get; set; }
    public int? CodMarca { get; set; }
    public string? IdUsuario { get; set; }
    public long? IdCliente { get; set; }
    public long? IdInventario { get; set; }
}

public sealed class FilaHistorialFaltanteDTO
{
    public long? IdInventario { get; set; }
    public string Producto { get; set; } = string.Empty;
    public bool EsCatalogado { get; set; }
    public string? Marca { get; set; }
    public int Quiebres { get; set; }
    public decimal UnidadesSolicitadas { get; set; }
    public decimal UnidadesNoSurtidas { get; set; }
    public decimal MontoVentaPerdida { get; set; }
}

public sealed class HistorialFaltantesDTO
{
    public int Solicitudes { get; set; }
    public int Quiebres { get; set; }
    public decimal UnidadesSolicitadas { get; set; }
    public decimal UnidadesNoSurtidas { get; set; }
    public decimal MontoVentaPerdida { get; set; }
    public int Concretadas { get; set; }
    public decimal TasaRecuperacionPct { get; set; }
    public List<FilaHistorialFaltanteDTO> PorProducto { get; set; } = new();
    public List<FilaHistorialFaltanteDTO> PorMarca { get; set; } = new();
    public List<FilaHistorialFaltanteDTO> NoCatalogados { get; set; } = new();
}
