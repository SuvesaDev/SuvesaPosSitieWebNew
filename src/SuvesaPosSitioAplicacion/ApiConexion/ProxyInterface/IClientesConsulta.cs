using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consulta de clientes. Solo lectura; el mantenimiento llega en la Ola 4.</summary>
public interface IClientesConsulta
{
    /// <summary>Busca por cedula o por nombre, segun lo que se escriba.</summary>
    Task<ResponseGeneric<ICollection<FiltranClienteDTO>>> Buscar(string texto);
}
