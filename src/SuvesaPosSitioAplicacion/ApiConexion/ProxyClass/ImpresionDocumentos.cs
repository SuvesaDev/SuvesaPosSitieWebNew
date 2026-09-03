using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IImpresionDocumentos" />
public sealed class ImpresionDocumentos : ProxyBase, IImpresionDocumentos
{
    private readonly HttpClient _api;

    public ImpresionDocumentos(IHttpClientFactory factory, IContextoSesion sesion, ILogger<ImpresionDocumentos> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<byte[]>> Pdf(string tipoSlug, long id, string? formato, bool copia)
    {
        var q = new List<string> { $"copia={copia.ToString().ToLowerInvariant()}" };
        if (!string.IsNullOrWhiteSpace(formato)) q.Add($"formato={Uri.EscapeDataString(formato)}");
        var url = $"api/impresion/{Uri.EscapeDataString(tipoSlug)}/{id}/pdf?{string.Join("&", q)}";
        return Ejecutar(async () => await PlantillasImpresion.LeerPdf(await _api.GetAsync(url)), "generar el PDF del documento");
    }
}
