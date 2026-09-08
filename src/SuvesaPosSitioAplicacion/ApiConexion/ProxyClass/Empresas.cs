using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IEmpresas" />
public sealed class Empresas : ProxyBase, IEmpresas
{
    private readonly IIdentificacionApiCliente _identificacion;
    private readonly IGeografiaApiCliente _geografia;
    private readonly IBancosApiCliente _bancos;
    private readonly IMonedaApiCliente _monedas;
    private readonly IHaciendaApiCliente _hacienda;
    private readonly ICentrosApiCliente _centros;

    public Empresas(
        IIdentificacionApiCliente identificacion,
        IGeografiaApiCliente geografia,
        IBancosApiCliente bancos,
        IMonedaApiCliente monedas,
        IHaciendaApiCliente hacienda,
        ICentrosApiCliente centros,
        IContextoSesion sesion,
        ILogger<Empresas> log)
        : base(sesion, log)
    {
        _identificacion = identificacion;
        _geografia = geografia;
        _bancos = bancos;
        _monedas = monedas;
        _hacienda = hacienda;
        _centros = centros;
    }

    public Task<ResponseGeneric<ICollection<TipoIdentificacionDTO>>> TiposIdentificacion()
        => Ejecutar(async () =>
        {
            var r = await _identificacion.ObtenerAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los tipos de documento");

    public Task<ResponseGeneric<ICollection<ProvinciaDTO>>> Provincias()
        => Ejecutar(async () =>
        {
            var r = await _geografia.GetProvinciasAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las provincias");

    public Task<ResponseGeneric<ICollection<CantonDTO>>> Cantones(int idProvincia)
        => Ejecutar(async () =>
        {
            var r = await _geografia.GetCantonAsync(idProvincia);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los cantones");

    public Task<ResponseGeneric<ICollection<DistritoDTO>>> Distritos(int idCanton)
        => Ejecutar(async () =>
        {
            var r = await _geografia.GetDistritoAsync(idCanton);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los distritos");

    public Task<ResponseGeneric<ICollection<EntidadesBancariasDTO>>> Bancos()
        => Ejecutar(async () =>
        {
            var r = await _bancos.ObtenerBancosAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los bancos");

    public Task<ResponseGeneric<ICollection<Moneda>>> Monedas()
        => Ejecutar(async () =>
        {
            var r = await _monedas.ObtenerMonedasInventarioAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las monedas");

    public Task<ResponseGeneric<ICollection<ActividadesEmpresaDTO>>> ActividadesHacienda(string identificacion)
        => Ejecutar(async () =>
        {
            var r = await _hacienda.ObtenerDatosActividadesAsync(identificacion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las actividades en Hacienda");

    public Task<ResponseGeneric<EmpresaDTO>> Crear(EmpresaDTO empresa)
        => Ejecutar(async () =>
        {
            var r = await _centros.CrearEmpresaAsync(empresa);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la empresa");
}
