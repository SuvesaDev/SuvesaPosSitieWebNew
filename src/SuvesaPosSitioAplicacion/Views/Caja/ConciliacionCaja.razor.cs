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
    private bool _cargando;
    private bool _cerrando;

    protected override async Task OnInitializedAsync()
    {
        _aperturas = (await Respuestas.DatoAsync(await CajaApi.AperturasSinCerrar(), "consultar las aperturas"))?.ToList() ?? new();
    }

    private async Task Cargar()
    {
        if (_napertura <= 0) { _datos = null; return; }
        _cargando = true;
        try
        {
            _datos = await Respuestas.DatoAsync(await Api.Obtener(_napertura), "consultar la conciliación de caja");
        }
        finally { _cargando = false; }
    }

    private async Task Cerrar()
    {
        if (_datos is null || _datos.Estado is not ("A" or "M")) return;
        if (!await Dialogos.ConfirmarPeligroAsync(
                $"Se cerrará la apertura N.º {_datos.NumApertura} con {Money(_datos.TotalEsperado)} como total de sistema " +
                "(desde el mayor de caja). Esta acción marca la caja como cerrada.",
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

            _aperturas = (await Respuestas.DatoAsync(await CajaApi.AperturasSinCerrar(), "consultar las aperturas"))?.ToList() ?? new();
            await Cargar();
        }
        finally { _cerrando = false; }
    }

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
