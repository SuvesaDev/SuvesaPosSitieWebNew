using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Caja;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IConciliacionCaja" />
public sealed class ConciliacionCaja : ProxyBase, IConciliacionCaja
{
    private readonly HttpClient _api;

    public ConciliacionCaja(IHttpClientFactory factory, IContextoSesion sesion, ILogger<ConciliacionCaja> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ConciliacionCajaWebDTO>> Obtener(long napertura)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ConciliacionCajaWebDTO>(
            await _api.GetAsync($"api/caja/{napertura}/conciliacion")), "consultar la conciliación de caja");

    public Task<ResponseGeneric<CierreConciliadoWebDTO>> Cerrar(long napertura)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CierreConciliadoWebDTO>(
            await _api.PostAsync($"api/caja/{napertura}/cerrar", null)), "cerrar la caja");
}
