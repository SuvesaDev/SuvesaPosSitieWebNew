namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Aritmetica de una linea de documento y de sus totales.
///
/// SEMILLA DEL DOMINIO FISCAL. Portado de BillingTotals.jsx y BillingItems.jsx del
/// sistema actual, que lo calculan en el navegador con coma flotante:
///
///     linea.SubTotal        = Precio_Unit * cantidad
///     linea.Monto_Descuento = SubTotal * (Descuento / 100)
///     linea.Monto_Impuesto  = (SubTotal - Monto_Descuento) * (Impuesto / 100)
///
/// Aqui se opera **siempre en decimal**. El API entrega los importes en double, asi
/// que se convierten en el borde con <see cref="Helpers.Formato.AImporte(double)"/>
/// antes de entrar, y se redondea a dos decimales al cerrar cada importe.
///
/// No se redondea a mitad de camino: redondear el descuento antes de calcular el
/// impuesto cambia el resultado y no es lo que hace el sistema actual.
/// </summary>
public static class CalculoDocumento
{
    /// <summary>Los importes de una linea, ya calculados.</summary>
    public readonly record struct LineaCalculada(
        decimal SubTotal,
        decimal MontoDescuento,
        decimal MontoImpuesto,
        decimal SubtotalGravado,
        decimal Total);

    /// <summary>
    /// Calcula una linea a partir de cantidad, precio unitario y los porcentajes
    /// de descuento e impuesto.
    /// </summary>
    public static LineaCalculada Linea(
        decimal cantidad,
        decimal precioUnitario,
        decimal porcentajeDescuento,
        decimal porcentajeImpuesto)
    {
        var subTotal = cantidad * precioUnitario;
        var montoDescuento = subTotal * (porcentajeDescuento / 100m);

        var gravado = subTotal - montoDescuento;
        var montoImpuesto = gravado * (porcentajeImpuesto / 100m);

        return new LineaCalculada(
            SubTotal: Redondear(subTotal),
            MontoDescuento: Redondear(montoDescuento),
            MontoImpuesto: Redondear(montoImpuesto),
            SubtotalGravado: Redondear(gravado),
            Total: Redondear(gravado + montoImpuesto));
    }

    /// <summary>Los totales del documento. Suma de lineas ya calculadas.</summary>
    public readonly record struct TotalesDocumento(
        decimal SubTotal,
        decimal Descuento,
        decimal Impuesto,
        decimal Total);

    public static TotalesDocumento Totales(IEnumerable<LineaCalculada> lineas)
    {
        decimal subTotal = 0, descuento = 0, impuesto = 0, total = 0;

        foreach (var l in lineas)
        {
            subTotal += l.SubTotal;
            descuento += l.MontoDescuento;
            impuesto += l.MontoImpuesto;
            total += l.Total;
        }

        return new TotalesDocumento(
            Redondear(subTotal),
            Redondear(descuento),
            Redondear(impuesto),
            Redondear(total));
    }

    /// <summary>
    /// Dos decimales, redondeo al alza en el empate. Es lo que hace
    /// <c>toFixed(2)</c> en el sistema actual para los valores positivos que maneja
    /// un documento, y lo que espera Hacienda.
    /// </summary>
    public static decimal Redondear(decimal valor)
        => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
