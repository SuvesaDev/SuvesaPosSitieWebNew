using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Inventario;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IAuditoriaCabys" />
public sealed class AuditoriaCabys : ProxyBase, IAuditoriaCabys
{
    private readonly HttpClient _api;

    public AuditoriaCabys(IHttpClientFactory factory, IContextoSesion sesion, ILogger<AuditoriaCabys> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ResultadoAuditoriaCabysWeb>> Obtener(bool soloActivos)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ResultadoAuditoriaCabysWeb>(
            await _api.GetAsync($"api/inventario/auditoria-cabys?soloActivos={(soloActivos ? "true" : "false")}")), "revisar los CABYS de los artículos");
}
