using SuvesaPosSitioAplicacion.DTOs.Auditoria;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IAuditoria
{
    Task<ResponseGeneric<List<FilaAuditoriaWebDTO>>> LineaAsync(string entidad, string id);
}
