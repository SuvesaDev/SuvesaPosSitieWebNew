using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IInventarioConsulta" />
public sealed class InventarioConsulta : IInventarioConsulta
{
    private readonly IInventarioApiCliente _api;
    private readonly ILogger<InventarioConsulta> _log;

    public InventarioConsulta(IInventarioApiCliente api, ILogger<InventarioConsulta> log)
    {
        _api = api;
        _log = log;
    }

    public async Task<ResponseGeneric<ICollection<InventarioDTO>>> Buscar(
        string texto, bool incluirInhabilitados = false)
    {
        var limpio = texto?.Trim() ?? string.Empty;

        if (limpio.Length < 2)
        {
            return new ResponseGeneric<ICollection<InventarioDTO>>(new List<InventarioDTO>());
        }

        // Si lo escrito son solo digitos se busca por codigo; si no, por descripcion.
        // Es lo que hace hoy la pantalla de inventario y evita pedirle al usuario
        // que elija el modo de busqueda.
        var porCodigo = limpio.All(char.IsDigit);

        var peticion = new BuscarInventarioDTO
        {
            ValorFiltro = limpio,
            Descripcion = porCodigo ? null : limpio,
            Cod_Articulo = porCodigo ? limpio : null,
            MostrarInhabilitados = incluirInhabilitados
        };

        try
        {
            var r = porCodigo
                ? await _api.BuscarCodigoArticuloAsync(peticion)
                : await _api.BuscarDescripcionAsync(peticion);

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Fallo la busqueda de inventario con {Texto}", limpio);
            return new ResponseGeneric<ICollection<InventarioDTO>>(ex);
        }
    }
}
