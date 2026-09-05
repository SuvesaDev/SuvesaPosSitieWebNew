namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Reparto de un cobro de venta entre formas de pago (PLAN_TIQUETE_RUTAS_FACTURACION_WEB.md
/// §W4). Función pura y testeable; reemplaza las tres implementaciones inline de la pantalla
/// de Facturación y de Cobrar. La confirmación financiera definitiva es del API.
///
/// Invariante: <c>recibido − vuelto = aplicado</c>; cuando se exige el 100%, <c>aplicado</c>
/// coincide con el total. El vuelto solo puede salir de formas de pago en efectivo: recibir
/// de más en tarjeta/transferencia no genera vuelto ni cobertura extra.
/// </summary>
public static class PreparacionPagoVenta
{
    /// <summary>Una forma de pago y lo que el cajero digitó como recibido.</summary>
    public readonly record struct LineaPago(
        string Codigo,
        decimal Recibido,
        bool EsEfectivo,
        bool RequiereReferencia,
        string? Referencia);

    /// <summary>Reparto ya calculado, con los importes por forma y los avisos.</summary>
    public readonly record struct Reparto(
        decimal Total,
        decimal RecibidoTotal,
        decimal AplicadoTotal,
        decimal Vuelto,
        decimal Faltante,
        IReadOnlyList<AplicacionForma> Formas,
        IReadOnlyList<string> Errores)
    {
        /// <summary>El cobro cubre el 100% del total y no tiene errores.</summary>
        public bool Cubre100 => Errores.Count == 0 && Faltante <= 0.005m;
    }

    public readonly record struct AplicacionForma(string Codigo, decimal Recibido, decimal Aplicado, decimal Vuelto);

    public static Reparto Calcular(decimal total, IEnumerable<LineaPago> lineas)
    {
        total = Redondear(total);
        var activas = lineas.Where(l => l.Recibido > 0m).ToList();

        var recibidoTotal = Redondear(activas.Sum(l => l.Recibido));
        var recibidoNoEfectivo = Redondear(activas.Where(l => !l.EsEfectivo).Sum(l => l.Recibido));
        var recibidoEfectivo = Redondear(recibidoTotal - recibidoNoEfectivo);

        // Lo no-efectivo aplica como máximo hasta el total (no infla la cobertura ni deja vuelto).
        var aplicadoNoEfectivo = Math.Min(recibidoNoEfectivo, total);
        var restanteParaEfectivo = Redondear(total - aplicadoNoEfectivo);
        var aplicadoEfectivo = Math.Min(recibidoEfectivo, Math.Max(0m, restanteParaEfectivo));
        var aplicadoTotal = Redondear(aplicadoNoEfectivo + aplicadoEfectivo);

        var vuelto = Redondear(Math.Max(0m, recibidoEfectivo - Math.Max(0m, restanteParaEfectivo)));
        var faltante = Redondear(Math.Max(0m, total - aplicadoTotal));

        // Reparto por forma: primero las no-efectivo (hasta su recibido, tope total),
        // el resto lo cubre el efectivo; el vuelto se carga a la última forma en efectivo.
        var formas = new List<AplicacionForma>();
        var pendiente = total;
        foreach (var l in activas.Where(l => !l.EsEfectivo))
        {
            var ap = Redondear(Math.Min(l.Recibido, Math.Max(0m, pendiente)));
            pendiente = Redondear(pendiente - ap);
            formas.Add(new(l.Codigo, Redondear(l.Recibido), ap, 0m));
        }
        var efectivos = activas.Where(l => l.EsEfectivo).ToList();
        for (var i = 0; i < efectivos.Count; i++)
        {
            var l = efectivos[i];
            var ap = Redondear(Math.Min(l.Recibido, Math.Max(0m, pendiente)));
            pendiente = Redondear(pendiente - ap);
            var v = i == efectivos.Count - 1 ? Redondear(l.Recibido - ap) : 0m;
            formas.Add(new(l.Codigo, Redondear(l.Recibido), ap, v));
        }

        var errores = new List<string>();
        if (activas.Count == 0)
            errores.Add("Indique al menos una forma de pago.");
        foreach (var l in activas.Where(l => l.RequiereReferencia && string.IsNullOrWhiteSpace(l.Referencia)))
            errores.Add($"La forma de pago {l.Codigo} requiere número de referencia.");
        if (recibidoNoEfectivo - total > 0.005m)
            errores.Add("El monto en tarjeta/transferencia supera el total; no se puede dar vuelto de un medio que no es efectivo.");

        return new Reparto(total, recibidoTotal, aplicadoTotal, vuelto, faltante, formas, errores);
    }

    private static decimal Redondear(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}
