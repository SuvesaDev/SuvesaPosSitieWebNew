using SuvesaPosSitioAplicacion.DTOs.Seguridad;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Catalogo de perfiles (tipo de cuenta) del rediseno de seguridad V2. La lectura
/// exige perfil que gestione seguridad; la escritura, Super Administración.
/// </summary>
public interface IPerfiles
{
    Task<ResponseGeneric<ICollection<PerfilSeguridadDTO>>> Listar();
    Task<ResponseGeneric<PerfilSeguridadDTO>> Crear(PerfilSeguridadDTO perfil);
    Task<ResponseGeneric<PerfilSeguridadDTO>> Editar(int idPerfil, PerfilSeguridadDTO perfil);
    Task<ResponseGeneric<bool>> Desactivar(int idPerfil);
}
