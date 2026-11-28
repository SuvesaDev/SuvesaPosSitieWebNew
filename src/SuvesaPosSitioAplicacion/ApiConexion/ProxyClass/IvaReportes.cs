using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class IvaReportes : ProxyBase, IIvaReportes
{
    private readonly HttpClient _api;

    public IvaReportes(IHttpClientFactory factory, IContextoSesion sesion, ILogger<IvaReportes> logger) : base(sesion, logger)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IvaRepercutidoResultado>> Repercutido(int idEmpresa, DateTime desde, DateTime hasta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaRepercutidoResultado>(
                await _api.GetAsync($"api/reportes-iva/repercutido?idEmpresa={idEmpresa}&desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}")),
            "consultar el IVA repercutido");

    public Task<ResponseGeneric<IvaDocumentoDetalle>> DocumentoRepercutido(bool notaCredito, long id)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaDocumentoDetalle>(
                await _api.GetAsync($"api/reportes-iva/repercutido/documento?tipo={(notaCredito ? "nc" : "venta")}&id={id}")),
            "consultar los artículos del documento");

    public Task<ResponseGeneric<IvaLineasPeriodo>> LineasRepercutido(int idEmpresa, DateTime desde, DateTime hasta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaLineasPeriodo>(
                await _api.GetAsync($"api/reportes-iva/repercutido/lineas?idEmpresa={idEmpresa}&desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}")),
            "consultar las líneas del IVA repercutido");

    public Task<ResponseGeneric<IvaSoportadoResultado>> Soportado(int idEmpresa, DateTime desde, DateTime hasta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaSoportadoResultado>(
                await _api.GetAsync($"api/reportes-iva/soportado?idEmpresa={idEmpresa}&desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}")),
            "consultar el IVA soportado");

    public Task<ResponseGeneric<IvaSoportadoDocumento>> DocumentoSoportado(string clase, long id)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaSoportadoDocumento>(
                await _api.GetAsync($"api/reportes-iva/soportado/documento?clase={Uri.EscapeDataString(clase)}&id={id}")),
            "consultar los artículos del documento");

    public Task<ResponseGeneric<IvaSoportadoLineasPeriodo>> LineasSoportado(int idEmpresa, DateTime desde, DateTime hasta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaSoportadoLineasPeriodo>(
                await _api.GetAsync($"api/reportes-iva/soportado/lineas?idEmpresa={idEmpresa}&desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}")),
            "consultar las líneas del IVA soportado");

    public Task<ResponseGeneric<IvaProrrataResultado>> Prorrata(int idEmpresa, int anio, int hastaMes)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaProrrataResultado>(
                await _api.GetAsync($"api/reportes-iva/prorrata?idEmpresa={idEmpresa}&anio={anio}&hastaMes={hastaMes}")),
            "consultar la prorrata");

    public Task<ResponseGeneric<IvaProrrataParametro>> ParametrosProrrata(int idEmpresa, int anio)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaProrrataParametro>(
                await _api.GetAsync($"api/reportes-iva/prorrata/parametros?idEmpresa={idEmpresa}&anio={anio}")),
            "consultar los parámetros de la prorrata");

    public Task<ResponseGeneric<IvaProrrataParametro>> GuardarParametrosProrrata(IvaProrrataParametro parametro)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaProrrataParametro>(
                await _api.PutAsJsonAsync("api/reportes-iva/prorrata/parametros", parametro, LecturaEnvelope.Json)),
            "guardar los parámetros de la prorrata");

    public Task<ResponseGeneric<IvaProrrataDeclarados>> DeclaradosProrrata(int idEmpresa, int anio)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaProrrataDeclarados>(
                await _api.GetAsync($"api/reportes-iva/prorrata/declarados?idEmpresa={idEmpresa}&anio={anio}")),
            "consultar lo declarado");

    public Task<ResponseGeneric<IvaProrrataDeclarados>> GuardarDeclaradosProrrata(IvaProrrataDeclarados declarados)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaProrrataDeclarados>(
                await _api.PutAsJsonAsync("api/reportes-iva/prorrata/declarados", declarados, LecturaEnvelope.Json)),
            "guardar lo declarado");

    public Task<ResponseGeneric<IvaProrrataLiquidacion>> CongelarLiquidacionProrrata(CongelarLiquidacion datos)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IvaProrrataLiquidacion>(
                await _api.PostAsJsonAsync("api/reportes-iva/prorrata/liquidacion", datos, LecturaEnvelope.Json)),
            "congelar la liquidación");

    public Task<ResponseGeneric<List<IvaProrrataLiquidacion>>> LiquidacionesProrrata(int idEmpresa, int anio)
        => Ejecutar(async () => await LecturaEnvelope.Leer<List<IvaProrrataLiquidacion>>(
                await _api.GetAsync($"api/reportes-iva/prorrata/liquidaciones?idEmpresa={idEmpresa}&anio={anio}")),
            "consultar las liquidaciones");

    public Task<ResponseGeneric<PorcentajeProrrata>> PorcentajeProrrataDeCompra(long idCompra)
        => Ejecutar(async () => await LecturaEnvelope.Leer<PorcentajeProrrata>(
                await _api.GetAsync($"api/reportes-iva/prorrata/porcentaje-compra?idCompra={idCompra}")),
            "consultar el porcentaje de prorrata");
}
