using Microsoft.AspNetCore.Components;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.Services;

/// <inheritdoc cref="IManejadorRespuestas" />
public sealed class ManejadorRespuestas : IManejadorRespuestas
{
    private readonly IServicioDialogos _dialogos;
    private readonly NavigationManager _navegacion;
    private readonly ILogger<ManejadorRespuestas> _log;

    public ManejadorRespuestas(
        IServicioDialogos dialogos,
        NavigationManager navegacion,
        ILogger<ManejadorRespuestas> log)
    {
        _dialogos = dialogos;
        _navegacion = navegacion;
        _log = log;
    }

    public async Task<T?> DatoAsync<T>(ResponseGeneric<T> respuesta, string? queSeIntentaba = null)
    {
        if (respuesta.EsCorrecta)
        {
            return respuesta.Responses;
        }

        await AvisarAsync(respuesta, queSeIntentaba);
        return default;
    }

    public async Task<bool> CorrectaAsync(Response respuesta, string? queSeIntentaba = null)
    {
        if (respuesta.EsCorrecta)
        {
            return true;
        }

        await AvisarAsync(respuesta, queSeIntentaba);
        return false;
    }

    private async Task AvisarAsync(Response respuesta, string? queSeIntentaba)
    {
        var detalle = respuesta.Excepcion ?? "El servidor no explico el motivo.";

        _log.LogWarning("Fallo del API al {Intento}: {Detalle}",
            queSeIntentaba ?? "consultar", detalle);

        // Sesion caducada: no tiene sentido ensenar el error tecnico, hay que reingresar.
        if (EsSesionCaducada(detalle))
        {
            await _dialogos.InformarAsync(
                "Su sesion expiro. Vuelva a ingresar para continuar.",
                "Sesion expirada");

            _navegacion.NavigateTo("/cuenta/salir", forceLoad: true);
            return;
        }

        if (respuesta.ErroresValidacion.Count > 0)
        {
            await _dialogos.ErrorAsync(
                string.Join("\n", respuesta.ErroresValidacion),
                queSeIntentaba is null ? "No se pudo completar" : $"No se pudo {queSeIntentaba}");
            return;
        }

        await _dialogos.ErrorAsync(
            detalle,
            queSeIntentaba is null ? "No se pudo completar" : $"No se pudo {queSeIntentaba}");
    }

    /// <summary>
    /// El cliente generado por NSwag mete el codigo HTTP en el texto de la excepcion,
    /// asi que se detecta ahi. Si algun dia el API devuelve un codigo de negocio para
    /// esto, conviene mirarlo en su lugar.
    /// </summary>
    private static bool EsSesionCaducada(string detalle)
        => detalle.Contains("(401)", StringComparison.Ordinal)
        || detalle.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase);
}
