using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Comandos transaccionales de venta (SANEAMIENTO Fase 4): devolución interna
/// no fiscal — usada por "Operaciones fallidas" para el flujo D10
/// (NC sin ruta fiscal + recrear la factura).
/// </summary>
public interface IVentaOrquestada
{
    Task<ResponseGeneric<DevolucionInternaResultadoWebDTO>> DevolucionInterna(DevolucionInternaComandoWebDTO comando);
}
