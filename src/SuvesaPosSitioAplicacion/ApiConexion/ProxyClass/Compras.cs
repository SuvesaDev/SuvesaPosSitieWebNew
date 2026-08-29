using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICompras" />
public sealed class Compras : ProxyBase, ICompras
{
    private readonly IComprasApiCliente _compras;
    private readonly ICentrosApiCliente _centros;
    private readonly IMonedaApiCliente _monedas;
    private readonly IBodegaApiCliente _bodegas;
    private readonly IUsuarioApiCliente _usuarios;

    public Compras(
        IComprasApiCliente compras,
        ICentrosApiCliente centros,
        IMonedaApiCliente monedas,
        IBodegaApiCliente bodegas,
        IUsuarioApiCliente usuarios,
        IContextoSesion sesion,
        ILogger<Compras> log)
        : base(sesion, log)
    {
        _compras = compras;
        _centros = centros;
        _monedas = monedas;
        _bodegas = bodegas;
        _usuarios = usuarios;
    }

    public Task<ResponseGeneric<ICollection<EmpresaDTO>>> Empresas()
        => Ejecutar(async () =>
        {
            var r = await _centros.ObtenerEmpresasFacturacionAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las empresas de compras");

    public Task<ResponseGeneric<ICollection<Moneda>>> Monedas()
        => Ejecutar(async () =>
        {
            var r = await _monedas.ObtenerMonedasInventarioAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las monedas de compras");

    public Task<ResponseGeneric<ICollection<Bodega>>> Bodegas(bool costaPets)
        => Ejecutar(async () =>
        {
            var r = costaPets
                ? await _bodegas.ObtenerBodegasCostaPetsAsync()
                : await _bodegas.ObtenerBodegasAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las bodegas de compras");

    public Task<ResponseGeneric<Usuario>> ValidarClaveInterna(string contrasena)
        => Ejecutar(async () =>
        {
            var r = await _usuarios.ValidarClaveInternaSinUsuarioAsync(contrasena);
            return r.Status == ResponseStatus._0
                ? EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses)
                : new ResponseGeneric<Usuario>("Contraseña incorrecta.");
        }, "validar la clave interna de compras");

    public Task<ResponseGeneric<FacturaCompraDTO>> Crear(FacturaCompraDTO compra)
        => Ejecutar(async () =>
        {
            var r = await _compras.CrearFacturaAsync(compra);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "registrar la compra");

    public Task<ResponseGeneric<FacturaCompraDTO>> Editar(FacturaCompraDTO compra)
        => Ejecutar(async () =>
        {
            var r = await _compras.EditarCompraNuevoAsync(compra.IdCompra, compra);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar la compra");

    public Task<ResponseGeneric<FacturaCompraDTO>> Anular(FacturaCompraDTO compra)
        => Ejecutar(async () =>
        {
            var r = await _compras.EliminarFacturaAsync(compra.IdCompra, compra);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "anular la compra");

    public Task<ResponseGeneric<ICollection<FacturaCompraDTO>>> Buscar(FiltroFacturaCompras filtro)
        => Ejecutar(async () =>
        {
            var r = await _compras.ObtenerFacturasFiltroAsync(filtro);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar compras");

    public Task<ResponseGeneric<FacturaCompraDTO>> Obtener(long id)
        => Ejecutar(async () =>
        {
            var r = await _compras.ObtenerFacturaAsync(id);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la compra");

    public Task<ResponseGeneric<ICollection<CatalogoProductosInternosDTO>>> CatalogosInternos(ICollection<CatalogoProductosInternosDTO> productos)
        => Ejecutar(async () =>
        {
            var r = await _compras.ObtenerCatalogoProductosInternosAsync(productos);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las asociaciones de productos del proveedor");

    public Task<ResponseGeneric<CatalogoProductosInternosDTO>> VincularArticuloXml(CatalogoProductosInternosDTO producto)
        => Ejecutar(async () =>
        {
            var r = await _compras.ActualizarInventarioXMLAsync(producto);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar la asociación de producto del proveedor");

    public Task<ResponseGeneric<ICollection<ActualizarPreciosArticulosDTO>>> ActualizarPrecios(ICollection<ActualizarPreciosArticulosDTO> precios)
        => Ejecutar(async () =>
        {
            var r = await _compras.ActualizarPreciosArticulosAsync(precios);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "actualizar los precios de artículos importados");
}
