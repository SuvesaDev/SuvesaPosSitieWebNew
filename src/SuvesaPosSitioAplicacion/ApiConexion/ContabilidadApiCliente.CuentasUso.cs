using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Contabilidad;

namespace SuvesaPosSitioAplicacion.ApiConexion.Generated;

public partial interface IContabilidadApiCliente
{
    System.Threading.Tasks.Task<CuentasUsoContableDTOResponseGeneric> CuentasUsoAsync(int idEmisor, System.Threading.CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<CuentasUsoContableDTOResponseGeneric> GuardarCuentasUsoAsync(int idEmisor, CuentasUsoContableDTO cuerpo, System.Threading.CancellationToken cancellationToken = default);
}

public partial class ContabilidadApiCliente
{
    public virtual async System.Threading.Tasks.Task<CuentasUsoContableDTOResponseGeneric> CuentasUsoAsync(int idEmisor, System.Threading.CancellationToken cancellationToken = default)
        => await EnviarCuentasUsoAsync(HttpMethod.Get, idEmisor, null, cancellationToken);

    public virtual async System.Threading.Tasks.Task<CuentasUsoContableDTOResponseGeneric> GuardarCuentasUsoAsync(int idEmisor, CuentasUsoContableDTO cuerpo, System.Threading.CancellationToken cancellationToken = default)
        => await EnviarCuentasUsoAsync(HttpMethod.Put, idEmisor, cuerpo, cancellationToken);

    private async System.Threading.Tasks.Task<CuentasUsoContableDTOResponseGeneric> EnviarCuentasUsoAsync(HttpMethod metodo, int idEmisor, CuentasUsoContableDTO? cuerpo, System.Threading.CancellationToken cancellationToken)
    {
        var client_ = _httpClient;
        using var request_ = new HttpRequestMessage();
        if (cuerpo is not null)
            request_.Content = JsonContent.Create(cuerpo);
        request_.Method = metodo;
        request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
        var url_ = "api/contabilidad/emisores/" + Uri.EscapeDataString(ConvertToString(idEmisor, System.Globalization.CultureInfo.InvariantCulture)) + "/cuentas-uso";
        PrepareRequest(client_, request_, url_);
        request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);
        PrepareRequest(client_, request_, url_);
        var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        try
        {
            ProcessResponse(client_, response_);
            var texto = response_.Content == null ? "" : await response_.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var objeto = JsonSerializer.Deserialize<CuentasUsoContableDTOResponseGeneric>(texto, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
