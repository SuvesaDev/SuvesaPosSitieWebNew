using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Albaranes de Qvet. Consulta; facturarlos llega en olas posteriores.</summary>
public interface IAlbaranes
{
    Task<ResponseGeneric<ICollection<VentaDTO>>> PendientesDeFacturar();

    Task<ResponseGeneric<ICollection<VentaDTO>>> Todos();

    Task<ResponseGeneric<VentaDTO>> Uno(long id);
}
