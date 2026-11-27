using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Clasificación comercial de clientes (canal) y de artículos (especies, funciones terapéuticas y margen mínimo).</summary>
public interface IClasificacionComercial
{
    Task<ResponseGeneric<PaginaClientesClasificacionDTO>> ListarClientes(FiltroClasificacionClientesDTO filtro);
    /// <summary>Asigna el canal a varios clientes o sucursales; con canal nulo, quita el canal propio.</summary>
    Task<ResponseGeneric<ResultadoAsignacionDTO>> AsignarCanal(AsignarCanalRequest request);
    Task<ResponseGeneric<OpcionesFiltroArticulosDTO>> OpcionesFiltroArticulos();
    Task<ResponseGeneric<PaginaArticulosClasificacionDTO>> ListarArticulos(FiltroClasificacionArticulosDTO filtro);
    Task<ResponseGeneric<ArticuloClasificacionDTO>> GuardarArticulo(long idInventario, GuardarClasificacionArticuloRequest request);
    Task<ResponseGeneric<ResultadoAsignacionDTO>> AsignarMasiva(AsignarClasificacionMasivaRequest request);
}
