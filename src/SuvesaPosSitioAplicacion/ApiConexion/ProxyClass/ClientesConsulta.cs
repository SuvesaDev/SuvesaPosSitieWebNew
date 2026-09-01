using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IClientesConsulta" />
public sealed class ClientesConsulta : ProxyBase, IClientesConsulta
{
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);
    private readonly IClienteApiCliente _api;

    public ClientesConsulta(
        IClienteApiCliente api,
        IContextoSesion sesion,
        ILogger<ClientesConsulta> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public async Task<ResponseGeneric<ICollection<FiltranClienteDTO>>> Buscar(string texto)
    {
        var limpio = texto?.Trim() ?? string.Empty;

        if (limpio.Length < 2)
        {
            return new ResponseGeneric<ICollection<FiltranClienteDTO>>(
                new List<FiltranClienteDTO>());
        }

        // Solo digitos se busca por cedula; si no, por nombre. Mismo criterio que en
        // la consulta de inventario: el usuario no tiene que elegir el modo.
        var porCedula = limpio.All(char.IsDigit);

        var peticion = new BuscarClienteDTO
        {
            Cedula = porCedula ? limpio : null,
            Nombre = porCedula ? null : limpio
        };

        return await Ejecutar(async () =>
        {
            var r = porCedula
                ? await _api.BuscarCedulaAsync(peticion)
                : await _api.BuscarNombreAsync(peticion);

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, $"buscar clientes con {limpio}");
    }

    public Task<ResponseGeneric<ICollection<FiltranClienteDTO>>> Listar()
        => Ejecutar(async () =>
        {
            // Sin termino: se pide por nombre con el filtro vacio; el API devuelve
            // el listado con su propio tope y la pantalla filtra en cliente.
            var r = await _api.BuscarNombreAsync(new BuscarClienteDTO { Nombre = string.Empty, Cedula = null });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "listar clientes");

    public Task<ResponseGeneric<ClienteDTO>> Crear(ClienteDTO cliente)
        => Ejecutar(async () =>
        {
            // NewRegistrar es el endpoint que utiliza la pantalla completa del
            // sistema actual; RegistrarBasico se reserva para altas desde venta.
            var r = await _api.NewRegistrarAsync(cliente);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el cliente");

    public Task<ResponseGeneric<ClienteDTO>> Editar(ClienteDTO cliente)
        => Ejecutar(async () =>
        {
            var r = await _api.NewActualizarAsync(cliente);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar el cliente");

    public Task<ResponseGeneric<FiltranClienteDTO>> CambiarEstado(
        EliminarClienteDTO cliente, bool activar)
        => Ejecutar(async () =>
        {
            var r = activar
                ? await _api.ActivarAsync(cliente)
                : await _api.DesactivarAsync(cliente);

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, activar ? "activar el cliente" : "desactivar el cliente");

    public Task<ResponseGeneric<ICollection<ClienteAdjuntoDTO>>> Adjuntos(long idCliente)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerAdjuntosClienteAsync(idCliente);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los adjuntos del cliente");

    public Task<ResponseGeneric<ICollection<ClienteAdjuntoDTO>>> GuardarAdjuntos(
        ICollection<ClienteAdjuntoDTO> adjuntos)
        => Ejecutar(async () =>
        {
            var r = await _api.InsertarAdjuntosClienteAsync(adjuntos);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar los adjuntos del cliente");

    public Task<ResponseGeneric<ClienteAdjuntoDTO>> EliminarAdjunto(long idAdjunto)
        => Ejecutar(async () =>
        {
            var r = await _api.EliminarAdjuntosClienteAsync(idAdjunto);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "eliminar el adjunto del cliente");

    public Task<ResponseGeneric<ICollection<ClienteDatosSucursalDTO>>> DatosSucursal(long idCliente)
        => Ejecutar<ICollection<ClienteDatosSucursalDTO>>(async () =>
        {
            var r = await _api.ObtenerDatosFacturacionClienteAsync(idCliente);
            if (r.Status != ResponseStatus._0)
            {
                return EnvelopeApi.A<ICollection<ClienteDatosSucursalDTO>>(
                    r.Status, r.CurrentException, r.ValidationErrors, null);
            }

            var json = JsonSerializer.Serialize(r.Responses, Opciones);
            var datos = JsonSerializer.Deserialize<List<ClienteDatosSucursalDTO>>(json, Opciones)
                        ?? new List<ClienteDatosSucursalDTO>();
            return EnvelopeApi.A<ICollection<ClienteDatosSucursalDTO>>(
                r.Status, r.CurrentException, r.ValidationErrors, datos);
        }, "consultar los datos de facturación del cliente");

    public Task<ResponseGeneric<CorreosComprobantes>> ObtenerCorreosComprobante(long idCliente)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerEmailsComprobantesAsync(idCliente);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los correos del comprobante");

    public Task<ResponseGeneric<CorreosComprobantes>> ActualizarCorreosComprobante(CorreosComprobantes correos)
        => Ejecutar(async () =>
        {
            var r = await _api.ActualizarEmailsComprobantesAsync(correos);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar los correos del comprobante");
}
