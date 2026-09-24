using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Contabilidad General — W1: solo lo que la resolución del feature flag y la
/// pantalla de activación de emisor necesitan. El resto de <c>api/contabilidad/*</c> se
/// agrega proxy por proxy en fases posteriores (W2+), no todo de una vez.</summary>
public interface IContabilidad
{
    Task<ResponseGeneric<EstadoActivacionContabilidadDTO>> EstadoActivacion(long idEmpresa, int? idEmisor);
    Task<ResponseGeneric<bool>> ActivarEmpresa(long idEmpresa);
    Task<ResponseGeneric<bool>> DesactivarEmpresa(long idEmpresa);
    Task<ResponseGeneric<long>> ActivarEmisor(ActivarEmisorDTO comando);
    Task<ResponseGeneric<bool>> DesactivarEmisor(int idEmisor);
    Task<ResponseGeneric<bool>> ConfirmarClave(string contrasena);
}
