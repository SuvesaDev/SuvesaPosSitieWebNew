using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Operaciones necesarias para registrar una compra. El contrato agrupa la
/// transacción y los catálogos que la ventana antigua cargaba al desbloquearse.
/// </summary>
public interface ICompras
{
    Task<ResponseGeneric<ICollection<EmpresaDTO>>> Empresas();
    Task<ResponseGeneric<ICollection<Moneda>>> Monedas();
    Task<ResponseGeneric<ICollection<Bodega>>> Bodegas(bool costaPets);
    Task<ResponseGeneric<Usuario>> ValidarClaveInterna(string contrasena);
    Task<ResponseGeneric<FacturaCompraDTO>> Crear(FacturaCompraDTO compra);
    Task<ResponseGeneric<FacturaCompraDTO>> Editar(FacturaCompraDTO compra);
    Task<ResponseGeneric<FacturaCompraDTO>> Anular(FacturaCompraDTO compra);
    Task<ResponseGeneric<ICollection<FacturaCompraDTO>>> Buscar(FiltroFacturaCompras filtro);
    Task<ResponseGeneric<FacturaCompraDTO>> Obtener(long id);
    Task<ResponseGeneric<ICollection<CatalogoProductosInternosDTO>>> CatalogosInternos(ICollection<CatalogoProductosInternosDTO> productos);
    Task<ResponseGeneric<CatalogoProductosInternosDTO>> VincularArticuloXml(CatalogoProductosInternosDTO producto);
    Task<ResponseGeneric<ICollection<ActualizarPreciosArticulosDTO>>> ActualizarPrecios(ICollection<ActualizarPreciosArticulosDTO> precios);
}
