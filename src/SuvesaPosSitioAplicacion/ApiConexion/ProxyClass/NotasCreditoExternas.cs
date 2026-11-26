using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="INotasCreditoExternas" />
public sealed class NotasCreditoExternas : ProxyBase, INotasCreditoExternas
{
    private readonly HttpClient _api;

    public NotasCreditoExternas(IHttpClientFactory factory, IContextoSesion sesion, ILogger<NotasCreditoExternas> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ValidacionNotaCreditoExternaWebDTO>> Validar(ValidarNotaCreditoExternaWebComando comando)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ValidacionNotaCreditoExternaWebDTO>(
            await _api.PostAsJsonAsync("api/notas-credito-externas/validar", comando, LecturaEnvelope.Json)), "validar los archivos del comprobante");

    public Task<ResponseGeneric<NotaCreditoExternaWebDTO>> Crear(CrearNotaCreditoExternaWebComando comando)
        => Ejecutar(async () => await LecturaEnvelope.Leer<NotaCreditoExternaWebDTO>(
            await _api.PostAsJsonAsync("api/notas-credito-externas", comando, LecturaEnvelope.Json)), "crear la nota de crédito externa");

    public Task<ResponseGeneric<NotaCreditoExternaWebDTO>> Obtener(long id)
        => Ejecutar(async () => await LecturaEnvelope.Leer<NotaCreditoExternaWebDTO>(
            await _api.GetAsync($"api/notas-credito-externas/{id}")), "consultar la nota de crédito externa");

    public Task<ResponseGeneric<PaginaNotasCreditoExternasWebDTO>> Listar(string? estado, string? texto, int pagina, int tamano)
        => Ejecutar(async () => await LecturaEnvelope.Leer<PaginaNotasCreditoExternasWebDTO>(await _api.GetAsync(
            "api/notas-credito-externas" + Cobranza.Consulta(new (string, object?)[] { ("estado", estado), ("texto", texto), ("pagina", pagina), ("tamano", tamano) }))),
            "consultar las notas de crédito externas");
}
