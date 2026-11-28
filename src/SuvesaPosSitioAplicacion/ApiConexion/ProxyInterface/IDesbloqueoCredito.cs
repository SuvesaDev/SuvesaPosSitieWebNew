using SuvesaPosSitioAplicacion.DTOs.Cobranza;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Autorización puntual para facturar a crédito a un cliente bloqueado por morosidad
/// (api/cobranza/desbloqueo-credito): quien factura la pide y un administrador la resuelve.
/// </summary>
public interface IDesbloqueoCredito
{
    Task<ResponseGeneric<SolicitudDesbloqueoWebDTO>> Solicitar(SolicitarDesbloqueoWebComando comando);

    /// <summary>La solicitud más reciente del cliente; sin datos si nunca se pidió.</summary>
    Task<ResponseGeneric<SolicitudDesbloqueoWebDTO?>> UltimaDelCliente(long idCliente);

    Task<ResponseGeneric<List<SolicitudDesbloqueoWebDTO>>> Listar(bool soloPendientes);

    Task<ResponseGeneric<SolicitudDesbloqueoWebDTO>> Aprobar(long id, string? comentario);

    Task<ResponseGeneric<SolicitudDesbloqueoWebDTO>> Rechazar(long id, string comentario);
}
