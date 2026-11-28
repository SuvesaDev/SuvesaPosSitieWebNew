using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Cobranza;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IDesbloqueoCredito" />
public sealed class DesbloqueoCredito : ProxyBase, IDesbloqueoCredito
{
    private const string Base = "api/cobranza/desbloqueo-credito";
    private readonly HttpClient _api;

    public DesbloqueoCredito(IHttpClientFactory factory, IContextoSesion sesion, ILogger<DesbloqueoCredito> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<SolicitudDesbloqueoWebDTO>> Solicitar(SolicitarDesbloqueoWebComando comando)
        => Ejecutar(async () => await LecturaEnvelope.Leer<SolicitudDesbloqueoWebDTO>(
            await _api.PostAsJsonAsync($"{Base}/solicitar", comando, LecturaEnvelope.Json)), "solicitar la autorización de crédito");

    public Task<ResponseGeneric<SolicitudDesbloqueoWebDTO?>> UltimaDelCliente(long idCliente)
        => Ejecutar(async () => await LecturaEnvelope.Leer<SolicitudDesbloqueoWebDTO?>(
            await _api.GetAsync($"{Base}/cliente/{idCliente}")), "consultar la autorización de crédito");

    public Task<ResponseGeneric<List<SolicitudDesbloqueoWebDTO>>> Listar(bool soloPendientes)
        => Ejecutar(async () => await LecturaEnvelope.Leer<List<SolicitudDesbloqueoWebDTO>>(
            await _api.GetAsync($"{Base}?soloPendientes={(soloPendientes ? "true" : "false")}")), "consultar las solicitudes de crédito");

    public Task<ResponseGeneric<SolicitudDesbloqueoWebDTO>> Aprobar(long id, string? comentario)
        => Ejecutar(async () => await LecturaEnvelope.Leer<SolicitudDesbloqueoWebDTO>(
            await _api.PostAsJsonAsync($"{Base}/{id}/aprobar", new ResolverDesbloqueoWebComando { Comentario = comentario }, LecturaEnvelope.Json)), "aprobar la solicitud");

    public Task<ResponseGeneric<SolicitudDesbloqueoWebDTO>> Rechazar(long id, string comentario)
        => Ejecutar(async () => await LecturaEnvelope.Leer<SolicitudDesbloqueoWebDTO>(
            await _api.PostAsJsonAsync($"{Base}/{id}/rechazar", new ResolverDesbloqueoWebComando { Comentario = comentario }, LecturaEnvelope.Json)), "rechazar la solicitud");
}
