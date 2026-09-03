using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <summary>
/// Mantenimiento de bodegas. <b>Solo lectura</b>: el API real solo expone
/// <c>bodega/ObtenerBodegas</c> (el mismo endpoint que consume la SPA React).
/// No hay endpoints de alta/edición/baja de bodegas, ni ningún endpoint de "áreas"
/// —esa pantalla sigue siendo el mockup del sistema anterior—. La versión previa de
/// esta clase llamaba a rutas REST (<c>api/mantenimientos/*</c>) que no existen en el
/// API y devolvían 404, lo que reventaba con "The input does not contain any JSON tokens".
/// </summary>
public sealed class MantenimientosInventario : ProxyBase, IMantenimientosInventario
{
    private readonly IBodegaApiCliente _bodegas;

    public MantenimientosInventario(IBodegaApiCliente bodegas, IContextoSesion sesion, ILogger<MantenimientosInventario> log) : base(sesion, log)
        => _bodegas = bodegas;

    public Task<ResponseGeneric<ICollection<Bodega>>> Bodegas()
        => Ejecutar(async () =>
        {
            var r = await _bodegas.ObtenerBodegasAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las bodegas");
}
