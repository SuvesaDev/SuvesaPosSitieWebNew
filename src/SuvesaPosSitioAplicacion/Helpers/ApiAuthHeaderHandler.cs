using System.Net.Http.Headers;
using SuvesaPosSitioAplicacion.ApiConexion;

namespace SuvesaPosSitioAplicacion.Helpers;

/// <summary>
/// Pone el token en cada llamada al API. El navegador nunca lo recibe: viaja del
/// servidor del sitio al API y punto.
///
/// Lee de <see cref="ContextoLlamada"/> y **no** de IContextoSesion. Motivo: los
/// handlers de IHttpClientFactory no se resuelven desde el ambito de la peticion,
/// asi que un servicio con scope inyectado aqui llega vacio. Esta comprobado: con
/// una sesion valida y token de 512 caracteres, el handler veia largo 0.
/// </summary>
public sealed class ApiAuthHeaderHandler : DelegatingHandler
{
    private readonly ILogger<ApiAuthHeaderHandler> _log;

    public ApiAuthHeaderHandler(ILogger<ApiAuthHeaderHandler> log)
    {
        _log = log;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = ContextoLlamada.Token;

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else if (!EsLogin(request))
        {
            // Salir sin token acaba en 401. El login es la unica llamada que
            // legitimamente va sin el.
            _log.LogWarning(
                "Llamada a {Ruta} sin token: el API respondera 401.",
                request.RequestUri?.AbsolutePath);
        }

        return base.SendAsync(request, cancellationToken);
    }

    private static bool EsLogin(HttpRequestMessage request)
        => request.RequestUri?.AbsolutePath.Contains("Login", StringComparison.OrdinalIgnoreCase) == true;
}
