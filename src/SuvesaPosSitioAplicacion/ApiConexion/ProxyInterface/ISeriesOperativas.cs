using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Catálogo de series operativas (no fiscales) — SANEAMIENTO Fase 7,
/// <c>api/series-operativas</c>.
/// </summary>
public interface ISeriesOperativas
{
    Task<ResponseGeneric<IReadOnlyList<SerieOperativaWebDTO>>> Listar(int? tipo = null, int? idEmisor = null, int? idSucursal = null);

    Task<ResponseGeneric<int>> Guardar(SerieOperativaWebDTO dto);

    Task<ResponseGeneric<bool>> Activar(int id, bool activa);
}
