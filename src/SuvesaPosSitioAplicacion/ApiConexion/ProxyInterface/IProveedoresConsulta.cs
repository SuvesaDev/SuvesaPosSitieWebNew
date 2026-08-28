using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consulta de proveedores. Solo lectura; el mantenimiento llega en la Ola 4.</summary>
public interface IProveedoresConsulta
{
    Task<ResponseGeneric<ICollection<ProveedorDTO>>> Obtener();
}
