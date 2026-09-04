using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.JSInterop;
using SuvesaPosSitioAplicacion.DTOs.Cobros;

namespace SuvesaPosSitioAplicacion.Views.Ventas;

public partial class RecibosEmitidos
{
    private const string Titulo = "Recibos y operaciones fallidas";

    private HxModal _modalDetalle = default!;

    private string _tab = "recibos";
    private bool _cargando;

    // Filtro de recibos
    private DateTime? _desde = DateTime.Today.AddDays(-7);
    private DateTime? _hasta = DateTime.Today;
    private long _numeroRecibo;
    private long _numApertura;
    private int _estado;

    private List<ReciboCobroResumenWebDTO> _recibos = new();
    private List<OperacionFallidaWebDTO> _fallidas = new();
    private ReciboCobroResumenWebDTO? _detalle;

    protected override async Task OnInitializedAsync()
    {
        await CargarRecibos();
        await CargarFallidas();
    }

    private async Task Cambiar(string tab)
    {
        _tab = tab;
        if (tab == "recibos" && _recibos.Count == 0) await CargarRecibos();
        if (tab == "fallidas") await CargarFallidas();
    }

    private async Task CargarRecibos()
    {
        _cargando = true;
        try
        {
            var r = await Api.Recibos(
                desde: _desde,
                hasta: _hasta?.AddDays(1).AddSeconds(-1),
                numApertura: _numApertura > 0 ? _numApertura : null,
                estado: _estado > 0 ? _estado : null,
                numeroRecibo: _numeroRecibo > 0 ? _numeroRecibo : null,
                limite: 300);
            _recibos = (await Respuestas.DatoAsync(r, "consultar los recibos emitidos"))?.ToList() ?? new();
        }
        finally { _cargando = false; }
    }

    private async Task CargarFallidas()
    {
        _cargando = true;
        try
        {
            var r = await Api.OperacionesFallidas(200);
            _fallidas = (await Respuestas.DatoAsync(r, "consultar las operaciones fallidas"))?.ToList() ?? new();
        }
        finally { _cargando = false; }
    }

    private async Task VerDetalle(ReciboCobroResumenWebDTO r)
    {
        _detalle = r;
        await _modalDetalle.ShowAsync();
    }

    private async Task ImprimirRecibo(long idCobro)
        => await JS.InvokeVoidAsync("open", $"/documentos/recibo-cobro/{idCobro}/pdf", "_blank");
}
