using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consulta y mantenimiento basico de clientes.</summary>
public interface IClientesConsulta
{
    /// <summary>Busca por cedula o por nombre, segun lo que se escriba.</summary>
    Task<ResponseGeneric<ICollection<FiltranClienteDTO>>> Buscar(string texto);

    Task<ResponseGeneric<ClienteDTO>> Crear(ClienteDTO cliente);

    Task<ResponseGeneric<ClienteDTO>> Editar(ClienteDTO cliente);

    Task<ResponseGeneric<FiltranClienteDTO>> CambiarEstado(EliminarClienteDTO cliente, bool activar);

    Task<ResponseGeneric<ICollection<ClienteAdjuntoDTO>>> Adjuntos(long idCliente);

    Task<ResponseGeneric<ICollection<ClienteAdjuntoDTO>>> GuardarAdjuntos(ICollection<ClienteAdjuntoDTO> adjuntos);

    Task<ResponseGeneric<ClienteAdjuntoDTO>> EliminarAdjunto(long idAdjunto);

    Task<ResponseGeneric<ICollection<ClienteDatosSucursalDTO>>> DatosSucursal(long idCliente);
}
