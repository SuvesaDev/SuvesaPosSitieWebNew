using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.DTOs.Correo;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;

namespace SuvesaPosSitioAplicacion.Views.Parametros;

public partial class ConfiguracionCorreoFiscal
{
    private const string Titulo = "Configuración de correo";

    private HxModal _modalProbar = default!;
    private List<EmisorFiscalDTO> _emisores = new();
    private int _idEmisor;
    private ConfiguracionCorreoVistaDTO? _cfg;
    private string? _contrasena;
    private string _destinoPrueba = "";
    private bool _personalizar;
    private bool _guardando, _probando;

    protected override async Task OnInitializedAsync()
    {
        _emisores = (await Respuestas.DatoAsync(await EmisoresApi.Obtener(), "consultar los emisores"))?.ToList() ?? new();
    }

    private async Task CargarEmisor()
    {
        _cfg = null;
        _contrasena = null;
        if (_idEmisor <= 0) return;

        _cfg = await Respuestas.DatoAsync(await Api.Obtener(_idEmisor), "consultar la configuración de correo")
               ?? new ConfiguracionCorreoVistaDTO { IdEmisor = _idEmisor };
        _cfg.IdEmisor = _idEmisor;
    }

    private async Task Guardar()
    {
        if (_cfg is null) return;
        _guardando = true;
        try
        {
            var dto = new ConfiguracionCorreoGuardarDTO
            {
                IdEmisor = _idEmisor,
                SmtpHost = _cfg.SmtpHost,
                SmtpPuerto = _cfg.SmtpPuerto,
                UsaSsl = _cfg.UsaSsl,
                Usuario = _cfg.Usuario,
                Contrasena = string.IsNullOrWhiteSpace(_contrasena) ? null : _contrasena,
                RemitenteNombre = _cfg.RemitenteNombre,
                RemitenteCorreo = _cfg.RemitenteCorreo,
                CopiaOculta = _cfg.CopiaOculta,
                Habilitado = _cfg.Habilitado,
                AlertarPorCorreo = _cfg.AlertarPorCorreo,
                AsuntoPlantilla = _cfg.AsuntoPlantilla,
                CuerpoPlantilla = _cfg.CuerpoPlantilla,
            };

            if (await Respuestas.CorrectaAsync(await Api.Guardar(dto), "guardar la configuración de correo"))
            {
                Dialogos.Exito("Configuración de correo guardada.");
                _contrasena = null;
                await CargarEmisor();
            }
        }
        finally { _guardando = false; }
    }

    private async Task AbrirProbar()
    {
        _destinoPrueba = _cfg?.RemitenteCorreo ?? "";
        await _modalProbar.ShowAsync();
    }

    private async Task Probar()
    {
        _probando = true;
        try
        {
            var r = await Api.Probar(_idEmisor, _destinoPrueba);
            if (await Respuestas.CorrectaAsync(r, "enviar el correo de prueba"))
            {
                Dialogos.Exito($"Correo de prueba enviado a {_destinoPrueba}.");
                await _modalProbar.HideAsync();
            }
        }
        finally { _probando = false; }
    }
}
