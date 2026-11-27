using System.Globalization;
using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class MetasVenta : ProxyBase, IMetasVenta
{
    private readonly HttpClient _api;
    public MetasVenta(IHttpClientFactory factory, IContextoSesion sesion, ILogger<MetasVenta> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<MetasMesDTO>> ListarMes(FiltroMetasDTO filtro)
    {
        var consulta = $"?Anio={filtro.Anio.ToString(CultureInfo.InvariantCulture)}&Mes={filtro.Mes.ToString(CultureInfo.InvariantCulture)}";
        if (!string.IsNullOrWhiteSpace(filtro.Nivel)) consulta += $"&Nivel={Uri.EscapeDataString(filtro.Nivel)}";
        if (filtro.IdRuta is int idRuta) consulta += $"&IdRuta={idRuta.ToString(CultureInfo.InvariantCulture)}";
        if (!string.IsNullOrWhiteSpace(filtro.IdUsuario)) consulta += $"&IdUsuario={Uri.EscapeDataString(filtro.IdUsuario)}";
        return Ejecutar(async () => await LecturaEnvelope.Leer<MetasMesDTO>(
            await _api.GetAsync("api/comercial/metas" + consulta)), "consultar las metas");
    }

    public Task<ResponseGeneric<IReadOnlyList<OpcionFiltroDTO>>> OpcionesDimension(string tipoDimension)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<OpcionFiltroDTO>>(
            await _api.GetAsync($"api/comercial/metas/dimensiones/{Uri.EscapeDataString(tipoDimension)}")), "consultar las opciones de la meta");

    public Task<ResponseGeneric<IReadOnlyList<MetaVentaCambioDTO>>> Historial(int anio, int mes)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<MetaVentaCambioDTO>>(
            await _api.GetAsync($"api/comercial/metas/historial?anio={anio}&mes={mes}")), "consultar el historial de metas");

    public Task<ResponseGeneric<MetaVentaDTO>> Crear(MetaVentaDTO meta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<MetaVentaDTO>(
            await _api.PostAsJsonAsync("api/comercial/metas", meta, LecturaEnvelope.Json)), "crear la meta");

    public Task<ResponseGeneric<MetaVentaDTO>> Actualizar(long id, MetaVentaDTO meta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<MetaVentaDTO>(
            await _api.PutAsJsonAsync($"api/comercial/metas/{id}", meta, LecturaEnvelope.Json)), "actualizar la meta");

    public Task<ResponseGeneric<bool>> Eliminar(long id, string? motivo = null)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.DeleteAsync($"api/comercial/metas/{id}" + (string.IsNullOrWhiteSpace(motivo) ? string.Empty : "?motivo=" + Uri.EscapeDataString(motivo.Trim())))), "eliminar la meta");
}
