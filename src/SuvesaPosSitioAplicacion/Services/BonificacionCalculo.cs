namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Resuelve un grupo de bonificación al facturar (docs/BONIFICACION_DISENO_WEB.md
/// §4.3.d). El API §3.4 quedó como "shape builder"; la regla la aplica el sitio:
///
///   - la suma de cantidades usadas no puede exceder <c>CantidadVenta + CantidadBonificable</c>
///     ("compra 10 lleva 1" factura 11 unidades: 10 pagadas + 1 gratis, no 10);
///   - el artículo que sale gratis es el de <b>menor precio</b> entre los usados;
///   - se regalan hasta <c>CantidadBonificable</c> unidades de ese artículo, a
///     precio 0 pero con el impuesto real (<see cref="CalculoDocumento.LineaBonificada"/>).
/// </summary>
public static class BonificacionCalculo
{
    /// <summary>Un artículo efectivamente usado para completar la cantidad del tipo (incluye el principal).</summary>
    public sealed record ArticuloUsado(
        long Codigo,
        string Descripcion,
        decimal PrecioUnit,
        decimal PorcentajeImpuesto,
        int Cantidad);

    /// <summary>Una línea del grupo ya resuelta (pagada o de regalo).</summary>
    public sealed record LineaGrupo(
        long Codigo,
        string Descripcion,
        int Cantidad,
        decimal PrecioUnit,
        decimal PorcentajeImpuesto,
        bool EsBonificacion,
        CalculoDocumento.LineaCalculada Calculo);

    public sealed record Resultado(bool Ok, string? Error, IReadOnlyList<LineaGrupo> Lineas)
    {
        public static Resultado Falla(string error) => new(false, error, Array.Empty<LineaGrupo>());
        public static Resultado Correcto(IReadOnlyList<LineaGrupo> lineas) => new(true, null, lineas);
    }

    public static Resultado ResolverGrupo(int cantidadVenta, int cantidadBonificable, IReadOnlyList<ArticuloUsado> usados)
    {
        if (usados is null || usados.Count == 0)
            return Resultado.Falla("Indique al menos un artículo para la bonificación.");
        if (usados.Any(u => u.Cantidad <= 0))
            return Resultado.Falla("Todas las cantidades de la mezcla deben ser mayores que cero.");

        var totalUsado = usados.Sum(u => u.Cantidad);
        var totalGrupo = cantidadVenta + cantidadBonificable;
        if (totalUsado > totalGrupo)
            return Resultado.Falla($"La suma de cantidades ({totalUsado}) excede la cantidad de la configuración ({totalGrupo}).");
        if (cantidadBonificable <= 0)
            return Resultado.Falla("La configuración no tiene cantidad bonificable.");

        // Artículo de regalo = el de menor precio (desempate estable por código).
        var regalo = usados.OrderBy(u => u.PrecioUnit).ThenBy(u => u.Codigo).First();
        var cantidadGratis = Math.Min(cantidadBonificable, regalo.Cantidad);

        var lineas = new List<LineaGrupo>();

        foreach (var u in usados)
        {
            var esElRegalo = u.Codigo == regalo.Codigo;
            var cantidadPagada = esElRegalo ? u.Cantidad - cantidadGratis : u.Cantidad;

            if (cantidadPagada > 0)
            {
                lineas.Add(new LineaGrupo(
                    u.Codigo, u.Descripcion, cantidadPagada, u.PrecioUnit, u.PorcentajeImpuesto,
                    EsBonificacion: false,
                    Calculo: CalculoDocumento.Linea(cantidadPagada, u.PrecioUnit, 0m, u.PorcentajeImpuesto)));
            }

            if (esElRegalo && cantidadGratis > 0)
            {
                // PrecioUnit conserva el precio de lista (base del impuesto que se
                // reporta a Hacienda, aunque lo asume el emisor); que la línea se
                // cobre en 0 lo marca EsBonificacion.
                lineas.Add(new LineaGrupo(
                    u.Codigo, u.Descripcion, cantidadGratis, u.PrecioUnit, u.PorcentajeImpuesto,
                    EsBonificacion: true,
                    Calculo: CalculoDocumento.LineaBonificada(cantidadGratis, u.PrecioUnit, u.PorcentajeImpuesto)));
            }
        }

        return Resultado.Correcto(lineas);
    }
}
