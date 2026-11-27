using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Metas de ventas por mes. Un mes que ya inició está bloqueado salvo para quien tiene el permiso especial.</summary>
public interface IMetasVenta
{
    Task<ResponseGeneric<MetasMesDTO>> ListarMes(FiltroMetasDTO filtro);
    /// <summary>Categorías, familias, marcas, especies o funciones sobre las que se puede fijar una meta.</summary>
    Task<ResponseGeneric<IReadOnlyList<OpcionFiltroDTO>>> OpcionesDimension(string tipoDimension);
    Task<ResponseGeneric<IReadOnlyList<MetaVentaCambioDTO>>> Historial(int anio, int mes);
    Task<ResponseGeneric<MetaVentaDTO>> Crear(MetaVentaDTO meta);
    /// <summary>Cambia los valores; el mes, la ruta o persona y la dimensión no se cambian.</summary>
    Task<ResponseGeneric<MetaVentaDTO>> Actualizar(long id, MetaVentaDTO meta);
    Task<ResponseGeneric<bool>> Eliminar(long id, string? motivo = null);
}
