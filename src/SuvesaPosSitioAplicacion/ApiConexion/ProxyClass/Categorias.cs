using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICategorias" />
public sealed class Categorias : ProxyBase, ICategorias
{
    private readonly ICategoriasApiCliente _api;

    public Categorias(ICategoriasApiCliente api, IContextoSesion sesion, ILogger<Categorias> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<CategoriasDTO>>> Obtener()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerCategoriasInventarioAsync();
            // ObtenerCategoriasInventarioAsync devuelve IEnumerable, no ICollection.
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors,
                (ICollection<CategoriasDTO>?)r.Responses?.ToList());
        }, "consultar las categorias");

    public Task<ResponseGeneric<CategoriasDTO>> Crear(CategoriasDTO categoria)
        => Ejecutar(async () =>
        {
            var r = await _api.CrearCategoriaAsync(categoria);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la categoria");
}
