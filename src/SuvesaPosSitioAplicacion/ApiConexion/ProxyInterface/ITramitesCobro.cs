using SuvesaPosSitioAplicacion.DTOs.Compras;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Boletas de trámite de cobro: constancia de entrega de facturas pendientes a
/// un cliente para su trámite interno de pago. Informativo — no cobra ni aplica.
/// </summary>
public interface ITramitesCobro
{
    Task<ResponseGeneric<IReadOnlyList<FacturaTramiteCobroWebDTO>>> Candidatas(long idCliente);

    Task<ResponseGeneric<TramiteCobroWebDTO>> Crear(CrearTramiteCobroWebDTO cmd);

    Task<ResponseGeneric<IReadOnlyList<TramiteCobroWebDTO>>> Listar(
        long? idCliente = null, bool incluirAnuladas = false,
        DateTime? desde = null, DateTime? hasta = null, long? consecutivo = null, int limite = 200);

    Task<ResponseGeneric<TramiteCobroWebDTO>> Obtener(long id);

    Task<ResponseGeneric<TramiteCobroWebDTO>> Anular(long id, string? motivo);

    Task<ResponseGeneric<ResultadoEnvioTramiteCobroWebDTO>> EnviarCorreo(long id, string? destino);
}
