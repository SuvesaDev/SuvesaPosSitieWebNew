using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consulta y mantenimiento basico de clientes.</summary>
public interface IClientesConsulta
{
    /// <summary>Busca por cedula o por nombre, segun lo que se escriba.</summary>
    Task<ResponseGeneric<ICollection<FiltranClienteDTO>>> Buscar(string texto);

    /// <summary>Listado de clientes sin termino (la pantalla lo carga al abrir y filtra en cliente).</summary>
    Task<ResponseGeneric<ICollection<FiltranClienteDTO>>> Listar();

    /// <summary>Consulta nombre, tipo de identificación y actividades de un contribuyente en Hacienda.</summary>
    Task<ResponseGeneric<BuscarClienteFacturacionDTO>> BuscarHacienda(string cedula);

    Task<ResponseGeneric<ClienteDTO>> Crear(ClienteDTO cliente);

    Task<ResponseGeneric<ClienteDTO>> Editar(ClienteDTO cliente);

    Task<ResponseGeneric<FiltranClienteDTO>> CambiarEstado(EliminarClienteDTO cliente, bool activar);

    Task<ResponseGeneric<ICollection<ClienteAdjuntoDTO>>> Adjuntos(long idCliente);

    Task<ResponseGeneric<ICollection<ClienteAdjuntoDTO>>> GuardarAdjuntos(ICollection<ClienteAdjuntoDTO> adjuntos);

    Task<ResponseGeneric<ClienteAdjuntoDTO>> EliminarAdjunto(long idAdjunto);

    Task<ResponseGeneric<ICollection<ClienteDatosSucursalDTO>>> DatosSucursal(long idCliente);

    /// <summary>Actividades económicas del cliente (Hacienda). Un cliente puede tener varias.</summary>
    Task<ResponseGeneric<ICollection<ActividadEconomicaClienteDTO>>> Actividades(long idCliente);

    /// <summary>Correos a los que se envía el comprobante electrónico de este cliente.</summary>
    Task<ResponseGeneric<CorreosComprobantes>> ObtenerCorreosComprobante(long idCliente);

    Task<ResponseGeneric<CorreosComprobantes>> ActualizarCorreosComprobante(CorreosComprobantes correos);
}
