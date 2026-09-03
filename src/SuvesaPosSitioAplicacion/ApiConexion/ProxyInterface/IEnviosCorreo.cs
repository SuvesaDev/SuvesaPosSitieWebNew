using SuvesaPosSitioAplicacion.DTOs.Correo;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Bandeja de envíos de correo de comprobantes (MOTOR_CORREO_COMPROBANTES_WEB.md §4).</summary>
public interface IEnviosCorreo
{
    Task<ResponseGeneric<PaginaEnviosCorreoDTO>> Listar(
        string? estado, int? idEmisor, DateTime? desde, DateTime? hasta, string? texto, int pagina, int tamano);

    Task<ResponseGeneric<bool>> Reenviar(string clave);
}
