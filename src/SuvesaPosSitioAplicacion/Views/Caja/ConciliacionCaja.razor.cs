using System.Globalization;
using SuvesaPosSitioAplicacion.DTOs.Caja;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Views.Shared.Componentes;

namespace SuvesaPosSitioAplicacion.Views.Caja;

public partial class ConciliacionCaja
{
    private const string Titulo = "Conciliación de caja";

    // Validación con clave interna antes de consultar/cerrar (igual que las demás
    // pantallas de caja): el cierre desde el mayor no debe quedar sin protección.
    private AppDesbloqueoClave? _bloqueo;
    private bool Bloqueada => _bloqueo is null || !_bloqueo.Desbloqueado;
    private Task AlDesbloquear(bool _) => InvokeAsync(StateHasChanged);

    private List<AperturaCajaDTO> _aperturas = new();
    private long _napertura;
    private ConciliacionCajaWebDTO? _datos;
    private ResumenInstrumentosCajaWebDTO? _instrumentos;
    private readonly List<OpcionesDePagoCierreCaja> _documentos = new();
    private bool _cargando;
    private bool _cerrando;

    protected override async Task OnInitializedAsync()
    {
        _aperturas = (await Respuestas.DatoAsync(await CajaApi.AperturasActivas(), "consultar las aperturas"))?.ToList() ?? new();
    }

    private async Task Cargar()
    {
        if (_napertura <= 0) { _datos = null; _instrumentos = null; _documentos.Clear(); return; }
        _cargando = true;
        try
        {
            _datos = await Respuestas.DatoAsync(await Api.Obtener(_napertura), "consultar la conciliación de caja");
            _instrumentos = await Respuestas.DatoAsync(await InstrumentosApi.ResumenCaja(_napertura), "consultar los instrumentos pendientes de caja");
            _documentos.Clear();
            var docs = await Respuestas.DatoAsync(await CajaApi.DocumentosDeApertura(_napertura), "consultar los documentos de la apertura");
            _documentos.AddRange(docs ?? Array.Empty<OpcionesDePagoCierreCaja>());
        }
        finally { _cargando = false; }
    }

    private async Task Cerrar()
    {
        if (_datos is null || _datos.Estado is not ("A" or "M")) return;
        if (!await Dialogos.ConfirmarPeligroAsync(
                $"Se cerrará la apertura N.º {_datos.NumApertura}. Contado por el cajero: {Money(_datos.TotalCajero ?? 0m)}; " +
                $"esperado por el sistema: {Money(_datos.TotalEsperado)}; diferencia: {Money(_datos.Diferencia ?? 0m)} " +
                $"({DescripcionDiferencia(_datos.Diferencia ?? 0m)}). Esta acción marca la caja como cerrada.",
                "Cerrar caja"))
            return;

        _cerrando = true;
        try
        {
            var r = await Api.Cerrar(_datos.NumApertura);
            var res = await Respuestas.DatoAsync(r, "cerrar la caja");
            if (res is null) return;

            Dialogos.Exito(res.FueReintento
                ? $"La apertura ya estaba cerrada (cierre #{res.IdCierre})."
                : $"Caja cerrada. Cierre #{res.IdCierre}, total de sistema {Money(res.TotalEsperado)}.");

            _aperturas = (await Respuestas.DatoAsync(await CajaApi.AperturasActivas(), "consultar las aperturas"))?.ToList() ?? new();
            await Cargar();
        }
        finally { _cerrando = false; }
    }

    /// <summary>Por qué no se puede cerrar todavía, o null si se puede: una caja abierta sin arqueo no tiene lo que contó el cajero.</summary>
    private string? MotivoNoSePuedeCerrar => _datos is null ? null
        : _datos.Estado is not ("A" or "M") ? "La apertura ya no se puede cerrar."
        : !_datos.TieneArqueo ? "Falta el arqueo: el cajero todavía no declaró lo que contó."
        : null;

    /// <summary>Qué hace el cierre con la diferencia (plantilla DiferenciaCaja): el faltante hasta el tope va a gasto, el que lo pasa se cobra al cajero y el sobrante va a ingreso.</summary>
    private static string DescripcionDiferencia(decimal dif) => Math.Abs(dif) <= 0.009m
        ? "cuadra, no genera asiento"
        : dif < 0 ? "faltante: a gasto si no pasa el tope de la empresa; si lo pasa, a cobrar al cajero"
        : "sobrante: a ingreso por sobrante de caja";

    private static string Money(decimal v) => v.ToString("N2", CultureInfo.GetCultureInfo("es-CR"));

    private static string MonedaTexto(int cod) => cod switch
    {
        1 => "CRC",
        2 => "USD",
        _ => $"#{cod}",
    };

    private static string EstadoTexto(string? e) => e switch
    {
        "A" => "Abierta",
        "C" => "Cerrada",
        "M" or "R" => "Arqueada",
        _ => e ?? "—",
    };
}
