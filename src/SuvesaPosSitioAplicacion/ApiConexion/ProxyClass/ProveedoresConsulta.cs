using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IProveedoresConsulta" />
public sealed class ProveedoresConsulta : ProxyBase, IProveedoresConsulta
{
    private readonly IProveedorApiCliente _api;
    private readonly IClienteApiCliente _clientes;

    public ProveedoresConsulta(
        IProveedorApiCliente api,
        IClienteApiCliente clientes,
        IContextoSesion sesion,
        ILogger<ProveedoresConsulta> log)
        : base(sesion, log)
    {
        _api = api;
        _clientes = clientes;
    }

    public Task<ResponseGeneric<ICollection<ProveedorDTO>>> Obtener()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerProveedoresAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los proveedores");

    public Task<ResponseGeneric<ProveedorDTO>> Uno(int codigo)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerProveedorAsync(codigo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el proveedor");

    public Task<ResponseGeneric<ProveedorDTO>> Crear(ProveedorDTO proveedor)
        => Ejecutar(async () =>
        {
            var r = await _api.CreateProveedorAsync(proveedor);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el proveedor");

    public Task<ResponseGeneric<ProveedorDTO>> Editar(ProveedorDTO proveedor)
        => Ejecutar(async () =>
        {
            var r = await _api.EditarProveedoresNuevoAsync(proveedor.CodigoProv, proveedor);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar el proveedor");

    public Task<ResponseGeneric<BuscarClienteFacturacionDTO>> BuscarHacienda(string cedula)
        => Ejecutar(async () =>
        {
            // Solo dígitos: Hacienda rechaza guiones y el mensaje genérico del API
            // ("Error: Buscar cliente…") confundía al consultar un proveedor.
            var limpia = new string((cedula ?? string.Empty).Where(char.IsDigit).ToArray());
            var r = await _clientes.BuscarClienteHaciendaAsync(new BuscarClienteDTO
            {
                Cedula = limpia
            });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el proveedor en Hacienda");

    public Task<ResponseGeneric<bool>> CambiarEstado(int codigo, bool inhabilitar)
        => Ejecutar(async () =>
        {
            // El contrato usa estado=true para deshabilitar (misma semantica que
            // startActiveDisablesProveedores del sistema actual).
            var r = await _api.HabilitarInhabilitarProveedorAsync(codigo, inhabilitar);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, inhabilitar ? "deshabilitar el proveedor" : "activar el proveedor");

    public Task<ResponseGeneric<CuentaBancariaProveedorDTO>> EliminarCuenta(int idCuenta)
        => Ejecutar(async () =>
        {
            var r = await _api.EliminarCuentaProveedoreAsync(idCuenta);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "eliminar la cuenta bancaria del proveedor");
}
