using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Fórmulas y conversión de producción de inventario. Se conserva fuera del
/// proxy de consulta porque estas operaciones alteran existencias.
/// </summary>
public interface IProduccionInventario
{
    Task<ResponseGeneric<ICollection<Bodega>>> Bodegas(bool costaPets);

    Task<ResponseGeneric<ICollection<ArticulosRelacionadosDTO>>> Formula(long articuloPrincipal);

    Task<ResponseGeneric<bool>> GuardarFormula(long principal, long componente, float cantidad, bool activa);

    Task<ResponseGeneric<int>> Calcular(CalculadoraDTO calculo);
}
