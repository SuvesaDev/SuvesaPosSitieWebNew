using SuvesaPosSitioAplicacion.DTOs.Correo;

namespace SuvesaPosSitioAplicacion.Views.Documentos;

public partial class AlertasAdministrador
{
    private const string Titulo = "Alertas";

    private PaginaAlertasAdministradorDTO _pagina = new();
    private bool _soloNoLeidas = true;

    protected override Task OnInitializedAsync() => Cargar();

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
