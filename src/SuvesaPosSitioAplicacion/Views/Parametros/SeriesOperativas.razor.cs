using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;

namespace SuvesaPosSitioAplicacion.Views.Parametros;

public partial class SeriesOperativas
{
    private const string Titulo = "Series operativas";

    private HxModal _modal = default!;
    private List<EmisorFiscalDTO> _emisores = new();
    private List<SucursalFiscalDTO> _sucursales = new();
    private List<SerieOperativaWebDTO> _series = new();

    private int _filtroTipo;
    private int _filtroEmisor;
    private bool _cargando, _guardando;

    private SerieOperativaWebDTO _edit = new();
    private int? _terminal;

    protected override async Task OnInitializedAsync()
    {
        _emisores = (await Respuestas.DatoAsync(await EmisoresApi.Obtener(), "consultar los emisores"))?.ToList() ?? new();
        _sucursales = (await Respuestas.DatoAsync(await SucursalesApi.Obtener(), "consultar los centros"))?.ToList() ?? new();
        await Cargar();
    }

    private async Task Cargar()
    {
        _cargando = true;
        try
        {
            var r = await Api.Listar(
                _filtroTipo > 0 ? _filtroTipo : null,
                _filtroEmisor > 0 ? _filtroEmisor : null);
            _series = (await Respuestas.DatoAsync(r, "consultar las series operativas"))?.ToList() ?? new();
        }
        finally { _cargando = false; }
    }

    private string NombreEmisor(int id) => _emisores.FirstOrDefault(e => e.Id == id)?.Nombre ?? $"Emisor {id}";

    private string NombreSucursal(int id) =>
        _sucursales.FirstOrDefault(s => s.Id == id) is { } s
            ? (s.NombreComercial ?? s.NombreFiscal ?? $"Centro {id}")
            : $"Centro {id}";

    private async Task Abrir(SerieOperativaWebDTO? s)
    {
        _edit = s is null
            ? new SerieOperativaWebDTO { Tipo = 1, Activa = true }
            : new SerieOperativaWebDTO
            {
                Id = s.Id,
                Tipo = s.Tipo,
                IdEmisor = s.IdEmisor,
                IdSucursal = s.IdSucursal,
                NumeroTerminal = s.NumeroTerminal,
                Prefijo = s.Prefijo,
                UltimoConsecutivo = s.UltimoConsecutivo,
                Activa = s.Activa,
                EsPredeterminada = s.EsPredeterminada,
            };
        _terminal = _edit.NumeroTerminal;
        await _modal.ShowAsync();
    }

    private async Task Guardar()
    {
        _edit.NumeroTerminal = _terminal is > 0 ? _terminal : null;

        if (_edit.IdEmisor <= 0 || _edit.IdSucursal <= 0)
        {
            Dialogos.Advertencia("El emisor y el centro son obligatorios (D1).");
            return;
        }

        _guardando = true;
        try
        {
            if (await Respuestas.DatoAsync(await Api.Guardar(_edit), "guardar la serie operativa") is > 0)
            {
                Dialogos.Exito("Serie operativa guardada.");
                await _modal.HideAsync();
                await Cargar();
            }
        }
        finally { _guardando = false; }
    }

    private async Task Alternar(SerieOperativaWebDTO s)
    {
        if (await Respuestas.CorrectaAsync(await Api.Activar(s.Id, !s.Activa), "cambiar el estado de la serie"))
        {
            await Cargar();
        }
    }
}
