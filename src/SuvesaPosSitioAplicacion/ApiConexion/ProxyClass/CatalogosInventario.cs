using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICatalogosInventario" />
public sealed class CatalogosInventario : ProxyBase, ICatalogosInventario
{
    private readonly ISubFamiliasApiCliente _familias;
    private readonly IProveedorApiCliente _proveedores;
    private readonly IPresentacionApiCliente _presentaciones;
    private readonly IHaciendaApiCliente _hacienda;

    public CatalogosInventario(
        ISubFamiliasApiCliente familias,
        IProveedorApiCliente proveedores,
        IPresentacionApiCliente presentaciones,
        IHaciendaApiCliente hacienda,
        IContextoSesion sesion,
        ILogger<CatalogosInventario> log)
        : base(sesion, log)
    {
        _familias = familias;
        _proveedores = proveedores;
        _presentaciones = presentaciones;
        _hacienda = hacienda;
    }

    public Task<ResponseGeneric<ICollection<SubFamiliasFilterInventarioDTO>>> Familias()
        => Ejecutar(async () =>
        {
            var r = await _familias.ObtenerSubFamiliasInventarioAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las familias de inventario");

    public Task<ResponseGeneric<ICollection<ProveedoresFilterInventarioDTO>>> Proveedores()
        => Ejecutar(async () =>
        {
            var r = await _proveedores.ObtenerProveedoresInventarioAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los proveedores de inventario");

    public Task<ResponseGeneric<ICollection<Presentacione>>> Presentaciones()
        => Ejecutar(async () =>
        {
            var r = await _presentaciones.ObtenerPresentacionesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las presentaciones de inventario");

    public Task<ResponseGeneric<ICollection<CabysArticulos>>> BuscarCabys(string texto)
        => Ejecutar(async () =>
        {
            var r = await _hacienda.ObtenerCabysAsync(texto.Trim());
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar códigos CABYS");
}
