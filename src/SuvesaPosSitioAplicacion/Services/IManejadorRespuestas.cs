using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Que hacer cuando el API responde mal. Punto unico.
///
/// Sin esto, cada pantalla decide por su cuenta si avisa, como avisa, y que hace
/// con una sesion caducada. Con 78 pantallas por delante eso son 78 criterios.
/// </summary>
public interface IManejadorRespuestas
{
    /// <summary>
    /// Si la respuesta vino bien, entrega el dato. Si no, avisa al usuario y
    /// devuelve el valor por defecto. La pantalla solo comprueba si hay dato.
    /// </summary>
    Task<T?> DatoAsync<T>(ResponseGeneric<T> respuesta, string? queSeIntentaba = null);

    /// <summary>Igual, para operaciones sin dato de vuelta. Devuelve si salio bien.</summary>
    Task<bool> CorrectaAsync(Response respuesta, string? queSeIntentaba = null);
}
