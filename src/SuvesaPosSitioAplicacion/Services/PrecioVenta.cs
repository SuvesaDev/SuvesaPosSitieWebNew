using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Precio de góndola en colones. Si el costo del artículo está en otra moneda y
/// tiene conversión activa, multiplica por el tipo de cambio congelado en el
/// artículo (Monedas.ValorVenta al calcular/guardar). No recotiza en la factura.
/// Las tarifas B, C, D y Promo usan el mismo factor que la A.
/// </summary>
public static class PrecioVenta
{
    public const int Colones = 1;

    public static decimal FactorColones(InventarioDTO? articulo)
    {
        if (articulo is null || articulo.MonedaCosto <= Colones || !articulo.ConviertePrecioColones)
            return 1m;
        var tc = Formato.AImporte(articulo.TipoCambioVenta);
        return tc > 0 ? tc : 1m;
    }

    public static decimal EnColones(InventarioDTO? articulo, decimal precioEnMonedaCosto)
        => Math.Round(precioEnMonedaCosto * FactorColones(articulo), 2);

    public static decimal EnColones(InventarioDTO? articulo, float precioEnMonedaCosto)
        => EnColones(articulo, Formato.AImporte(precioEnMonedaCosto));

    public static decimal EnColones(InventarioDTO? articulo, double precioEnMonedaCosto)
        => EnColones(articulo, Formato.AImporte(precioEnMonedaCosto));
}
