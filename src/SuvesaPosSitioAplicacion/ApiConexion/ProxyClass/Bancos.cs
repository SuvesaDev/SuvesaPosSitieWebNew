using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <summary>
/// Proxy de catalogo. Mismo molde que <see cref="Seguridad"/>: envuelve el cliente
/// generado, traduce el envelope y no deja escapar excepciones.
///
/// Los ~30 catalogos de la Ola 6 se escriben exactamente asi.
/// </summary>
public sealed class Bancos : ProxyBase, IBancos
{
    private readonly IBancosApiCliente _api;
    private readonly ILogger<Bancos> _log;

    public Bancos(IBancosApiCliente api, IContextoSesion sesion, ILogger<Bancos> log)
        : base(sesion, log)
    {
        _api = api;
        _log = log;
    }

    public Task<ResponseGeneric<ICollection<EntidadesBancariasDTO>>> Obtener()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerBancosAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los bancos");

    public Task<ResponseGeneric<EntidadesBancariasDTO>> ObtenerPorId(int id)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerBancosPorIdAsync(id);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el banco");

    public Task<ResponseGeneric<EntidadesBancariasDTO>> Crear(EntidadesBancariasDTO banco)
        => Ejecutar(async () =>
        {
            var r = await _api.CrearBancoAsync(banco);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el banco");

    public Task<ResponseGeneric<EntidadesBancariasDTO>> Editar(EntidadesBancariasDTO banco)
        => Ejecutar(async () =>
        {
            var r = await _api.EditarBancoAsync(banco);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar el banco");

    public Task<ResponseGeneric<EntidadesBancariasDTO>> Activar(int id)
        => Ejecutar(async () =>
        {
            var r = await _api.ActivarBancosAsync(id);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "activar el banco");

    public Task<ResponseGeneric<EntidadesBancariasDTO>> Inactivar(int id)
        => Ejecutar(async () =>
        {
            var r = await _api.InactivarBancoAsync(id);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "desactivar el banco");

}
