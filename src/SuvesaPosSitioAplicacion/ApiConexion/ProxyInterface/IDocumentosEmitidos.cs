using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consulta de facturas emitidas. Solo lectura.</summary>
public interface IDocumentosEmitidos
{
    Task<ResponseGeneric<ICollection<FacturaDTO>>> PorFechas(DateTime desde, DateTime hasta);

    Task<ResponseGeneric<ICollection<FacturaDTO>>> PorCliente(string codCliente);

    Task<ResponseGeneric<FacturaDTO>> PorNumero(string numero);
}
