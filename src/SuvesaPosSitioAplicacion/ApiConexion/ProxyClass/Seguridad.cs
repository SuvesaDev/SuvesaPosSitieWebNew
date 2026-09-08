using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <summary>
/// ARQUETIPO DE PROXY. Los otros 50 se escriben igual que este.
///
/// Reglas:
///   1. Envuelve el cliente generado por NSwag; nunca construye HttpClient a mano.
///   2. Traduce el envelope del API a ResponseGeneric con <see cref="EnvelopeApi"/>.
///   3. Atrapa la excepcion aqui: una View jamas debe ver una ApiException.
///   4. No lleva logica de negocio. Solo transporte y traduccion.
/// </summary>
public sealed class Seguridad : ProxyBase, ISeguridad
{
    private readonly IUsuarioApiCliente _usuario;
    private readonly ICentrosApiCliente _centros;
    private readonly ILogger<Seguridad> _log;

    public Seguridad(
        IUsuarioApiCliente usuario,
        ICentrosApiCliente centros,
        IContextoSesion sesion,
        ILogger<Seguridad> log)
        : base(sesion, log)
    {
        _usuario = usuario;
        _centros = centros;
        _log = log;
    }

    public Task<ResponseGeneric<Autenticacion>> Login(string usuario, string password)
        => Ejecutar(async () =>
        {
            var r = await _usuario.LoginNuevoAsync(new Credencial
            {
                Usuario = usuario,
                Password = password
            });

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, $"iniciar sesion de {usuario}");

    public Task<ResponseGeneric<ICollection<SucursalDTO>>> ObtenerSucursales()
        => Ejecutar(async () =>
        {
            var r = await _centros.ObtenerSucursalAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las sucursales");
}
