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

    /// <summary>Estado de cuenta comercial con resumen, antigüedad y saldos abiertos.</summary>
    byte[] EstadoCuenta(EstadoCuentaPdf reporte);
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

/// <summary>Datos ya consolidados para el estado de cuenta que recibe el cliente.</summary>
public sealed record EstadoCuentaPdf(
    string NombreCliente,
    string IdentificacionCliente,
    DateTime FechaCorte,
    decimal LimiteAprobado,
    decimal SaldoAbierto,
    decimal CreditoAFavor,
    decimal Disponible,
    decimal PorVencer,
    decimal Vencido1a30,
    decimal Vencido31a60,
    decimal Vencido61a90,
    decimal Vencido91oMas,
    IReadOnlyList<LineaEstadoCuentaPdf> Detalle,
    string Moneda = "CRC");

/// <summary>Una factura con saldo pendiente dentro de un estado de cuenta.</summary>
public sealed record LineaEstadoCuentaPdf(
    string Factura,
    string? Consecutivo,
    DateTime Fecha,
    DateTime? Vence,
    decimal Original,
    decimal NotasCredito,
    decimal Pagado,
    decimal Saldo,
    string? EstadoMh);
