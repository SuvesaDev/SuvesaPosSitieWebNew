using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Auditoria;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class Auditoria : ProxyBase, IAuditoria
{
    private readonly HttpClient _api;

    public Auditoria(IHttpClientFactory factory, IContextoSesion sesion, ILogger<Auditoria> log)
        : base(sesion, log)
    {
        _api = factory.CreateClient("SeePosApi");
    }

    public Task<ResponseGeneric<List<FilaAuditoriaWebDTO>>> LineaAsync(string entidad, string id)
        => Ejecutar(async () =>
        {
            var url = $"api/auditoria/linea?entidad={Uri.EscapeDataString(entidad)}&id={Uri.EscapeDataString(id)}";
            return await LecturaEnvelope.Leer<List<FilaAuditoriaWebDTO>>(await _api.GetAsync(url));
        }, "consultar la bitácora");
}
