using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Configuracion general (menu "Configuración"). El sistema actual solo tiene un
/// valor real aqui: el porcentaje de pronto pago para clientes. El resto de la
/// pantalla original (tarifas, permisos, tipos de bonificaciones, comunicaciones,
/// valores) llama a endpoints comentados en el codigo fuente — mockup, no se migra.
/// </summary>
public interface IConfiguracion
{
    Task<ResponseGeneric<ConfiguracionCostaPet>> Obtener();

    Task<ResponseGeneric<ConfiguracionCostaPet>> Guardar(float porcentajeProntoPago);
}
