using SuvesaPosSitioAplicacion.DTOs.Tarifas;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Catálogo de tarifas (listas de precio) y los precios por artículo de las tarifas nuevas.</summary>
public interface ITarifasPrecio
{
    Task<ResponseGeneric<IReadOnlyList<TarifaPrecioDTO>>> Listar(bool incluirInactivas = false);
    Task<ResponseGeneric<TarifaPrecioDTO>> Crear(TarifaPrecioDTO tarifa);
    Task<ResponseGeneric<TarifaPrecioDTO>> Actualizar(short idTarifa, TarifaPrecioDTO tarifa);
    /// <summary>Precios de un artículo en las tarifas nuevas (A–D vienen con el artículo).</summary>
    Task<ResponseGeneric<IReadOnlyList<PrecioTarifaArticuloDTO>>> PreciosDeArticulo(long idArticulo);
    Task<ResponseGeneric<IReadOnlyList<PrecioTarifaArticuloDTO>>> GuardarPreciosDeArticulo(long idArticulo, IReadOnlyList<PrecioTarifaArticuloDTO> precios);
    /// <summary>Precio de varios artículos en una tarifa (A–D incluidas).</summary>
    Task<ResponseGeneric<IReadOnlyList<PrecioArticuloEnTarifaDTO>>> PreciosEnTarifa(short idTarifa, IEnumerable<long> idsArticulo);
}
