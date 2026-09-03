using SuvesaPosSitioAplicacion.DTOs.Correo;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Configuración SMTP por emisor (MOTOR_CORREO_COMPROBANTES_WEB.md §3).</summary>
public interface IConfiguracionCorreo
{
    Task<ResponseGeneric<ConfiguracionCorreoVistaDTO>> Obtener(int idEmisor);

    Task<ResponseGeneric<bool>> Guardar(ConfiguracionCorreoGuardarDTO datos);

    Task<ResponseGeneric<bool>> Probar(int idEmisor, string destino);
}
