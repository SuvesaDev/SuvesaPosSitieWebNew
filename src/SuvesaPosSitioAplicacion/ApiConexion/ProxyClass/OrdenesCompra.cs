using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IOrdenesCompra" />
///
/// NOTA: GetOrdenCompraAllProvedorAsync devuelve ObjectResponseGeneric.Responses
/// tipado como `object` (el swagger no lo declara tipado); mismo patron que
/// Familias.cs). Se deserializa aqui a mano.
public sealed class OrdenesCompra : ProxyBase, IOrdenesCompra
{
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);

    private readonly IOrdenCompraApiCliente _api;

    public OrdenesCompra(IOrdenCompraApiCliente api, IContextoSesion sesion, ILogger<OrdenesCompra> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<OrdenCompraDTO>>> BuscarPorProveedor(long idProveedor)
        => Ejecutar(async () =>
        {
            // state=false trae solo las ordenes activas (no anuladas); coincide con
            // el checkbox "incluir anuladas" desestildado por defecto en el sistema actual.
            var r = await _api.GetOrdenCompraAllProvedorAsync(idProveedor, false);

            if (r.Status != ResponseStatus._0)
            {
                return EnvelopeApi.A<ICollection<OrdenCompraDTO>>(r.Status, r.CurrentException, r.ValidationErrors, null);
            }

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, ADatos(r.Responses));
        }, "buscar órdenes de compra");

    public Task<ResponseGeneric<OrdenCompraDTO>> Obtener(long idOrdenCompra)
        => Ejecutar(async () =>
        {
            var r = await _api.GetOrdenCompraAsync(idOrdenCompra);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la orden de compra");

    public Task<ResponseGeneric<OrdenCompraDTO>> Crear(OrdenCompraDTO orden)
        => Ejecutar(async () =>
        {
            var r = await _api.CreateOrdenCompraAsync(orden);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la orden de compra");

    public Task<ResponseGeneric<OrdenCompraDTO>> Editar(OrdenCompraDTO orden)
        => Ejecutar(async () =>
        {
            var r = await _api.UpdateOrdenCompraAsync(orden);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar la orden de compra");

    public Task<ResponseGeneric<OrdenCompraDTO>> Anular(long idOrdenCompra)
        => Ejecutar(async () =>
        {
            // state=true marca la orden como anulada (visto en
            // handleDesactiveOrdenCompra: startChangeStateOrdenCompra(id, true)).
            var r = await _api.DesactivateOrdenCompraAsync(idOrdenCompra, true);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "anular la orden de compra");

    private static ICollection<OrdenCompraDTO> ADatos(object? responses)
    {
        if (responses is null)
        {
            return new List<OrdenCompraDTO>();
        }

        var json = JsonSerializer.Serialize(responses, Opciones);
        return JsonSerializer.Deserialize<List<OrdenCompraDTO>>(json, Opciones)
               ?? new List<OrdenCompraDTO>();
    }
}
