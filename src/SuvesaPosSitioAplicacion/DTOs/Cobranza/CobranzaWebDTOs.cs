namespace SuvesaPosSitioAplicacion.DTOs.Cobranza;

// Espejo de los DTO de api/cobranza (ApiSuvesaPos/Class/Cobranza/CobranzaDtos.cs). A mano mientras los contratos
// NSwag no se regeneren contra el API que ya los trae. El API serializa los enums como número.

public enum EstadoSeguimientoWeb { Abierto = 1, EnPromesa = 2, Cerrado = 3 }

public enum MotivoCierreWeb { Pago = 1, NotaCredito = 2, Anulacion = 3 }

public enum TipoInteraccionWeb { Llamada = 1, Correo = 2, Whatsapp = 3, Visita = 4, NotaInterna = 5, Sistema = 6 }

public enum ResultadoInteraccionWeb
{
    SinResultado = 0, Contactado = 1, NoContesta = 2, NumeroErrado = 3, PrometioPago = 4, EnDisputa = 5,
    AbonoRegistrado = 20, FacturaSalio = 21, Reasignado = 22,
}

public enum EstadoPromesaWeb { Pendiente = 1, Cumplida = 2, Incumplida = 3, Cancelada = 4 }

public enum FiltroGestionWeb { Todas = 0, SinSeguimiento = 1, ConSeguimiento = 2 }

public sealed class FiltroFacturasVencidasWeb
{
    public int? DiasMoraMin { get; set; }
    public int? DiasMoraMax { get; set; }
    public long? IdCliente { get; set; }
    public string? Cobrador { get; set; }
    public FiltroGestionWeb Gestion { get; set; } = FiltroGestionWeb.Todas;
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 50;
}

public sealed class FacturaVencidaWebDTO
{
    public long IdVenta { get; set; }
    public double NumFactura { get; set; }
    public long IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime? Vence { get; set; }
    public int DiasMora { get; set; }
    public int CodMoneda { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal Saldo { get; set; }
    public long? IdSeguimiento { get; set; }
    public string? Cobrador { get; set; }
    public EstadoSeguimientoWeb? EstadoSeguimiento { get; set; }
    public bool TienePromesaIncumplida { get; set; }
}

public sealed class PaginaFacturasVencidasWeb
{
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamano { get; set; }
    public decimal SaldoTotal { get; set; }
    public List<FacturaVencidaWebDTO> Items { get; set; } = new();
}

public sealed class AbrirSeguimientoWebComando
{
    public List<long> IdsVenta { get; set; } = new();
    public string Observacion { get; set; } = string.Empty;
}

public sealed class AgregarFacturasWebComando
{
    public List<long> IdsVenta { get; set; } = new();
    public string Observacion { get; set; } = string.Empty;
}

public sealed class NuevaInteraccionWebComando
{
    public TipoInteraccionWeb Tipo { get; set; } = TipoInteraccionWeb.Llamada;
    public ResultadoInteraccionWeb Resultado { get; set; } = ResultadoInteraccionWeb.Contactado;
    public string Observacion { get; set; } = string.Empty;
    public long? IdCorrige { get; set; }
}

public sealed class NuevaPromesaWebComando
{
    public DateOnly FechaPrometida { get; set; }
    public decimal MontoAcordado { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observacion { get; set; }
}

public sealed class ReasignarWebComando
{
    public string NuevoCobrador { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}

public sealed class SeguimientoResumenWebDTO
{
    public long Id { get; set; }
    public long IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int CodMoneda { get; set; }
    public string Cobrador { get; set; } = string.Empty;
    public EstadoSeguimientoWeb Estado { get; set; }
    public MotivoCierreWeb? MotivoCierre { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaCierreUtc { get; set; }
    public int FacturasActivas { get; set; }
    public decimal SaldoActivo { get; set; }
}

public sealed class SeguimientoFacturaWebDTO
{
    public long IdVenta { get; set; }
    public double NumFactura { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime? Vence { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal SaldoAlAgregar { get; set; }
    public decimal SaldoVigente { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaAgregadaUtc { get; set; }
    public DateTime? FechaSalidaUtc { get; set; }
    public MotivoCierreWeb? MotivoSalida { get; set; }
}

public sealed class InteraccionWebDTO
{
    public long Id { get; set; }
    public TipoInteraccionWeb Tipo { get; set; }
    public ResultadoInteraccionWeb Resultado { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; }
    public long? IdVenta { get; set; }
    public decimal? Monto { get; set; }
    public long? IdCorrige { get; set; }
}

public sealed class PromesaPagoWebDTO
{
    public long Id { get; set; }
    public long IdSeguimiento { get; set; }
    public DateOnly FechaPrometida { get; set; }
    public decimal MontoAcordado { get; set; }
    public decimal MontoAbonado { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public EstadoPromesaWeb Estado { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaRegistroUtc { get; set; }
    public DateTime? FechaResolucionUtc { get; set; }
}

public sealed class ReasignacionWebDTO
{
    public string CobradorAnterior { get; set; } = string.Empty;
    public string CobradorNuevo { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public string UsuarioAdministrador { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; }
}

public sealed class SeguimientoDetalleWebDTO
{
    public SeguimientoResumenWebDTO Resumen { get; set; } = new();
    public string Version { get; set; } = string.Empty;
    public List<SeguimientoFacturaWebDTO> Facturas { get; set; } = new();
    /// <summary>Más reciente primero.</summary>
    public List<InteraccionWebDTO> Timeline { get; set; } = new();
    public List<PromesaPagoWebDTO> Promesas { get; set; } = new();
    public List<ReasignacionWebDTO> Reasignaciones { get; set; } = new();
}

public sealed class TareaCobradorWebDTO
{
    public string Tipo { get; set; } = string.Empty;
    public long IdSeguimiento { get; set; }
    public string? NombreCliente { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateOnly? Fecha { get; set; }
}

public sealed class DashboardCobradorWebDTO
{
    public string Cobrador { get; set; } = string.Empty;
    public DateTime GeneradoUtc { get; set; }
    public int SeguimientosActivos { get; set; }
    public decimal SaldoAsignado { get; set; }
    public List<TareaCobradorWebDTO> TareasDelDia { get; set; } = new();
    public List<PromesaPagoWebDTO> PromesasPendientes { get; set; } = new();
    public List<PromesaPagoWebDTO> PromesasIncumplidas { get; set; } = new();
}

public sealed class KpiCobradorWebDTO
{
    public string Cobrador { get; set; } = string.Empty;
    public int SeguimientosActivos { get; set; }
    public int SeguimientosCerradosPeriodo { get; set; }
    public int Interacciones { get; set; }
    public int PromesasCumplidas { get; set; }
    public int PromesasIncumplidas { get; set; }
    public decimal? EfectividadPromesas { get; set; }
    public decimal SaldoCartera { get; set; }
    public decimal? Dso { get; set; }
}

public sealed class DashboardGerencialWebDTO
{
    public DateOnly Desde { get; set; }
    public DateOnly Hasta { get; set; }
    public DateTime GeneradoUtc { get; set; }
    public int SeguimientosActivos { get; set; }
    public int InteraccionesTotales { get; set; }
    public decimal? EfectividadPromesasGlobal { get; set; }
    public List<KpiCobradorWebDTO> Cobradores { get; set; } = new();
}

/// <summary>Rotulos y reglas de presentación que comparten las pantallas de cobranza.</summary>
public static class PresentacionCobranza
{
    public static string Rotulo(TipoInteraccionWeb t) => t switch
    {
        TipoInteraccionWeb.Llamada => "Llamada",
        TipoInteraccionWeb.Correo => "Correo",
        TipoInteraccionWeb.Whatsapp => "WhatsApp",
        TipoInteraccionWeb.Visita => "Visita",
        TipoInteraccionWeb.NotaInterna => "Nota interna",
        _ => "Sistema",
    };

    public static string Icono(TipoInteraccionWeb t) => t switch
    {
        TipoInteraccionWeb.Llamada => "bi-telephone",
        TipoInteraccionWeb.Correo => "bi-envelope",
        TipoInteraccionWeb.Whatsapp => "bi-whatsapp",
        TipoInteraccionWeb.Visita => "bi-geo-alt",
        TipoInteraccionWeb.NotaInterna => "bi-journal-text",
        _ => "bi-gear",
    };

    public static string Rotulo(ResultadoInteraccionWeb r) => r switch
    {
        ResultadoInteraccionWeb.Contactado => "Contactado",
        ResultadoInteraccionWeb.NoContesta => "No contesta",
        ResultadoInteraccionWeb.NumeroErrado => "Número errado",
        ResultadoInteraccionWeb.PrometioPago => "Prometió pago",
        ResultadoInteraccionWeb.EnDisputa => "En disputa",
        ResultadoInteraccionWeb.AbonoRegistrado => "Abono registrado",
        ResultadoInteraccionWeb.FacturaSalio => "Factura salió",
        ResultadoInteraccionWeb.Reasignado => "Reasignado",
        _ => "Sin resultado",
    };

    /// <summary>Resultados que el cobrador puede digitar (los de sistema no).</summary>
    public static readonly ResultadoInteraccionWeb[] ResultadosDigitables =
    {
        ResultadoInteraccionWeb.Contactado, ResultadoInteraccionWeb.NoContesta, ResultadoInteraccionWeb.NumeroErrado,
        ResultadoInteraccionWeb.PrometioPago, ResultadoInteraccionWeb.EnDisputa, ResultadoInteraccionWeb.SinResultado,
    };

    public static readonly TipoInteraccionWeb[] TiposDigitables =
    {
        TipoInteraccionWeb.Llamada, TipoInteraccionWeb.Correo, TipoInteraccionWeb.Whatsapp,
        TipoInteraccionWeb.Visita, TipoInteraccionWeb.NotaInterna,
    };

    public static string Rotulo(EstadoSeguimientoWeb e) => e switch
    {
        EstadoSeguimientoWeb.Abierto => "Abierto",
        EstadoSeguimientoWeb.EnPromesa => "En promesa",
        _ => "Cerrado",
    };

    public static string Rotulo(MotivoCierreWeb? m) => m switch
    {
        MotivoCierreWeb.Pago => "Pago",
        MotivoCierreWeb.NotaCredito => "Nota de crédito",
        MotivoCierreWeb.Anulacion => "Anulación",
        _ => "—",
    };

    public static string Rotulo(EstadoPromesaWeb e) => e switch
    {
        EstadoPromesaWeb.Pendiente => "Pendiente",
        EstadoPromesaWeb.Cumplida => "Cumplida",
        EstadoPromesaWeb.Incumplida => "Incumplida",
        _ => "Cancelada",
    };

    /// <summary>Clase de Bootstrap del distintivo de la promesa; la incumplida va en rojo.</summary>
    public static string ClasePromesa(EstadoPromesaWeb e) => e switch
    {
        EstadoPromesaWeb.Cumplida => "text-bg-success",
        EstadoPromesaWeb.Incumplida => "text-bg-danger",
        EstadoPromesaWeb.Cancelada => "text-bg-secondary",
        _ => "text-bg-warning",
    };

    public static string ClaseEstado(EstadoSeguimientoWeb e) => e switch
    {
        EstadoSeguimientoWeb.Cerrado => "text-bg-secondary",
        EstadoSeguimientoWeb.EnPromesa => "text-bg-info",
        _ => "text-bg-primary",
    };

    /// <summary>Mínimo de caracteres que el API exige a la observación de una interacción.</summary>
    public const int MinimoObservacion = 10;

    public static bool ObservacionValida(string? texto) => (texto ?? string.Empty).Trim().Length >= MinimoObservacion;

    /// <summary>Las marcas del API están en UTC; la pantalla muestra la hora local del navegador del servidor.</summary>
    public static string Fecha(DateTime utc)
        => DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy HH:mm");
}
