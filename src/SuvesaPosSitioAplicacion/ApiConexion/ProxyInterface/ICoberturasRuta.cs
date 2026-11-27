using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Cobertura de vacaciones por ruta: quién atiende la ruta mientras un colaborador está fuera.</summary>
public interface ICoberturasRuta
{
    Task<ResponseGeneric<IReadOnlyList<CoberturaRutaDTO>>> Listar(FiltroCoberturasDTO filtro);
    Task<ResponseGeneric<IReadOnlyList<UsuarioCubridorDTO>>> UsuariosElegibles();
    Task<ResponseGeneric<CoberturaRutaDTO>> Crear(CoberturaRutaDTO cobertura);
    Task<ResponseGeneric<CoberturaRutaDTO>> Actualizar(long idCobertura, CoberturaRutaDTO cobertura);
    /// <summary>La cobertura se anula (queda como historial).</summary>
    Task<ResponseGeneric<bool>> Anular(long idCobertura);
}
