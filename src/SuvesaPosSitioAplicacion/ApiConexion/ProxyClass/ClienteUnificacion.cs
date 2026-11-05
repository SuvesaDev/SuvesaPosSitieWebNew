using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Clientes;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IClienteUnificacion" />
public sealed class ClienteUnificacion : ProxyBase, IClienteUnificacion
{
    private const string Base = "api/clientes/unificacion";
    private readonly HttpClient _api;

    public ClienteUnificacion(IHttpClientFactory factory, IContextoSesion sesion, ILogger<ClienteUnificacion> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<VistaUnificacionWebDTO>> VistaPrevia(long idOrigen, long idDestino)
        => Ejecutar(async () => await LecturaEnvelope.Leer<VistaUnificacionWebDTO>(await _api.PostAsJsonAsync(
            $"{Base}/vista-previa", new UnificarClientesWebComando { IdOrigen = idOrigen, IdDestino = idDestino }, LecturaEnvelope.Json)),
            "consultar qué se va a unificar");

    public Task<ResponseGeneric<VistaUnificacionWebDTO>> Unificar(long idOrigen, long idDestino)
        => Ejecutar(async () => await LecturaEnvelope.Leer<VistaUnificacionWebDTO>(await _api.PostAsJsonAsync(
            $"{Base}/unificar", new UnificarClientesWebComando { IdOrigen = idOrigen, IdDestino = idDestino }, LecturaEnvelope.Json)),
            "unificar los clientes");

    public Task<ResponseGeneric<EstadoEliminacionClienteWebDTO>> PuedeEliminar(long idCliente)
        => Ejecutar(async () => await LecturaEnvelope.Leer<EstadoEliminacionClienteWebDTO>(
            await _api.GetAsync($"{Base}/{idCliente}/puede-eliminar")), "consultar si el cliente se puede eliminar");

    public Task<ResponseGeneric<EstadoEliminacionClienteWebDTO>> Eliminar(long idCliente)
        => Ejecutar(async () => await LecturaEnvelope.Leer<EstadoEliminacionClienteWebDTO>(
            await _api.PostAsJsonAsync($"{Base}/eliminar", new { idCliente }, LecturaEnvelope.Json)), "eliminar el cliente");
}
