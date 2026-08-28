using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
public interface IArticulosRelacionados { Task<ResponseGeneric<ICollection<ArticulosRelacionadosDTO>>> Buscar(long principal); Task<ResponseGeneric<bool>> Guardar(long principal, long relacionado, float cantidad, bool activo); }
