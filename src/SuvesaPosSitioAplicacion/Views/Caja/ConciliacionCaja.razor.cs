using System.Globalization;
using SuvesaPosSitioAplicacion.DTOs.Caja;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.Views.Caja;

public partial class ConciliacionCaja
{
    private const string Titulo = "Conciliación de caja";

    private List<AperturaCajaDTO> _aperturas = new();
    private long _napertura;
    private ConciliacionCajaWebDTO? _datos;
    private bool _cargando;

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
        "R" => "Arqueada",
        _ => e ?? "—",
    };
}
