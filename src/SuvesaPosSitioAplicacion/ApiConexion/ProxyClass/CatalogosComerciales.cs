using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class CatalogosComerciales : ProxyBase, ICatalogosComerciales
{
    private readonly HttpClient _api;
    public CatalogosComerciales(IHttpClientFactory factory, IContextoSesion sesion, ILogger<CatalogosComerciales> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<CatalogoComercialDTO>>> Listar(TipoCatalogoComercial tipo, bool incluirInactivos = false)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<CatalogoComercialDTO>>(
            await _api.GetAsync($"api/comercial/catalogos/{CatalogoComercialRutas.Segmento(tipo)}?incluirInactivos={(incluirInactivos ? "true" : "false")}")),
            "consultar el catálogo");

    public Task<ResponseGeneric<CatalogoComercialDTO>> Crear(TipoCatalogoComercial tipo, CatalogoComercialDTO valor)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CatalogoComercialDTO>(
            await _api.PostAsJsonAsync($"api/comercial/catalogos/{CatalogoComercialRutas.Segmento(tipo)}", valor, LecturaEnvelope.Json)),
            "crear el valor del catálogo");

    public Task<ResponseGeneric<CatalogoComercialDTO>> Actualizar(TipoCatalogoComercial tipo, int id, CatalogoComercialDTO valor)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CatalogoComercialDTO>(
            await _api.PutAsJsonAsync($"api/comercial/catalogos/{CatalogoComercialRutas.Segmento(tipo)}/{id}", valor, LecturaEnvelope.Json)),
            "actualizar el valor del catálogo");
}
