using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Correo;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Views.Shared.Componentes;

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
    private bool _mostrarTablaLegada => false;

    private bool HayMas => _paginaNum * Tamano < _pagina.Total;

    protected override async Task OnInitializedAsync() => await Cargar();

    /// <summary>El cliente del comprobante, tal como lo resuelve el API. Antes se deducía del correo, y varios clientes pueden compartir uno (p. ej. un correo comodín).</summary>
    private static string NombreReceptor(EnvioCorreoDTO e)
        => !string.IsNullOrWhiteSpace(e.NombreReceptor) ? e.NombreReceptor! : string.IsNullOrWhiteSpace(e.Destinatarios) ? "Sin destinatario" : e.Destinatarios!;

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

    private AppModal _modalReenvio = default!;
    private EnvioCorreoDTO? _reenvio;
    private string? _correoNuevo;
    private bool _reenviando;

    /// <summary>Abre el modal de reenvío: al mismo correo o a uno nuevo solo para este envío (la ficha del cliente no cambia).</summary>
    private async Task Reenviar(EnvioCorreoDTO e)
    {
        _reenvio = e;
        _correoNuevo = e.Destinatarios;
        await _modalReenvio.MostrarAsync();
    }

    private bool CorreoCambiado
        => !string.IsNullOrWhiteSpace(_correoNuevo)
           && !string.Equals(_correoNuevo.Trim(), _reenvio?.Destinatarios?.Trim(), StringComparison.OrdinalIgnoreCase);

    private Task ReenviarAlMismo() => EjecutarReenvio(null);

    private Task ReenviarANuevo() => EjecutarReenvio(_correoNuevo);

    private async Task EjecutarReenvio(string? destinatarios)
    {
        if (_reenvio is null || _reenviando) return;
        _reenviando = true;
        try
        {
            if (await Respuestas.CorrectaAsync(await Api.Reenviar(_reenvio.Clave, destinatarios), "reencolar el envío"))
            {
                await _modalReenvio.OcultarAsync();
                Dialogos.Exito(destinatarios is null ? "Envío reencolado." : "Envío reencolado al correo indicado.");
                await Cargar();
            }
        }
        finally { _reenviando = false; }
    }

    private static bool Reenviable(string estado) => estado is "Enviado" or "Fallido" or "OmitidoSinDestinatario" or "OmitidoRechazado";

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
