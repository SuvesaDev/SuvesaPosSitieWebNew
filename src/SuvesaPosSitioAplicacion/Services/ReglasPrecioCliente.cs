using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Reglas de precio y descuento por cliente. UN solo sitio: Facturación y Proforma las usan igual, para que una
/// proforma ofrezca exactamente lo que Facturación cobraría (y no se dupliquen reglas de negocio).
/// </summary>
public static class ReglasPrecioCliente
{
    /// <summary>Tarifa del cliente: 1 = A, 2 = B, 3 = C, 4 = D, mayor a 4 = tarifa nueva por artículo. Sin tarifa, la A.</summary>
    public static int TarifaDelCliente(int tipoPrecio) => tipoPrecio > 0 ? tipoPrecio : 1;

    /// <summary>
    /// Precio de lista en colones para la tarifa dada. Si el precio de la tarifa no está definido (0) cae al A, y el A
    /// es la promoción cuando está activa y vigente hoy. Convierte a colones si el artículo lo indica.
    /// </summary>
    public static decimal PrecioLista(InventarioDTO articulo, int tarifa, decimal? precioTarifaNueva, DateTime hoy)
    {
        var enMonedaCosto = precioTarifaNueva ?? tarifa switch
        {
            2 when articulo.Precio_B > 0 => Formato.AImporte(articulo.Precio_B),
            3 when articulo.Precio_C > 0 => Formato.AImporte(articulo.Precio_C),
            4 when articulo.Precio_D > 0 => Formato.AImporte(articulo.Precio_D),
            _ => articulo.Promo_Activa && articulo.Promo_Inicio <= hoy && articulo.Promo_Finaliza >= hoy
                ? Formato.AImporte(articulo.Precio_Promo)
                : Formato.AImporte(articulo.Precio_A)
        };
        return PrecioVenta.EnColones(articulo, enMonedaCosto);
    }

    /// <summary>Descuento con el que arranca una línea: el configurado al cliente, sin pasar del máximo del artículo.</summary>
    public static decimal DescuentoInicial(double? descuentoEspecialCliente, float maxDescuentoArticulo)
        => Math.Min(Formato.AImporte(descuentoEspecialCliente ?? 0), Formato.AImporte(maxDescuentoArticulo));
}
