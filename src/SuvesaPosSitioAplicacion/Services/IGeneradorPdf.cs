namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Genera los PDF del sitio.
///
/// Es una interfaz y no una llamada directa a la libreria por una razon concreta:
/// **la licencia de QuestPDF esta sin confirmar**. Su licencia comunitaria es
/// gratuita solo por debajo de cierto umbral de facturacion anual. Si SUVESA lo
/// supera, hay que cambiar a PDFsharp con MigraDoc, que es MIT sin condiciones.
/// Con esta interfaz de por medio, ese cambio toca una clase y no 20 pantallas.
///
/// Contexto: por la decision 05 no se imprime nada. Los PDF se ven o se descargan,
/// siempre en A4. No hay formatos termicos que reproducir.
/// </summary>
public interface IGeneradorPdf
{
    /// <summary>Reporte tabular sencillo: titulo, encabezados y filas.</summary>
    byte[] Tabla(ReporteTabular reporte);
}

/// <summary>Lo que necesita un reporte tabular para dibujarse.</summary>
public sealed record ReporteTabular(
    string Titulo,
    string? Subtitulo,
    IReadOnlyList<string> Encabezados,
    IReadOnlyList<IReadOnlyList<string>> Filas,
    IReadOnlyList<string>? Totales = null)
{
    /// <summary>Columnas que se alinean a la derecha, por llevar importes.</summary>
    public IReadOnlySet<int> ColumnasNumericas { get; init; } = new HashSet<int>();
}
