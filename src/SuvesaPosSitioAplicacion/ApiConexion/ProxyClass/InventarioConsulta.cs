using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IInventarioConsulta" />
public sealed class InventarioConsulta : ProxyBase, IInventarioConsulta
{
    private readonly IInventarioApiCliente _api;
    private readonly IStockLoteApiCliente _lotes;
    private readonly IStocksApiCliente _stocks;
    private readonly ILogger<InventarioConsulta> _log;
    private readonly HttpClient _http;

    public InventarioConsulta(
        IInventarioApiCliente api,
        IStockLoteApiCliente lotes,
        IStocksApiCliente stocks,
        IHttpClientFactory factory,
        IContextoSesion sesion,
        ILogger<InventarioConsulta> log)
        : base(sesion, log)
    {
        _api = api;
        _lotes = lotes;
        _stocks = stocks;
        _log = log;
        _http = factory.CreateClient("SeePosApi");
    }

    public Task<ResponseGeneric<UltimoCodigoArticuloDTO>> UltimoCodigo()
        => Ejecutar(async () => await LecturaEnvelope.Leer<UltimoCodigoArticuloDTO>(
            await _http.GetAsync("inventario/UltimoCodigoArticulo")),
            "consultar el último código de artículo");

    public Task<ResponseGeneric<ICollection<StockLoteDTO>>> Lotes(long idArticulo)
        => Ejecutar(async () =>
        {
            var r = await _lotes.GetStockLotesArticuloAsync(idArticulo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los lotes del articulo");

    public Task<ResponseGeneric<StockLoteDTO>> CrearLote(StockLoteDTO lote)
        => Ejecutar(async () =>
        {
            var r = await _lotes.InsertLoteAsync(lote);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear un lote de inventario");

    public Task<ResponseGeneric<bool>> EliminarLote(long idLote)
        => Ejecutar(async () =>
        {
            var r = await _lotes.DesactivateLoteAsync(idLote);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "desactivar un lote de inventario");

    public Task<ResponseGeneric<InventarioDTO>> Uno(long codigo)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerUnInventarioAsync(new BuscarInventarioDTO { Codigo = checked((int)codigo) });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el artículo completo");

    public async Task<ResponseGeneric<ICollection<InventarioDTO>>> Buscar(
        string texto, bool incluirInhabilitados = false, int? idBodega = null)
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
            MostrarInhabilitados = incluirInhabilitados,
            IdBodega = idBodega
        };

        return await Ejecutar(async () =>
        {
            var r = porCodigo
                ? await _api.BuscarCodigoArticuloAsync(peticion)
                : await _api.BuscarDescripcionAsync(peticion);

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, $"buscar en el inventario con {limpio}");
    }

    public Task<ResponseGeneric<ICollection<InventarioDTO>>> Listar(bool incluirInhabilitados = false)
        => Ejecutar(async () =>
        {
            // Sin termino: se pide por descripcion con el filtro vacio. El API
            // devuelve el listado (con su propio tope) y la pantalla filtra en cliente.
            var peticion = new BuscarInventarioDTO
            {
                ValorFiltro = string.Empty,
                Descripcion = string.Empty,
                Cod_Articulo = null,
                MostrarInhabilitados = incluirInhabilitados
            };

            var r = await _api.BuscarDescripcionAsync(peticion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "listar el inventario");

    public async Task<ResponseGeneric<ICollection<InventarioDTO>>> BuscarMag(string texto)
    {
        var resultado = await Buscar(texto);

        if (!resultado.EsCorrecta)
        {
            return resultado;
        }

        return new ResponseGeneric<ICollection<InventarioDTO>>(
            (resultado.Responses ?? Array.Empty<InventarioDTO>())
                .Where(articulo => articulo.Mag)
                .ToList());
    }

    public Task<ResponseGeneric<InventarioDTO>> Crear(InventarioDTO articulo)
        => Ejecutar(async () =>
        {
            var r = await _api.InventarioAsync(articulo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el artículo");

    public Task<ResponseGeneric<InventarioDTO>> Editar(InventarioDTO articulo)
        => Ejecutar(async () =>
        {
            var r = await _api.Actualizar4Async(articulo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar el artículo");

    public Task<ResponseGeneric<InventarioDTO>> CambiarEstado(
        EliminarInventarioDTO articulo, bool activar)
        => Ejecutar(async () =>
        {
            var r = activar
                ? await _api.Activar2Async(articulo)
                : await _api.Desactivar2Async(articulo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, activar ? "activar el artículo" : "desactivar el artículo");

    public Task<ResponseGeneric<CodigoBarrasInventarioDTO>> EliminarCodigoBarras(
        CodigoBarrasInventarioDTO codigo)
        => Ejecutar(async () =>
        {
            var r = await _api.EliminarCodigoBarrasInventarioAsync(codigo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "eliminar el código de barras");

    public Task<ResponseGeneric<bool>> ActualizarExistencia(int codArticulo, float cantidad, int codBodega = 0)
        => Ejecutar(async () =>
        {
            var r = await _stocks.ActualizarExistenciaArticuloAsync(cantidad, codBodega, codArticulo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "actualizar la existencia del artículo");

    public Task<ResponseGeneric<bool>> ActualizarCosto(string codArticulo, double costoNuevo)
        => Ejecutar(async () =>
        {
            var r = await _api.PutActualizarCostoAsync(codArticulo, costoNuevo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "actualizar el costo del artículo");
}
