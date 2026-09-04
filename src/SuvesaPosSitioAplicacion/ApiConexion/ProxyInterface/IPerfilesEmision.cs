using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Perfiles de emisión elegibles para un ámbito (emisor + centro [+ terminal])
/// y una modalidad (contado/crédito). SANEAMIENTO Fase 8.3.
/// </summary>
public interface IPerfilesEmision
{
    Task<ResponseGeneric<IReadOnlyList<PerfilEmisionElegibleWebDTO>>> Elegibles(
        int idEmisor, int idSucursal, int? numeroTerminal = null, string? modalidad = null);
}
