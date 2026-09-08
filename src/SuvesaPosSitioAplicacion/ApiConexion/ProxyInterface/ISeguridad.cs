using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Autenticacion y datos de la sesion.</summary>
public interface ISeguridad
{
    Task<ResponseGeneric<Autenticacion>> Login(string usuario, string password);

    Task<ResponseGeneric<ICollection<SucursalDTO>>> ObtenerSucursales();
}
