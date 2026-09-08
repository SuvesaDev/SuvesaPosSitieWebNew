using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Formas de pago con sus propiedades semánticas (activa, vuelto, referencia,
/// afecta caja, moneda extranjera, código de Hacienda) — SANEAMIENTO Fase 8.1.
/// </summary>
public interface IFormasPagoConfig
{
    Task<ResponseGeneric<IReadOnlyList<FormaPagoConfigWebDTO>>> Listar();

    Task<ResponseGeneric<FormaPagoConfigWebDTO>> Guardar(FormaPagoConfigWebDTO forma);
}
