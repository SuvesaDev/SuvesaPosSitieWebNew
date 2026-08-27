using System.Net.Http.Headers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Helpers;

/// <summary>
/// Inyecta el token en cada llamada al API. El navegador nunca lo recibe: viaja
/// del servidor del sitio al API y punto.
///
/// Misma intencion que ApiAuthHeaderHandler de FCRCASitioAplicacion, pero leyendo
/// de <see cref="IContextoSesion"/> en lugar de ISession, por el motivo explicado ahi.
/// </summary>
public sealed class ApiAuthHeaderHandler : DelegatingHandler
{
    private readonly IContextoSesion _sesion;

    public ApiAuthHeaderHandler(IContextoSesion sesion)
    {
        _sesion = sesion;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_sesion.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sesion.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
