namespace SuvesaPosSitioAplicacion.DTOs.Cobros;

/// <summary>
/// Serie operativa (no fiscal): recibos de cobro/pago, preventa, traslados,
/// consignación, toma física, devolución interna. SANEAMIENTO Fase 7 —
/// espejo de <c>api/series-operativas</c>.
/// </summary>
public sealed class SerieOperativaWebDTO
{
    public int Id { get; set; }
    public int Tipo { get; set; }
    public string TipoNombre { get; set; } = "";
    public int IdEmisor { get; set; }
    public int IdSucursal { get; set; }
    public int? NumeroTerminal { get; set; }
    public string? Prefijo { get; set; }
    public long UltimoConsecutivo { get; set; }
    public bool Activa { get; set; }
    public bool EsPredeterminada { get; set; }
}

/// <summary>Catálogo de tipos de serie operativa (enum <c>TipoSerieOperativa</c> del API).</summary>
public static class TiposSerieOperativa
{
    public static readonly (int Valor, string Nombre)[] Todos =
    {
        (1, "Recibo de cobro"),
        (2, "Recibo de pago"),
        (3, "Preventa"),
        (4, "Compra interna"),
        (5, "Traslado entre bodegas"),
        (6, "Consignación — ingreso"),
        (7, "Consignación — salida"),
        (8, "Toma física"),
        (9, "Devolución interna"),
    };

    public static string Nombre(int valor)
    {
        foreach (var t in Todos)
            if (t.Valor == valor) return t.Nombre;
        return $"Tipo {valor}";
    }
}
