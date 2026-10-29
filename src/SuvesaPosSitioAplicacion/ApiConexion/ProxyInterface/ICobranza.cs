using SuvesaPosSitioAplicacion.DTOs.Cobranza;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Gestión y seguimiento de cobranzas (api/cobranza): facturas vencidas, seguimientos, promesas y dashboards.</summary>
public interface ICobranza
{
    Task<ResponseGeneric<PaginaFacturasVencidasWeb>> FacturasVencidas(FiltroFacturasVencidasWeb filtro);

    Task<ResponseGeneric<List<SeguimientoResumenWebDTO>>> Seguimientos(string? cobrador, EstadoSeguimientoWeb? estado, long? idCliente);

    Task<ResponseGeneric<SeguimientoDetalleWebDTO>> Detalle(long idSeguimiento);

    Task<ResponseGeneric<long>> Abrir(AbrirSeguimientoWebComando comando);

    Task<ResponseGeneric<long>> RegistrarInteraccion(long idSeguimiento, NuevaInteraccionWebComando comando);

    Task<ResponseGeneric<long>> RegistrarPromesa(long idSeguimiento, NuevaPromesaWebComando comando);

    Task<ResponseGeneric<bool>> Reasignar(long idSeguimiento, ReasignarWebComando comando);

    Task<ResponseGeneric<DashboardCobradorWebDTO>> DashboardCobrador();

    Task<ResponseGeneric<DashboardGerencialWebDTO>> DashboardGerencial(DateOnly desde, DateOnly hasta);
}
