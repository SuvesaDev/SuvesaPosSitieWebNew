using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Familias de articulos. Catalogo simple.</summary>
public interface IFamilias
{
    Task<ResponseGeneric<ICollection<FamiliaDTO>>> Obtener();

    Task<ResponseGeneric<FamiliaDTO>> Crear(FamiliaDTO familia);

    Task<ResponseGeneric<FamiliaDTO>> Editar(FamiliaDTO familia);

    Task<ResponseGeneric<bool>> Eliminar(int codigo);

    Task<ResponseGeneric<ICollection<SubFamiliaDTO>>> ObtenerSubFamilias(int codigoFamilia);

    Task<ResponseGeneric<SubFamiliaDTO>> CrearSubFamilia(SubFamiliaDTO subFamilia);

    Task<ResponseGeneric<SubFamiliaDTO>> EditarSubFamilia(SubFamiliaDTO subFamilia);

    Task<ResponseGeneric<bool>> EliminarSubFamilia(int codigo);
}
