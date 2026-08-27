using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>
/// Base de todas las clases de ProxyClass. Resuelve en un solo sitio las dos cosas
/// que si no se repetirian en 51 proxies:
///
///   1. **Poner el token al alcance del handler.** Ver <see cref="ContextoLlamada"/>.
///   2. **Atrapar la excepcion.** Una View jamas debe ver una ApiException.
///
/// Cargar el contexto tambien ocurre aqui, asi que una pantalla ya no puede olvidarlo.
/// </summary>
public abstract class ProxyBase
{
    private readonly IContextoSesion _sesion;
    private readonly ILogger _log;

    protected ProxyBase(IContextoSesion sesion, ILogger log)
    {
        _sesion = sesion;
        _log = log;
    }

    /// <summary>Ejecuta una llamada al API con el token puesto y los fallos contenidos.</summary>
    protected async Task<ResponseGeneric<T>> Ejecutar<T>(
        Func<Task<ResponseGeneric<T>>> llamada,
        string queSeIntentaba)
    {
        try
        {
            // El proxy si vive en el ambito correcto, asi que aqui el contexto es el bueno.
            await _sesion.CargarAsync();
            ContextoLlamada.Token = _sesion.Token;

            return await llamada();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Fallo al {Intento}", queSeIntentaba);
            return new ResponseGeneric<T>(ex);
        }
        finally
        {
            ContextoLlamada.Token = null;
        }
    }
}
