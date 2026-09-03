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
/// Alcance: este generador cubre SOLO los reportes tabulares del sitio
/// (/reportes/compras/pdf, cuentas por pagar). La representacion grafica de los
/// documentos (factura, tiquete, nota de credito, recibos, presupuesto,
/// consignacion, inventarios, traslados, toma fisica) la renderiza el API
/// (proyecto SuvesaPos.Impresion) y el sitio la abre por el endpoint local
/// /documentos/{tipo}/{id}/pdf — ver MOTOR_PLANTILLAS_IMPRESION_WEB.md. Ese motor
/// admite A4 y termico 80 mm.
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
