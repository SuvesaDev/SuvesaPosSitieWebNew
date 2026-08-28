using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IOrdenesCompraConsulta" />
///
/// <remarks>
/// El contrato generado declara <c>responses</c> como <see cref="object"/> aun
/// cuando estos dos endpoints devuelven arreglos de pedidos. Se normaliza en este
/// borde, para que las vistas no dependan de <c>JsonElement</c>.
/// </remarks>
public sealed class OrdenesCompraConsulta : ProxyBase, IOrdenesCompraConsulta
{
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);

    private readonly IOrdenCompraApiCliente _api;

    public OrdenesCompraConsulta(
        IOrdenCompraApiCliente api,
        IContextoSesion sesion,
        ILogger<OrdenesCompraConsulta> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<OrdenCompraDTO>>> Obtener()
        => Ejecutar(async () =>
        {
            var r = await _api.GetOrdenCompraAllAsync();
            return ALista(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los pedidos de compra");

    public Task<ResponseGeneric<ICollection<OrdenCompraDTO>>> Buscar(long numero, bool anuladas)
        => Ejecutar(async () =>
        {
            var r = await _api.GetOrdenComprasDataBasicAsync(numero, anuladas);
            return ALista(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, $"consultar el pedido de compra {numero}");

    private static ResponseGeneric<ICollection<OrdenCompraDTO>> ALista(
        ResponseStatus estado,
        string? excepcion,
        ICollection<string>? errores,
        object? responses)
    {
        if (estado != ResponseStatus._0)
        {
            return EnvelopeApi.A<ICollection<OrdenCompraDTO>>(estado, excepcion, errores, null);
        }

        return EnvelopeApi.A(estado, excepcion, errores, ADatos(responses));
    }

    private static ICollection<OrdenCompraDTO> ADatos(object? responses)
    {
        if (responses is null)
        {
            return Array.Empty<OrdenCompraDTO>();
        }

        var json = JsonSerializer.Serialize(responses, Opciones);
        return JsonSerializer.Deserialize<List<OrdenCompraDTO>>(json, Opciones)
               ?? new List<OrdenCompraDTO>();
    }
}
