using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consulta y aplicación de consignaciones.</summary>
public interface IConsignaciones
{
    /// <summary>
    /// Encabezados por estado. El parametro se pasa tal cual a
    /// <c>venta/ObtenerConsignacionEstado</c>; el API no documenta el significado
    /// exacto del booleano y no se ha podido verificar (el endpoint hermano
    /// <c>Consignacion/ObtenerConsignacionEncabezadoEstado</c> responde 500 con
    /// cualquier valor, con datos o sin ellos, asi que no sirvio para contrastar).
    /// </summary>
    Task<ResponseGeneric<ICollection<FacturaDTO>>> PorEstado(bool valor);

    Task<ResponseGeneric<ICollection<ResultadoBusquedaConsignacionDTO>>> Buscar(string texto);
    Task<ResponseGeneric<FacturaDTO>> Obtener(long id);
    Task<ResponseGeneric<bool>> Aprobar(long id);
    Task<ResponseGeneric<FacturaDTO>> Despachar(ConsignacionAplicacionDTO aplicacion);
}
