using SuvesaPosSitioAplicacion.DTOs.Inventario;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Revisión (solo lectura) de artículos con CABYS incoherente con su marca de servicio/mercancía.</summary>
public interface IAuditoriaCabys
{
    Task<ResponseGeneric<ResultadoAuditoriaCabysWeb>> Obtener(bool soloActivos);
}
