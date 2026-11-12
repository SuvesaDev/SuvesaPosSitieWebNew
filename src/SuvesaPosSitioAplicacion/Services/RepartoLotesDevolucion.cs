namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Una línea de bonificación se devuelve completa y con los lotes con que se vendió. Si la línea salió de varios lotes,
/// se parte en una línea por lote (cantidad propia) y sus importes se reparten en proporción, sin perder ni ganar un
/// centavo: la última parte toma el resto.
/// </summary>
public static class RepartoLotesDevolucion
{
    public sealed record Parte(long? IdStockLote, decimal Cantidad, CalculoDocumento.LineaCalculada Calculo);

    /// <param name="cantidad">Cantidad de la línea en la factura.</param>
    /// <param name="calculo">Importes de la línea completa.</param>
    /// <param name="lotes">Lotes con que se vendió (id y cantidad). Vacío si el artículo no maneja lote.</param>
    public static IReadOnlyList<Parte> Partir(decimal cantidad, CalculoDocumento.LineaCalculada calculo, IReadOnlyList<(long IdLote, decimal Cantidad)> lotes)
    {
        var validos = lotes.Where(l => l.IdLote > 0 && l.Cantidad > 0).ToList();
        if (validos.Count == 0) return new[] { new Parte(null, cantidad, calculo) };
        if (validos.Count == 1) return new[] { new Parte(validos[0].IdLote, cantidad, calculo) };

        var totalLotes = validos.Sum(l => l.Cantidad);
        var partes = new List<Parte>(validos.Count);
        decimal subTotal = 0, descuento = 0, impuesto = 0, gravado = 0, total = 0;
        for (var i = 0; i < validos.Count; i++)
        {
            var (id, cant) = validos[i];
            CalculoDocumento.LineaCalculada c;
            if (i == validos.Count - 1)
                c = new(calculo.SubTotal - subTotal, calculo.MontoDescuento - descuento, calculo.MontoImpuesto - impuesto,
                        calculo.SubtotalGravado - gravado, calculo.Total - total);
            else
            {
                var f = cant / totalLotes;
                c = new(R(calculo.SubTotal * f), R(calculo.MontoDescuento * f), R(calculo.MontoImpuesto * f),
                        R(calculo.SubtotalGravado * f), R(calculo.Total * f));
                subTotal += c.SubTotal; descuento += c.MontoDescuento; impuesto += c.MontoImpuesto;
                gravado += c.SubtotalGravado; total += c.Total;
            }
            partes.Add(new Parte(id, cant, c));
        }
        return partes;
    }

    private static decimal R(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}
