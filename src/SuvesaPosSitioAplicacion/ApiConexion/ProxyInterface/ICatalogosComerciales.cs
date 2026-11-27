using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Catálogos editables del módulo comercial: canal del cliente, especie y función terapéutica.</summary>
public interface ICatalogosComerciales
{
    Task<ResponseGeneric<IReadOnlyList<CatalogoComercialDTO>>> Listar(TipoCatalogoComercial tipo, bool incluirInactivos = false);
    Task<ResponseGeneric<CatalogoComercialDTO>> Crear(TipoCatalogoComercial tipo, CatalogoComercialDTO valor);
    Task<ResponseGeneric<CatalogoComercialDTO>> Actualizar(TipoCatalogoComercial tipo, int id, CatalogoComercialDTO valor);
}
