namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Consolida en un solo total lo declarado en un arqueo de caja, que llega en
/// dos monedas (colones y dolares).
///
/// Portado de ArqueoCashBodyTotales.jsx del sistema actual:
///
///     Total = Colones + (Dolares * TipoCambioD)
///
/// Un arqueo compara lo declarado por el cajero contra lo que el sistema
/// registro; si los dolares se suman sin convertir (como si $1 valiera ₡1), el
/// total queda mal y el arqueo deja de servir para lo que existe. Extraido a su
/// propia clase, como <see cref="CalculoDocumento"/>, para que quede cubierto
/// por una prueba unitaria en vez de vivir solo dentro de la pantalla.
/// </summary>
public static class CalculoArqueo
{
    /// <summary>
    /// Total consolidado en colones. <paramref name="tipoCambio"/> es el valor
    /// de venta del dolar (mismo que usa el resto de la aplicacion, via
    /// <c>ICajaOperaciones.TipoCambioDolar</c>).
    /// </summary>
    public static decimal Total(decimal colones, decimal dolares, decimal tipoCambio)
        => colones + (dolares * tipoCambio);
}
