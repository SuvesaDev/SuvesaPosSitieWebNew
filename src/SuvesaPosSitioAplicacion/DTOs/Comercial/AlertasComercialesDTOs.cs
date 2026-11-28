namespace SuvesaPosSitioAplicacion.DTOs.Comercial;

// Espejo a mano de api/comercial/alertas (DTOs/AlertasComercialesDTOs.cs del API).

public static class TipoAlertaComercial
{
    public const string ClienteEnRiesgo = "ClienteEnRiesgo";
    public const string ClienteInactivo = "ClienteInactivo";
    public const string DisminucionCompra = "DisminucionCompra";
    public const string RentabilidadBaja = "RentabilidadBaja";
    public const string FaltaInventario = "FaltaInventario";
    public const string VentaBajoMeta = "VentaBajoMeta";
    public const string CarteraVencida = "CarteraVencida";
    public const string FaltanteAbastecido = "FaltanteAbastecido";
    public const string VentaCruzada = "VentaCruzada";

    public static readonly (string Valor, string Texto)[] Todos =
    {
        (ClienteEnRiesgo, "Cliente en riesgo de perderse"),
        (ClienteInactivo, "Cliente inactivo"),
        (DisminucionCompra, "Disminución de compra"),
        (RentabilidadBaja, "Rentabilidad baja"),
        (FaltaInventario, "Falta de inventario"),
        (VentaBajoMeta, "Venta por debajo de la meta"),
        (CarteraVencida, "Cartera vencida"),
        (FaltanteAbastecido, "Producto faltante abastecido"),
        (VentaCruzada, "Oportunidad de venta cruzada"),
    };
}

/// <summary>Una alerta del módulo comercial. Tiene un solo estado, compartido por todos sus destinatarios.</summary>
public sealed class AlertaComercialDTO
{
    public long Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    /// <summary>Preventiva, Critica o Informativa.</summary>
    public string Severidad { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public long? IdCliente { get; set; }
    public int? IdRuta { get; set; }
    public string? NombreRuta { get; set; }
    /// <summary>Pendiente, Gestionada o Cerrada (ya no aplica).</summary>
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacionUtc { get; set; }
    public string? GestionadaPor { get; set; }
    public DateTime? FechaGestionUtc { get; set; }
    public string? NotaGestion { get; set; }
    public DateTime? FechaCierreUtc { get; set; }
    /// <summary>Solo en la supervisión: a quién le llega.</summary>
    public List<string> Destinatarios { get; set; } = new();
}

public sealed class FiltroAlertasComercialesDTO
{
    /// <summary>Pendiente (por defecto), Gestionada, Cerrada o Todas.</summary>
    public string? Estado { get; set; }
    public string? Tipo { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 25;
}

public sealed class PaginaAlertasComercialesDTO
{
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamano { get; set; }
    public List<AlertaComercialDTO> Items { get; set; } = new();
}

public sealed class GestionarAlertaComercialRequest
{
    public string? Nota { get; set; }
}

public sealed class ResumenAlertasComercialesDTO
{
    public int Pendientes { get; set; }
    public int PendientesCriticas { get; set; }
}

public sealed class ResultadoGeneracionAlertasDTO
{
    public int Creadas { get; set; }
    public int Actualizadas { get; set; }
    public int Cerradas { get; set; }
}
