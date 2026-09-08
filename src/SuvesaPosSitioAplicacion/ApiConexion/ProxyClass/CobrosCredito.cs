using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICobrosCredito" />
public sealed class CobrosCredito : ProxyBase, ICobrosCredito
{
    private readonly HttpClient _api;

    public CobrosCredito(IHttpClientFactory factory, IContextoSesion sesion, ILogger<CobrosCredito> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<CreditoClienteWebDTO>> Credito(long idCliente)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CreditoClienteWebDTO>(
            await _api.GetAsync($"api/cobros-credito/clientes/{idCliente}/credito")), "consultar el crédito del cliente");

    public Task<ResponseGeneric<ICollection<FacturaCreditoWebDTO>>> Facturas(long idCliente)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ICollection<FacturaCreditoWebDTO>>(
            await _api.GetAsync($"api/cobros-credito/clientes/{idCliente}/facturas")), "consultar las facturas de crédito");

    public Task<ResponseGeneric<CobroCreditoResultadoWebDTO>> Cobrar(CobroCreditoComandoWebDTO comando)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CobroCreditoResultadoWebDTO>(
            await _api.PostAsJsonAsync("api/cobros-credito", comando, LecturaEnvelope.Json)), "registrar el cobro de crédito");

    public Task<ResponseGeneric<bool>> Anular(long idCobro, string? motivo)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PostAsJsonAsync($"api/cobros-credito/{idCobro}/anular", new { Motivo = motivo }, LecturaEnvelope.Json)),
            "anular el cobro");
}
