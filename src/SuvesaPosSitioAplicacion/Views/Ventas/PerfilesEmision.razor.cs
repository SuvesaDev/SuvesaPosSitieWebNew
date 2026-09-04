using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;

namespace SuvesaPosSitioAplicacion.Views.Ventas;

public partial class PerfilesEmision
{
    private const string Titulo = "Perfiles de emisión";

    private List<EmisorFiscalDTO> _emisores = new();
    private List<SucursalFiscalDTO> _sucursales = new();

    private int _idEmisor;
    private int _idSucursal;
    private int _terminal;
    private string _modalidad = "";

    private List<PerfilEmisionElegibleWebDTO> _perfiles = new();
    private bool _cargando, _consultado;

    protected override async Task OnInitializedAsync()
    {
        _emisores = (await Respuestas.DatoAsync(await EmisoresApi.Obtener(), "consultar los emisores"))?.ToList() ?? new();
        _sucursales = (await Respuestas.DatoAsync(await SucursalesApi.Obtener(), "consultar los centros"))?.ToList() ?? new();
    }

    private async Task Consultar()
    {
        if (_idEmisor <= 0 || _idSucursal <= 0) return;
        _cargando = true;
        try
        {
            var r = await Api.Elegibles(
                _idEmisor, _idSucursal,
                _terminal > 0 ? _terminal : null,
                string.IsNullOrWhiteSpace(_modalidad) ? null : _modalidad);
            _perfiles = (await Respuestas.DatoAsync(r, "consultar los perfiles de emisión"))?.ToList() ?? new();
            _consultado = true;
        }
        finally { _cargando = false; }
    }
}
