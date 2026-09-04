using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IPerfilesEmision" />
public sealed class PerfilesEmision : ProxyBase, IPerfilesEmision
{
    private readonly HttpClient _api;

    public PerfilesEmision(IHttpClientFactory factory, IContextoSesion sesion, ILogger<PerfilesEmision> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<PerfilEmisionElegibleWebDTO>>> Elegibles(
        int idEmisor, int idSucursal, int? numeroTerminal = null, string? modalidad = null)
        => Ejecutar(async () =>
        {
            var q = new List<string> { $"idEmisor={idEmisor}", $"idSucursal={idSucursal}" };
            if (numeroTerminal is { } t && t > 0) q.Add($"numeroTerminal={t}");
            if (!string.IsNullOrWhiteSpace(modalidad)) q.Add($"modalidad={modalidad}");
            return await LecturaEnvelope.Leer<IReadOnlyList<PerfilEmisionElegibleWebDTO>>(
                await _api.GetAsync("api/facturacion/perfiles-emision/elegibles?" + string.Join("&", q)));
        }, "consultar los perfiles de emisión");
}
