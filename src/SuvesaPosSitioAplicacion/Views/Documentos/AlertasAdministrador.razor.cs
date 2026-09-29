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

    private static string Color(AlertaAdministradorDTO alerta) => alerta.Tipo switch
    {
        "ComprobanteRechazado" => "danger",
        "EnvioCorreoFallido" when EsSinDestinatario(alerta) => "warning",
        "EnvioCorreoFallido" => "warning",
        "ConfiguracionCorreoInvalida" => "secondary",
        "MensajeReceptorPendiente" => "warning",
        "LotePorVencer" or "RegistroSanitarioPorVencer" => "warning",
        "ExistenciaBajoMinimo" => "danger",
        _ => "secondary",
    };

    private static string Humano(AlertaAdministradorDTO alerta)
    {
        if (EsSinDestinatario(alerta)) return "Sin destinatario";
        return alerta.Tipo switch
        {
            "ComprobanteRechazado" => "Comprobante rechazado",
            "EnvioCorreoFallido" => "Envío fallido",
            "ConfiguracionCorreoInvalida" => "SMTP del emisor",
            "MensajeReceptorPendiente" => "Factura sin aceptar",
            "LotePorVencer" => "Lote por vencer",
            "RegistroSanitarioPorVencer" => "Registro sanitario",
            "ExistenciaBajoMinimo" => "Existencia bajo el mínimo",
            "InstrumentoPendienteConfirmacion" => "Instrumento pendiente",
            "InstrumentoEnRecuperacion" => "Instrumento en recuperación",
            _ => alerta.Tipo,
        };
    }

    private static bool EsSinDestinatario(AlertaAdministradorDTO alerta)
        => alerta.Tipo == "EnvioCorreoFallido"
           && (alerta.Titulo ?? string.Empty).Contains("sin correo", StringComparison.OrdinalIgnoreCase);

    /// <summary>La clave fiscal es numérica. Las claves internas (emisor, instrumento) no abren la bandeja.</summary>
    private static bool ClaveDeDocumento(string? clave)
        => !string.IsNullOrWhiteSpace(clave) && clave.Length >= 40 && clave.All(char.IsDigit);
}
