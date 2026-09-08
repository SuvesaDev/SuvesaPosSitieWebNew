using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IVentaOrquestada" />
public sealed class VentaOrquestada : ProxyBase, IVentaOrquestada
{
    private readonly HttpClient _api;

    public VentaOrquestada(IHttpClientFactory factory, IContextoSesion sesion, ILogger<VentaOrquestada> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<DevolucionInternaResultadoWebDTO>> DevolucionInterna(DevolucionInternaComandoWebDTO comando)
        => Ejecutar(async () => await LecturaEnvelope.Leer<DevolucionInternaResultadoWebDTO>(
            await _api.PostAsJsonAsync("api/venta-orquestada/devolucion-interna", comando, LecturaEnvelope.Json)),
            "registrar la devolución interna");
}
