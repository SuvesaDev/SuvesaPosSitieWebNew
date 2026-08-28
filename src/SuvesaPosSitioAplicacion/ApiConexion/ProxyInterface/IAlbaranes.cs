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
}
