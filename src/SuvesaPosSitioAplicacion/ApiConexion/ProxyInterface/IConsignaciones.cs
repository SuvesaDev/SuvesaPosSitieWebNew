using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Seguimiento de consignaciones. Solo lectura; registrar y facturar quedan
/// para cuando el arquetipo maestro-detalle se aplique a este documento.</summary>
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
}
