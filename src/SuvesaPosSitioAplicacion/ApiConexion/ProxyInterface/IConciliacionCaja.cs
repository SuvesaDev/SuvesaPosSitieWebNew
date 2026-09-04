using SuvesaPosSitioAplicacion.DTOs.Caja;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Conciliación monetaria de caja desde el mayor de movimientos
/// (SANEAMIENTO Fase 3, <c>GET api/caja/{napertura}/conciliacion</c>).
/// </summary>
public interface IConciliacionCaja
{
    Task<ResponseGeneric<ConciliacionCajaWebDTO>> Obtener(long napertura);
}
