using SuvesaPosSitioAplicacion.DTOs.Correo;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using System.Collections.Generic;

namespace SuvesaPosSitioAplicacion.Views.Documentos;

public partial class AlertasAdministrador
{
    private const string Titulo = "Alertas";

    private PaginaAlertasAdministradorDTO _pagina = new();
    private IReadOnlyDictionary<int, string> _nombresEmisores = new Dictionary<int, string>();
    private bool _soloNoLeidas = true;
    private bool _mostrarTablaLegada => false;

    protected override async Task OnInitializedAsync()
    {
        var emisores = await Respuestas.DatoAsync(await EmisoresApi.Obtener(), "consultar los emisores");
        _nombresEmisores = emisores?.Where(e => e.Id > 0)
            .ToDictionary(e => e.Id, e => string.IsNullOrWhiteSpace(e.Nombre) ? $"Emisor {e.Id}" : e.Nombre.Trim())
            ?? new Dictionary<int, string>();
        await Cargar();
    }

    private string NombreEmisor(int? id) => id is null or <= 0
        ? "—"
        : _nombresEmisores.TryGetValue(id.Value, out var nombre) ? nombre : $"Emisor {id.Value}";

    private async Task Cargar()
    {
        var r = await Api.Listar(_soloNoLeidas, null, 1, 100);
        _pagina = await Respuestas.DatoAsync(r, "consultar las alertas") ?? new();
    }

    private async Task MarcarLeida(AlertaAdministradorDTO a)
    {
        if (await Respuestas.CorrectaAsync(await Api.MarcarLeida(a.Id), "marcar la alerta como leída"))
        {
            await Cargar();
            await Avisador.NotificarAsync();   // refresca el contador de la campana al instante
        }
    }

    private async Task MarcarTodas()
    {
        if (!await Dialogos.ConfirmarAsync("¿Marcar como leídas todas las alertas no leídas?", "Alertas")) return;
        if (await Respuestas.CorrectaAsync(await Api.MarcarTodasLeidas(), "marcar todas las alertas como leídas"))
        {
            Dialogos.Exito("Alertas actualizadas.");
            await Cargar();
            await Avisador.NotificarAsync();
        }
    }

    private static string Color(string tipo) => tipo switch
    {
        "ComprobanteRechazado" => "danger",
        "EnvioCorreoFallido" => "warning",
        "ConfiguracionCorreoInvalida" => "secondary",
        _ => "secondary",
    };

    private static string Humano(string tipo) => tipo switch
    {
        "ComprobanteRechazado" => "Comprobante rechazado",
        "EnvioCorreoFallido" => "Envío fallido",
        "ConfiguracionCorreoInvalida" => "Configuración inválida",
        _ => tipo,
    };
}
