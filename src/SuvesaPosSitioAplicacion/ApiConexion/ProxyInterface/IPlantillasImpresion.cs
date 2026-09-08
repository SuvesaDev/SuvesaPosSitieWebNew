using SuvesaPosSitioAplicacion.DTOs.Impresion;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>CRUD y previsualización de plantillas de impresión (MOTOR_PLANTILLAS_IMPRESION_WEB.md §3).</summary>
public interface IPlantillasImpresion
{
    Task<ResponseGeneric<ICollection<PlantillaImpresionResumenDTO>>> Listar(int? idEmisor, string? tipoSlug);

    Task<ResponseGeneric<PlantillaImpresionDTO>> Obtener(int id);

    Task<ResponseGeneric<int>> Crear(PlantillaImpresionDTO dto);

    Task<ResponseGeneric<int>> Actualizar(PlantillaImpresionDTO dto);

    Task<ResponseGeneric<bool>> MarcarPredeterminada(int id);

    Task<ResponseGeneric<bool>> Desactivar(int id);

    Task<ResponseGeneric<CatalogoPlantillaImpresionDTO>> Catalogo(string tipoSlug);

    /// <summary>Devuelve el PDF de previsualización (bytes).</summary>
    Task<ResponseGeneric<byte[]>> Previsualizar(int id, string? configuracionJson, int? formato);
}
