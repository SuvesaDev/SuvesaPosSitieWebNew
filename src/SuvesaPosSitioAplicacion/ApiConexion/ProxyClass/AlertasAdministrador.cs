using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Correo;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IAlertasAdministrador" />
public sealed class AlertasAdministrador : ProxyBase, IAlertasAdministrador
{
    private readonly HttpClient _api;

    public AlertasAdministrador(IHttpClientFactory factory, IContextoSesion sesion, ILogger<AlertasAdministrador> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<PaginaAlertasAdministradorDTO>> Listar(bool soloNoLeidas, int? idEmisor, int pagina, int tamano)
    {
        var q = new List<string> { $"soloNoLeidas={soloNoLeidas.ToString().ToLowerInvariant()}", $"pagina={pagina}", $"tamano={tamano}" };
        if (idEmisor is { } e) q.Add($"idEmisor={e}");
        return Ejecutar(async () => await LecturaEnvelope.Leer<PaginaAlertasAdministradorDTO>(
            await _api.GetAsync($"api/alertas-administrador?{string.Join("&", q)}")), "consultar las alertas");
    }

    public Task<ResponseGeneric<int>> Conteo(int? idEmisor)
        => Ejecutar(async () => await LecturaEnvelope.Leer<int>(
            await _api.GetAsync(idEmisor is { } e ? $"api/alertas-administrador/conteo?idEmisor={e}" : "api/alertas-administrador/conteo")),
            "consultar el conteo de alertas");

    public Task<ResponseGeneric<bool>> MarcarLeida(long id)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PostAsync($"api/alertas-administrador/{id}/marcar-leida", null)), "marcar la alerta como leída");
}
