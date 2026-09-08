using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consulta de depositos bancarios. Solo lectura.</summary>
public interface IDepositosConsulta
{
    Task<ResponseGeneric<ICollection<DepositosBuscarDTO>>> Buscar(
        string? numero, DateTime? desde, DateTime? hasta);
}
