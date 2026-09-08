using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Bandeja;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IBandejaDocumentos" />
public sealed class BandejaDocumentos : ProxyBase, IBandejaDocumentos
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public BandejaDocumentos(IHttpClientFactory factory, IContextoSesion sesion, ILogger<BandejaDocumentos> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoBandeja>>> Preventas(BandejaDocumentosFiltro filtro)
        => Ejecutar(async () => await Leer<BandejaDocumentosResultado<DocumentoBandeja>>(
            await _api.PostAsJsonAsync("BandejaDocumentos/Preventas", filtro, Json)), "consultar las preventas");

    public Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoFiscalBandeja>>> Facturas(BandejaDocumentosFiltro filtro)
        => Ejecutar(async () => await Leer<BandejaDocumentosResultado<DocumentoFiscalBandeja>>(
            await _api.PostAsJsonAsync("BandejaDocumentos/Facturas", filtro, Json)), "consultar las facturas");

    public Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoFiscalBandeja>>> NotasCredito(BandejaDocumentosFiltro filtro)
        => Ejecutar(async () => await Leer<BandejaDocumentosResultado<DocumentoFiscalBandeja>>(
            await _api.PostAsJsonAsync("BandejaDocumentos/NotasCredito", filtro, Json)), "consultar las notas de crédito");

    public Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoBandeja>>> Consignaciones(BandejaDocumentosFiltro filtro)
        => Ejecutar(async () => await Leer<BandejaDocumentosResultado<DocumentoBandeja>>(
            await _api.PostAsJsonAsync("BandejaDocumentos/Consignaciones", filtro, Json)), "consultar las consignaciones");

    public Task<ResponseGeneric<FacturaBandejaDetalle>> DetalleFactura(long id)
        => Ejecutar(async () => await Leer<FacturaBandejaDetalle>(
            await _api.GetAsync($"BandejaDocumentos/DetalleFactura/{id}")), "consultar el detalle de la factura");

    public Task<ResponseGeneric<NotaCreditoBandejaDetalle>> DetalleNotaCredito(long id)
        => Ejecutar(async () => await Leer<NotaCreditoBandejaDetalle>(
            await _api.GetAsync($"BandejaDocumentos/DetalleNotaCredito/{id}")), "consultar el detalle de la nota de crédito");

    private static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage respuesta)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
            return new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}");

        var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json)
                       ?? throw new InvalidOperationException("Respuesta vacía.");
        return envelope.Status == 0
            ? new(envelope.Responses)
            : new(envelope.CurrentException ?? "Error sin detalle.", envelope.ValidationErrors ?? Array.Empty<string>());
    }

    private sealed class Envelope<T>
    {
        public int Status { get; init; }
        public string? CurrentException { get; init; }
        public IReadOnlyList<string>? ValidationErrors { get; init; }
        public T? Responses { get; init; }
    }
}
