using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ISucursales" />
public sealed class Sucursales : ProxyBase, ISucursales
{
    private readonly IIdentificacionApiCliente _identificacion;
    private readonly ICentrosApiCliente _centros;

    public Sucursales(
        IIdentificacionApiCliente identificacion,
        ICentrosApiCliente centros,
        IContextoSesion sesion,
        ILogger<Sucursales> log)
        : base(sesion, log)
    {
        _identificacion = identificacion;
        _centros = centros;
    }

    public Task<ResponseGeneric<ICollection<TipoIdentificacionDTO>>> TiposIdentificacion()
        => Ejecutar(async () =>
        {
            var r = await _identificacion.ObtenerAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los tipos de documento");

    public Task<ResponseGeneric<SucursalDTO>> Crear(SucursalDTO sucursal)
        => Ejecutar(async () =>
        {
            var r = await _centros.CrearSucursalAsync(sucursal);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la sucursal");
}
