using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Albaranes de Qvet. Consulta; facturarlos llega en olas posteriores.</summary>
public interface IAlbaranes
{
    Task<ResponseGeneric<ICollection<VentaDTO>>> PendientesDeFacturar();

    Task<ResponseGeneric<ICollection<VentaDTO>>> Todos();

    Task<ResponseGeneric<VentaDTO>> Uno(long id);

    /// <summary>Catalogo de estados que Qvet publica para los albaranes.</summary>
    Task<ResponseGeneric<ICollection<EstadoAlbaranesDTO>>> Estados();

    /// <summary>
    /// Peticiones (pruebas medicas) pendientes de facturar, ya filtradas por Qvet
    /// segun la sesion. Es lo que alimenta el panel de "Consulta Estados
    /// Albaranes": cada albaran trae una o mas lineas, una por prueba solicitada.
    /// </summary>
    Task<ResponseGeneric<ICollection<VentaDTO>>> PendientesDeFacturarFiltrado();

    /// <summary>Catalogo de pruebas medicas que Qvet publica, para filtrar peticiones.</summary>
    Task<ResponseGeneric<ICollection<PruebasMedicas>>> PruebasMedicas();
}
