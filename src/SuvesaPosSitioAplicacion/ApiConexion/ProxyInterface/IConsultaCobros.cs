using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Consultas de sólo lectura de la pantalla "Cobrar" unificada
/// (SANEAMIENTO Fase 8.2): recibos emitidos y operaciones fallidas.
/// </summary>
public interface IConsultaCobros
{
    Task<ResponseGeneric<IReadOnlyList<ReciboCobroResumenWebDTO>>> Recibos(
        DateTime? desde = null, DateTime? hasta = null, long? idCliente = null,
        int? idSucursal = null, long? numApertura = null, int? estado = null,
        long? numeroRecibo = null, int limite = 100);

    Task<ResponseGeneric<IReadOnlyList<OperacionFallidaWebDTO>>> OperacionesFallidas(int limite = 100);
}
