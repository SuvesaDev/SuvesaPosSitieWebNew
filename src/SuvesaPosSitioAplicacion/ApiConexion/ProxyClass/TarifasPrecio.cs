using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Tarifas;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class TarifasPrecio : ProxyBase, ITarifasPrecio
{
    private readonly HttpClient _api;
    public TarifasPrecio(IHttpClientFactory factory, IContextoSesion sesion, ILogger<TarifasPrecio> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<TarifaPrecioDTO>>> Listar(bool incluirInactivas = false)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<TarifaPrecioDTO>>(
            await _api.GetAsync($"api/mantenimientos/tarifas?incluirInactivas={(incluirInactivas ? "true" : "false")}")), "consultar las tarifas");

    public Task<ResponseGeneric<TarifaPrecioDTO>> Crear(TarifaPrecioDTO tarifa)
        => Ejecutar(async () => await LecturaEnvelope.Leer<TarifaPrecioDTO>(
            await _api.PostAsJsonAsync("api/mantenimientos/tarifas", tarifa, LecturaEnvelope.Json)), "crear la tarifa");

    public Task<ResponseGeneric<TarifaPrecioDTO>> Actualizar(short idTarifa, TarifaPrecioDTO tarifa)
        => Ejecutar(async () => await LecturaEnvelope.Leer<TarifaPrecioDTO>(
            await _api.PutAsJsonAsync($"api/mantenimientos/tarifas/{idTarifa}", tarifa, LecturaEnvelope.Json)), "actualizar la tarifa");

    public Task<ResponseGeneric<IReadOnlyList<PrecioTarifaArticuloDTO>>> PreciosDeArticulo(long idArticulo)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<PrecioTarifaArticuloDTO>>(
            await _api.GetAsync($"api/mantenimientos/tarifas/articulos/{idArticulo}")), "consultar los precios por tarifa del artículo");

    public Task<ResponseGeneric<IReadOnlyList<PrecioTarifaArticuloDTO>>> GuardarPreciosDeArticulo(long idArticulo, IReadOnlyList<PrecioTarifaArticuloDTO> precios)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<PrecioTarifaArticuloDTO>>(
            await _api.PutAsJsonAsync($"api/mantenimientos/tarifas/articulos/{idArticulo}",
                new GuardarPreciosTarifaArticuloRequest { Precios = precios.ToList() }, LecturaEnvelope.Json)), "guardar los precios por tarifa del artículo");

    public Task<ResponseGeneric<IReadOnlyList<PrecioArticuloEnTarifaDTO>>> PreciosEnTarifa(short idTarifa, IEnumerable<long> idsArticulo)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<PrecioArticuloEnTarifaDTO>>(
            await _api.GetAsync($"api/mantenimientos/tarifas/{idTarifa}/precios?ids={string.Join(',', idsArticulo.Distinct())}")), "consultar el precio de la tarifa");
}
