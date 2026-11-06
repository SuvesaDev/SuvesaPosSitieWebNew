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
    /// <summary>Máximo de líneas (depósitos o cheques) que una misma forma de pago admite en un cobro.</summary>
    public const int MaxLineasPorForma = 5;

    /// <summary>Una forma de pago y lo que el cajero digitó como recibido.</summary>
    public readonly record struct LineaPago(
        string Codigo,
        decimal Recibido,
        bool EsEfectivo,
        bool RequiereReferencia,
        string? Referencia,
        bool RequiereBanco = false,
        int? IdBanco = null);

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
        errores.AddRange(ValidarInstrumentos(activas));
        if (recibidoNoEfectivo - total > 0.005m)
            errores.Add("El monto en tarjeta/transferencia supera el total; no se puede dar vuelto de un medio que no es efectivo.");

        return new Reparto(total, recibidoTotal, aplicadoTotal, vuelto, faltante, formas, errores);
    }

    /// <summary>
    /// Validación de los instrumentos (tarjeta, depósito, cheque) de un cobro: referencia y banco por línea
    /// (el aviso dice cuál línea falla cuando hay varias de la misma forma), duplicados de banco + referencia y
    /// el tope de <see cref="MaxLineasPorForma"/> líneas por forma. Las usa también la pantalla Cobrar.
    /// </summary>
    public static List<string> ValidarInstrumentos(IReadOnlyList<LineaPago> activas)
    {
        var errores = new List<string>();
        // Con varias líneas de la misma forma (varios depósitos/cheques) el aviso dice cuál es.
        string Nombre(int i)
        {
            var codigo = activas[i].Codigo;
            if (activas.Count(x => x.Codigo == codigo) < 2) return codigo;
            return $"{codigo} (línea {activas.Take(i + 1).Count(x => x.Codigo == codigo)})";
        }
        for (var i = 0; i < activas.Count; i++)
        {
            var l = activas[i];
            if (l.RequiereReferencia && string.IsNullOrWhiteSpace(l.Referencia))
                errores.Add($"La forma de pago {Nombre(i)} requiere número de referencia.");
            if (l.RequiereBanco && l.IdBanco is null)
                errores.Add($"La forma de pago {Nombre(i)} requiere seleccionar el banco.");
        }
        foreach (var g in activas
                     .Where(l => l.IdBanco is not null && !string.IsNullOrWhiteSpace(l.Referencia))
                     .GroupBy(l => (l.IdBanco, Ref: l.Referencia!.Trim().ToUpperInvariant()))
                     .Where(g => g.Count() > 1))
            errores.Add($"Hay {g.Count()} pagos con el mismo banco y la misma referencia ({g.First().Referencia!.Trim()}): un depósito o cheque no puede registrarse dos veces.");
        foreach (var g in activas.GroupBy(l => l.Codigo).Where(g => g.Count() > MaxLineasPorForma))
            errores.Add($"La forma de pago {g.Key} admite como máximo {MaxLineasPorForma} líneas.");
        return errores;
    }

    private static decimal Redondear(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}
