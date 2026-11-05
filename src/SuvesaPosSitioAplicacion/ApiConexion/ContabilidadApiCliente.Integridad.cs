using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Contabilidad;

namespace SuvesaPosSitioAplicacion.ApiConexion.Generated;

public partial interface IContabilidadApiCliente
{
    System.Threading.Tasks.Task<RespuestaIntegridad<DocumentosSinEventoDTO>> DocumentosSinEventoAsync(long idLibroContable, DateOnly? desde, DateOnly? hasta, System.Threading.CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<RespuestaIntegridad<RegularizacionDocumentosDTO>> RegularizarDocumentosAsync(RangoIntegridadContableDTO cuerpo, System.Threading.CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<RespuestaIntegridad<DiagnosticoPlantillasDTO>> DiagnosticarPlantillasAsync(long idLibroContable, System.Threading.CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<RespuestaIntegridad<ResultadoReencoladoDTO>> ReintentarErroresConfiguracionAsync(ReencolarErroresDTO cuerpo, System.Threading.CancellationToken cancellationToken = default);
}

public partial class ContabilidadApiCliente
{
    public virtual System.Threading.Tasks.Task<RespuestaIntegridad<DocumentosSinEventoDTO>> DocumentosSinEventoAsync(long idLibroContable, DateOnly? desde, DateOnly? hasta, System.Threading.CancellationToken cancellationToken = default)
    {
        var consulta = "?idLibroContable=" + idLibroContable
            + (desde is DateOnly d ? "&desde=" + d.ToString("yyyy-MM-dd") : "")
            + (hasta is DateOnly h ? "&hasta=" + h.ToString("yyyy-MM-dd") : "");
        return EnviarIntegridadAsync<DocumentosSinEventoDTO>(HttpMethod.Get, "api/contabilidad/integridad/documentos-sin-evento" + consulta, null, cancellationToken);
    }

    public virtual System.Threading.Tasks.Task<RespuestaIntegridad<RegularizacionDocumentosDTO>> RegularizarDocumentosAsync(RangoIntegridadContableDTO cuerpo, System.Threading.CancellationToken cancellationToken = default)
        => EnviarIntegridadAsync<RegularizacionDocumentosDTO>(HttpMethod.Post, "api/contabilidad/integridad/regularizar-documentos", cuerpo, cancellationToken);

    public virtual System.Threading.Tasks.Task<RespuestaIntegridad<DiagnosticoPlantillasDTO>> DiagnosticarPlantillasAsync(long idLibroContable, System.Threading.CancellationToken cancellationToken = default)
        => EnviarIntegridadAsync<DiagnosticoPlantillasDTO>(HttpMethod.Get, "api/contabilidad/integridad/plantillas?idLibroContable=" + idLibroContable, null, cancellationToken);

    public virtual System.Threading.Tasks.Task<RespuestaIntegridad<ResultadoReencoladoDTO>> ReintentarErroresConfiguracionAsync(ReencolarErroresDTO cuerpo, System.Threading.CancellationToken cancellationToken = default)
        => EnviarIntegridadAsync<ResultadoReencoladoDTO>(HttpMethod.Post, "api/contabilidad/eventos/reintentar-configuracion", cuerpo, cancellationToken);

    private async System.Threading.Tasks.Task<RespuestaIntegridad<T>> EnviarIntegridadAsync<T>(HttpMethod metodo, string url_, object? cuerpo, System.Threading.CancellationToken cancellationToken) where T : class
    {
        var client_ = _httpClient;
        using var request_ = new HttpRequestMessage();
        if (cuerpo is not null)
            request_.Content = JsonContent.Create(cuerpo, cuerpo.GetType());
        request_.Method = metodo;
        request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
        PrepareRequest(client_, request_, url_);
        request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);
        PrepareRequest(client_, request_, url_);
        var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        try
        {
            ProcessResponse(client_, response_);
            var texto = response_.Content == null ? "" : await response_.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var objeto = JsonSerializer.Deserialize<RespuestaIntegridad<T>>(texto, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (objeto == null)
                throw new ApiException("Response was null which was not expected.", (int)response_.StatusCode, texto, new Dictionary<string, IEnumerable<string>>(), null);
            return objeto;
        }
        finally
        {
            response_.Dispose();
        }
    }
}
