using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Impresion;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IPlantillasImpresion" />
public sealed class PlantillasImpresion : ProxyBase, IPlantillasImpresion
{
    private readonly HttpClient _api;

    public PlantillasImpresion(IHttpClientFactory factory, IContextoSesion sesion, ILogger<PlantillasImpresion> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ICollection<PlantillaImpresionResumenDTO>>> Listar(int? idEmisor, string? tipoSlug)
    {
        var q = new List<string>();
        if (idEmisor is { } e) q.Add($"idEmisor={e}");
        if (!string.IsNullOrWhiteSpace(tipoSlug)) q.Add($"tipo={Uri.EscapeDataString(tipoSlug)}");
        var url = "api/plantillas-impresion" + (q.Count > 0 ? "?" + string.Join("&", q) : "");
        return Ejecutar(async () => await LecturaEnvelope.Leer<ICollection<PlantillaImpresionResumenDTO>>(
            await _api.GetAsync(url)), "consultar las plantillas de impresión");
    }

    public Task<ResponseGeneric<PlantillaImpresionDTO>> Obtener(int id)
        => Ejecutar(async () => await LecturaEnvelope.Leer<PlantillaImpresionDTO>(
            await _api.GetAsync($"api/plantillas-impresion/{id}")), "consultar la plantilla");

    public Task<ResponseGeneric<int>> Crear(PlantillaImpresionDTO dto)
        => Ejecutar(async () => await LecturaEnvelope.Leer<int>(
            await _api.PostAsJsonAsync("api/plantillas-impresion", dto, LecturaEnvelope.Json)), "crear la plantilla");

    public Task<ResponseGeneric<int>> Actualizar(PlantillaImpresionDTO dto)
        => Ejecutar(async () => await LecturaEnvelope.Leer<int>(
            await _api.PutAsJsonAsync($"api/plantillas-impresion/{dto.Id}", dto, LecturaEnvelope.Json)), "actualizar la plantilla");

    public Task<ResponseGeneric<bool>> MarcarPredeterminada(int id)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PostAsync($"api/plantillas-impresion/{id}/predeterminada", null)), "marcar la plantilla como predeterminada");

    public Task<ResponseGeneric<bool>> Desactivar(int id)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.DeleteAsync($"api/plantillas-impresion/{id}")), "desactivar la plantilla");

    public Task<ResponseGeneric<CatalogoPlantillaImpresionDTO>> Catalogo(string tipoSlug)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CatalogoPlantillaImpresionDTO>(
            await _api.GetAsync($"api/plantillas-impresion/catalogo/{Uri.EscapeDataString(tipoSlug)}")),
            "consultar el catálogo de la plantilla");

    public Task<ResponseGeneric<byte[]>> Previsualizar(int id, string? configuracionJson, int? formato)
        => Ejecutar(async () =>
        {
            var respuesta = await _api.PostAsJsonAsync($"api/plantillas-impresion/{id}/previsualizar",
                new PrevisualizarPlantillaDTO { ConfiguracionJson = configuracionJson, Formato = formato },
                LecturaEnvelope.Json);
            return await LeerPdf(respuesta);
        }, "previsualizar la plantilla");

    internal static async Task<ResponseGeneric<byte[]>> LeerPdf(HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode)
            return new ResponseGeneric<byte[]>(await respuesta.Content.ReadAsByteArrayAsync());

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        return new ResponseGeneric<byte[]>($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}");
    }
}
