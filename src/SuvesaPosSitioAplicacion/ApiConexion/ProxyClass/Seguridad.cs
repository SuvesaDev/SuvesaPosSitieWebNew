using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

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
public sealed class Seguridad : ISeguridad
{
    private readonly IUsuarioApiCliente _usuario;
    private readonly ICentrosApiCliente _centros;
    private readonly ILogger<Seguridad> _log;

    public Seguridad(
        IUsuarioApiCliente usuario,
        ICentrosApiCliente centros,
        ILogger<Seguridad> log)
    {
        _usuario = usuario;
        _centros = centros;
        _log = log;
    }

    public async Task<ResponseGeneric<Autenticacion>> Login(string usuario, string password)
    {
        try
        {
            var r = await _usuario.LoginNuevoAsync(new Credencial
            {
                Usuario = usuario,
                Password = password
            });

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Fallo el inicio de sesion del usuario {Usuario}", usuario);
            return new ResponseGeneric<Autenticacion>(ex);
        }
    }

    public async Task<ResponseGeneric<ICollection<SucursalDTO>>> ObtenerSucursales()
    {
        try
        {
            var r = await _centros.ObtenerSucursalAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Fallo la consulta de sucursales");
            return new ResponseGeneric<ICollection<SucursalDTO>>(ex);
        }
    }
}
