using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.ApiConexion.Generated;

// Cliente escrito a mano hasta que se regeneren los contratos (./tools/actualizar-contratos.sh):
// al regenerar, ObtenerAperturasDeCajaActivasAsync aparece en SeePosApiClientes.cs y este archivo se borra.
public partial interface ICajaApiCliente
{
    /// <summary>Cajas abiertas (A) y arqueadas (M), las que la conciliación puede cerrar.</summary>
    System.Threading.Tasks.Task<AperturaCajaDTOListResponseGeneric> ObtenerAperturasDeCajaActivasAsync(System.Threading.CancellationToken cancellationToken = default);
}

public partial class CajaApiCliente
{
    public virtual async System.Threading.Tasks.Task<AperturaCajaDTOListResponseGeneric> ObtenerAperturasDeCajaActivasAsync(System.Threading.CancellationToken cancellationToken = default)
    {
        using var request_ = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "Caja/ObtenerAperturasDeCajaActivas")
        {
            Content = new System.Net.Http.StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json"),
        };
        request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

        var client_ = _httpClient;
        using var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var headers_ = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IEnumerable<string>>();
        foreach (var item_ in response_.Headers)
            headers_[item_.Key] = item_.Value;
        foreach (var item_ in response_.Content.Headers)
            headers_[item_.Key] = item_.Value;

        var status_ = (int)response_.StatusCode;
        if (status_ == 200)
        {
            var objectResponse_ = await ReadObjectResponseAsync<AperturaCajaDTOListResponseGeneric>(response_, headers_, cancellationToken).ConfigureAwait(false);
            return objectResponse_.Object ?? throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
        }
        var responseData_ = await response_.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
    }
}
