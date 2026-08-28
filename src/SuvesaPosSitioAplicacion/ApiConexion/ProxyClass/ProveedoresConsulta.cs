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

    public ProveedoresConsulta(
        IProveedorApiCliente api,
        IContextoSesion sesion,
        ILogger<ProveedoresConsulta> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<ProveedorDTO>>> Obtener()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerProveedoresAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los proveedores");
}
