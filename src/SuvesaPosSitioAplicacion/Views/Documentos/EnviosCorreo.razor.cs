using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.DTOs.Correo;

namespace SuvesaPosSitioAplicacion.Views.Documentos;

public partial class EnviosCorreo
{
    private const string Titulo = "Envíos de correo";
    private const int Tamano = 40;

    private HxOffcanvas _detalle = default!;
    private PaginaEnviosCorreoDTO _pagina = new();
    private EnvioCorreoDTO? _sel;

    private string _estado = "";
    private int _idEmisor;
    private DateTime? _desde, _hasta;
    private string? _texto;
    private int _paginaNum = 1;

    private bool HayMas => _paginaNum * Tamano < _pagina.Total;

    protected override Task OnInitializedAsync() => Cargar();

    private async Task Buscar()
    {
        _paginaNum = 1;
        await Cargar();
    }

    private async Task Cargar()
    {
        var r = await Api.Listar(
            string.IsNullOrWhiteSpace(_estado) ? null : _estado,
            _idEmisor > 0 ? _idEmisor : null,
            _desde, _hasta?.AddDays(1).AddSeconds(-1),
            string.IsNullOrWhiteSpace(_texto) ? null : _texto,
            _paginaNum, Tamano);

        _pagina = await Respuestas.DatoAsync(r, "consultar los envíos de correo") ?? new();
    }

    private async Task Anterior() { if (_paginaNum > 1) { _paginaNum--; await Cargar(); } }
    private async Task Siguiente() { if (HayMas) { _paginaNum++; await Cargar(); } }

    private async Task VerDetalle(EnvioCorreoDTO e)
    {
        _sel = e;
        await _detalle.ShowAsync();
    }

    private async Task Reenviar(EnvioCorreoDTO e)
    {
        if (!await Dialogos.ConfirmarAsync($"¿Reencolar el envío del comprobante {e.Clave}?", "Reenviar")) return;
        if (await Respuestas.CorrectaAsync(await Api.Reenviar(e.Clave), "reencolar el envío"))
        {
            Dialogos.Exito("Envío reencolado.");
            await Cargar();
        }
    }

    private static bool Reenviable(string estado) => estado is "Fallido" or "OmitidoSinDestinatario" or "OmitidoRechazado";

    private static string Color(string estado) => estado switch
    {
        "Enviado" => "success",
        "Fallido" => "danger",
        "Enviando" => "info",
        "PendienteEnvio" => "warning",
        _ => "secondary",
    };

    private static string Humano(string estado) => estado switch
    {
        "PendienteEnvio" => "Pendiente",
        "Enviando" => "Enviando",
        "Enviado" => "Enviado",
        "Fallido" => "Fallido",
        "OmitidoSinDestinatario" => "Sin destinatario",
        "OmitidoRechazado" => "Rechazado",
        _ => estado,
    };
}
