using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class ClasificacionComercial : ProxyBase, IClasificacionComercial
{
    private readonly HttpClient _api;
    public ClasificacionComercial(IHttpClientFactory factory, IContextoSesion sesion, ILogger<ClasificacionComercial> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<PaginaClientesClasificacionDTO>> ListarClientes(FiltroClasificacionClientesDTO filtro)
        => Ejecutar(async () => await LecturaEnvelope.Leer<PaginaClientesClasificacionDTO>(
            await _api.GetAsync("api/comercial/clasificacion/clientes" + Consulta(
                ("Texto", filtro.Texto),
                ("SoloPendientes", filtro.SoloPendientes ? "true" : null),
                ("IdCanal", filtro.IdCanal?.ToString(CultureInfo.InvariantCulture)),
                ("Pagina", filtro.Pagina.ToString(CultureInfo.InvariantCulture)),
                ("Tamano", filtro.Tamano.ToString(CultureInfo.InvariantCulture))))), "consultar los clientes");

    public Task<ResponseGeneric<ResultadoAsignacionDTO>> AsignarCanal(AsignarCanalRequest request)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ResultadoAsignacionDTO>(
            await _api.PutAsJsonAsync("api/comercial/clasificacion/clientes/canal", request, LecturaEnvelope.Json)), "asignar el canal");

    public Task<ResponseGeneric<OpcionesFiltroArticulosDTO>> OpcionesFiltroArticulos()
        => Ejecutar(async () => await LecturaEnvelope.Leer<OpcionesFiltroArticulosDTO>(
            await _api.GetAsync("api/comercial/clasificacion/articulos/opciones-filtro")), "consultar los filtros");

    public Task<ResponseGeneric<PaginaArticulosClasificacionDTO>> ListarArticulos(FiltroClasificacionArticulosDTO filtro)
        => Ejecutar(async () => await LecturaEnvelope.Leer<PaginaArticulosClasificacionDTO>(
            await _api.GetAsync("api/comercial/clasificacion/articulos" + Consulta(
                ("Texto", filtro.Texto),
                ("CodMarca", filtro.CodMarca?.ToString(CultureInfo.InvariantCulture)),
                ("IdFamilia", filtro.IdFamilia?.ToString(CultureInfo.InvariantCulture)),
                ("IdCategoria", filtro.IdCategoria?.ToString(CultureInfo.InvariantCulture)),
                ("SinEspecie", filtro.SinEspecie ? "true" : null),
                ("SinFuncion", filtro.SinFuncion ? "true" : null),
                ("SinMargen", filtro.SinMargen ? "true" : null),
                ("Pagina", filtro.Pagina.ToString(CultureInfo.InvariantCulture)),
                ("Tamano", filtro.Tamano.ToString(CultureInfo.InvariantCulture))))), "consultar los artículos");

    public Task<ResponseGeneric<ArticuloClasificacionDTO>> GuardarArticulo(long idInventario, GuardarClasificacionArticuloRequest request)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ArticuloClasificacionDTO>(
            await _api.PutAsJsonAsync($"api/comercial/clasificacion/articulos/{idInventario}", request, LecturaEnvelope.Json)), "guardar la clasificación del artículo");

    public Task<ResponseGeneric<ResultadoAsignacionDTO>> AsignarMasiva(AsignarClasificacionMasivaRequest request)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ResultadoAsignacionDTO>(
            await _api.PostAsJsonAsync("api/comercial/clasificacion/articulos/asignar", request, LecturaEnvelope.Json)), "asignar la clasificación");

    /// <summary>Cadena de consulta con solo los parámetros presentes.</summary>
    private static string Consulta(params (string Nombre, string? Valor)[] parametros)
    {
        var sb = new StringBuilder();
        foreach (var (nombre, valor) in parametros)
        {
            if (string.IsNullOrWhiteSpace(valor)) continue;
            sb.Append(sb.Length == 0 ? '?' : '&').Append(nombre).Append('=').Append(Uri.EscapeDataString(valor.Trim()));
        }
        return sb.ToString();
    }
}
