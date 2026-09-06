using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Caja;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;
using System.Net.Http.Json;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc />
public sealed class CajaOperaciones : ProxyBase, ICajaOperaciones
{
    private readonly ICajaApiCliente _caja;
    private readonly IArqueoApiCliente _arqueo;
    private readonly ICierreCajaApiCliente _cierre;
    private readonly IBancosApiCliente _bancos;
    private readonly ICentrosApiCliente _centros;
    private readonly IUsuarioApiCliente _usuarios;
    private readonly IMonedaApiCliente _moneda;
    private readonly HttpClient _api;

    public CajaOperaciones(
        ICajaApiCliente caja, IArqueoApiCliente arqueo, ICierreCajaApiCliente cierre,
        IBancosApiCliente bancos, ICentrosApiCliente centros, IUsuarioApiCliente usuarios, IMonedaApiCliente moneda,
        IHttpClientFactory factory,
        IContextoSesion sesion, ILogger<CajaOperaciones> log) : base(sesion, log)
    {
        _caja = caja;
        _arqueo = arqueo;
        _cierre = cierre;
        _bancos = bancos;
        _centros = centros;
        _usuarios = usuarios;
        _moneda = moneda;
        _api = factory.CreateClient("SeePosApi");
    }

    public Task<ResponseGeneric<Usuario>> ValidarClaveInterna(string contrasena) => Ejecutar(async () =>
    {
        var r = await _usuarios.ValidarClaveInternaSinUsuarioAsync(contrasena);
        return r.Status == ResponseStatus._0
            ? EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses)
            : new ResponseGeneric<Usuario>("Contraseña incorrecta.");
    }, "validar la clave interna de caja");

    public Task<ResponseGeneric<UsuarioCajaAbiertaValidadaWebDTO>> ValidarClaveInternaConCajaAbierta(string contrasena)
        => Ejecutar(async () => await LecturaEnvelope.Leer<UsuarioCajaAbiertaValidadaWebDTO>(
            await _api.PostAsJsonAsync("Caja/ValidarClaveInternaConCajaAbierta", new { contrasena })),
            "validar la clave interna y la caja abierta");

    public Task<ResponseGeneric<ICollection<CajasCantidad>>> CajasDisponibles() => Ejecutar(async () =>
    {
        var r = await _caja.ObtenerCajasAsync();
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar las cajas disponibles");

    public Task<ResponseGeneric<ICollection<CajasCantidad>>> TodasLasCajas() => Ejecutar(async () =>
    {
        var r = await _caja.ObtenerTodasCajasAsync();
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar las cajas");

    public Task<ResponseGeneric<CajasCantidad>> CrearCaja(long numCaja)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CajasCantidad>(
            await _api.PostAsJsonAsync("Caja/CrearCaja", new { NumCaja = numCaja })),
            "crear la caja");

    public Task<ResponseGeneric<bool>> EliminarCaja(long idCaja)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PostAsJsonAsync("Caja/EliminarCaja", new { IdCaja = idCaja })),
            "eliminar la caja");

    public Task<ResponseGeneric<ICollection<DenominacionMonedum>>> Denominaciones() => Ejecutar(async () =>
    {
        var r = await _caja.GetDenominacionMonedasAsync();
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar las denominaciones");

    public Task<ResponseGeneric<ICollection<User>>> CajerosConCajaAbierta() => Ejecutar(async () =>
    {
        var r = await _caja.ObtenerUsuariosCajaAbiertaAsync();
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar los cajeros");

    public Task<ResponseGeneric<ICollection<AperturaCajaDTO>>> AperturasSinCerrar() => Ejecutar(async () =>
    {
        var r = await _caja.ObtenerAperturasDeCajaSinCerrarAsync();
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar las aperturas sin cerrar");

    public Task<ResponseGeneric<ICollection<ObtenerAperturaCajaDTO>>> AperturasSinArqueo() => Ejecutar(async () =>
    {
        var r = await _arqueo.ObtenerAperturasDeCajaSinArqueoAsync();
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar las aperturas sin arqueo");

    public Task<ResponseGeneric<AperturaCajaDTO>> CrearApertura(AperturaCajaDTO apertura) => Ejecutar(async () =>
    {
        var r = await _caja.CrearAperturaCajaAsync(apertura);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "crear la apertura de caja");

    public Task<ResponseGeneric<ArqueoCajaDTO>> CrearArqueo(ArqueoCajaDTO arqueo) => Ejecutar(async () =>
    {
        var r = await _arqueo.CrearArqueoCajaAsync(arqueo);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "crear el arqueo de caja");

    public Task<ResponseGeneric<ObtenerDatosCierreCaja>> DatosCierre(long numeroApertura) => Ejecutar(async () =>
    {
        var r = await _cierre.ObtenerDatosDelCierreCajaAsync(numeroApertura);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar los datos del cierre");

    public Task<ResponseGeneric<CierreCajaDTO>> CrearCierre(CierreCajaDTO cierre) => Ejecutar(async () =>
    {
        var r = await _cierre.CrearCierreDeCajaAsync(cierre);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "crear el cierre de caja");

    public Task<ResponseGeneric<ICollection<EntidadesBancariasDTO>>> Bancos() => Ejecutar(async () =>
    {
        var r = await _bancos.ObtenerBancosAsync();
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar los bancos");

    public Task<ResponseGeneric<ICollection<EmpresaDTO>>> Empresas() => Ejecutar(async () =>
    {
        var r = await _centros.ObtenerEmpresasFacturacionAsync();
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar las empresas");

    public Task<ResponseGeneric<ICollection<CuentaBancariaDTO>>> Cuentas(int banco, int empresa) => Ejecutar(async () =>
    {
        var r = await _bancos.ObtenerCuentasPorBancoAsync(banco, empresa);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar las cuentas bancarias");

    public Task<ResponseGeneric<ICollection<PreDepositosDTO>>> PreDepositosDeApertura(long apertura) => Ejecutar(async () =>
    {
        var r = await _bancos.ObtenerPreDepositosPorNumAperturaAsync(apertura);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar los pre depósitos");

    public Task<ResponseGeneric<ICollection<PreDepositosBuscarDTO>>> BuscarPreDepositos(FiltroBusquedaPreDepositosDTO filtro) => Ejecutar(async () =>
    {
        var r = await _bancos.ObtenerPreDepositosAsync(filtro);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "buscar los pre depósitos");

    public Task<ResponseGeneric<ICollection<DepositosBuscarDTO>>> BuscarDepositos(FiltroBusquedaDepositosDTO filtro) => Ejecutar(async () =>
    {
        var r = await _bancos.ObtenerDepositosAsync(filtro);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "buscar los depósitos");

    public Task<ResponseGeneric<PreDepositosDTO>> CrearPreDeposito(PreDepositosDTO deposito) => Ejecutar(async () =>
    {
        var r = await _bancos.CrearPreDepositoAsync(deposito);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "crear el pre depósito");

    public Task<ResponseGeneric<PreDepositosDTO>> EliminarPreDeposito(int id) => Ejecutar(async () =>
    {
        var r = await _bancos.EliminarPreDepositoAsync(id);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "eliminar el pre depósito");

    public Task<ResponseGeneric<DepositosDTO>> CrearDeposito(DepositosDTO deposito) => Ejecutar(async () =>
    {
        var r = await _bancos.CrearDepositoAsync(deposito);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "crear el depósito");

    public Task<ResponseGeneric<Moneda>> TipoCambioDolar() => Ejecutar(async () =>
    {
        var r = await _moneda.ObtenerTipoCambioAsync();
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar el tipo de cambio del dólar");

    public Task<ResponseGeneric<ICollection<AperturaCajaFiltroResultadoDTO>>> BuscarAperturas(AperturaCajaFiltroDTO filtro) => Ejecutar(async () =>
    {
        var r = await _caja.ObtenerFiltrosAperturaCajaAsync(filtro);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "buscar las aperturas de caja");

    public Task<ResponseGeneric<AperturaCajaDTO>> ObtenerApertura(int numeroApertura) => Ejecutar(async () =>
    {
        var r = await _caja.ConsultarAperturaCajaAsync(numeroApertura);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar la apertura de caja");

    public Task<ResponseGeneric<AperturaDenominacionDTO>> EditarDenominacionApertura(AperturaDenominacionDTO linea) => Ejecutar(async () =>
    {
        var r = await _caja.EditarAperturaDenominacionAsync(linea);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "editar la denominación de la apertura");

    public Task<ResponseGeneric<AperturaTotalTopeDTO>> EditarTotalTopeApertura(AperturaTotalTopeDTO linea) => Ejecutar(async () =>
    {
        var r = await _caja.EditarAperturaTotalTopeAsync(linea);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "editar el total tope de la apertura");

    public Task<ResponseGeneric<AperturaCajaDTO>> AnularApertura(int numeroApertura) => Ejecutar(async () =>
    {
        var r = await _caja.DeleteAperturaCajaAsync(numeroApertura);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "anular la apertura de caja");

    public Task<ResponseGeneric<ICollection<ArqueoCajaFiltroResultadoDTO>>> BuscarArqueos(ArqueoCajaFiltroDTO filtro) => Ejecutar(async () =>
    {
        var r = await _arqueo.ObtenerFiltrosArqueoCajaAsync(filtro);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "buscar los arqueos de caja");

    public Task<ResponseGeneric<ArqueoCajaDTO>> ObtenerArqueo(long id) => Ejecutar(async () =>
    {
        var r = await _arqueo.ConsultarArqueoCajaAsync(id);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar el arqueo de caja");

    public Task<ResponseGeneric<ArqueoCajaDTO>> EditarArqueo(ArqueoCajaDTO arqueo) => Ejecutar(async () =>
    {
        var r = await _arqueo.EditarArqueoCajaAsync(arqueo);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "editar el arqueo de caja");

    public Task<ResponseGeneric<ArqueoCajaDTO>> AnularArqueo(long id) => Ejecutar(async () =>
    {
        var r = await _arqueo.DeleteArqueoCajaAsync(id);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "anular el arqueo de caja");

    public Task<ResponseGeneric<ICollection<FiltroCierreCajaBusquedaDTO>>> BuscarCierres(BuscarCierreCajaDTO filtro) => Ejecutar(async () =>
    {
        var r = await _cierre.BuscarCierreCajaAsync(filtro);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "buscar los cierres de caja");

    public Task<ResponseGeneric<CierreCajaDTO>> AnularCierre(long idCierre) => Ejecutar(async () =>
    {
        var r = await _cierre.AnularCierreDeCajaAsync(idCierre);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "anular el cierre de caja");

    public Task<ResponseGeneric<ObtenerDatosCierreCaja>> DatosCierreRegistrado(long numeroApertura) => Ejecutar(async () =>
    {
        var r = await _cierre.ObtenerDatosDelCierreCajaInsertadoAsync(numeroApertura);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar el consolidado del cierre registrado");

    public Task<ResponseGeneric<ICollection<OpcionesDePagoCierreCaja>>> DocumentosDeApertura(long numeroApertura) => Ejecutar(async () =>
    {
        var r = await _arqueo.ObtenerDocumentosEnArqueoCajaAsync(numeroApertura);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar los documentos de la apertura");

    public Task<ResponseGeneric<ArqueoMontoDepositoDTO>> MontoDepositosDeApertura(int numeroApertura) => Ejecutar(async () =>
    {
        var r = await _arqueo.ObtenerMontoDepositosCajaAsync(numeroApertura);
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar el monto de depósitos registrados");

    public Task<ResponseGeneric<ICollection<FormasPagoDTO>>> TiposDeTarjeta() => Ejecutar(async () =>
    {
        var r = await _caja.GetTipoTarjetumAsync();
        return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
    }, "consultar los tipos de tarjeta");
}
