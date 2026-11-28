using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Centro de alertas comerciales: la bandeja de cada persona, su gestión y la supervisión del administrador.</summary>
public interface IAlertasComerciales
{
    Task<ResponseGeneric<PaginaAlertasComercialesDTO>> Bandeja(FiltroAlertasComercialesDTO filtro);
    Task<ResponseGeneric<ResumenAlertasComercialesDTO>> Resumen();
    /// <summary>Marca como gestionada una alerta de la propia bandeja. Un solo estado para todos los destinatarios.</summary>
    Task<ResponseGeneric<AlertaComercialDTO>> Gestionar(long idAlerta, string? nota);
    Task<ResponseGeneric<PaginaAlertasComercialesDTO>> Supervision(FiltroAlertasComercialesDTO filtro);
    Task<ResponseGeneric<AlertaComercialDTO>> GestionarComoSupervisor(long idAlerta, string? nota);
    /// <summary>Corre ahora la misma generación del proceso periódico.</summary>
    Task<ResponseGeneric<ResultadoGeneracionAlertasDTO>> GenerarAhora();
}
