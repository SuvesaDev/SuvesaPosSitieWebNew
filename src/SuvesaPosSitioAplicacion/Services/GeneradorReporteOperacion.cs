using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Helpers;
using System.Globalization;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>Exportaciones con la misma jerarquía visual y gráficas de la consulta web.</summary>
public sealed class GeneradorReporteOperacion : IGeneradorReporteOperacion
{
    private const string Verde = "#287A1A";
    private const string Azul = "#1072A9";
    private const string Fondo = "#F3F6F1";
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-CR");

    public byte[] Pdf(string tipoReporte, ReporteOperacionWebDTO reporte, DateTime generadoEnEquipo)
    {
        var graficas = ResumenGraficasReporteOperacion.Crear(tipoReporte, reporte.Filas);
        return Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(PageSizes.A4.Landscape());
            pagina.Margin(1.2f, Unit.Centimetre);
            pagina.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(7.5f).FontColor("#263238"));
            pagina.Header().Element(c => EncabezadoPdf(c, reporte, generadoEnEquipo));
            pagina.Content().PaddingTop(8).Column(columna =>
            {
                columna.Item().Element(c => IndicadoresPdf(c, reporte));
                columna.Item().PaddingTop(9).Row(fila =>
                {
                    fila.RelativeItem().Element(c => GraficoPdf(c, graficas.TituloEvolucion, graficas.Evolucion, graficas.EsCantidad));
                    fila.ConstantItem(10);
                    fila.RelativeItem().Element(c => GraficoPdf(c, graficas.TituloEstados, graficas.Estados, graficas.EsCantidad));
                });
                columna.Item().PaddingTop(10).Element(c => TablaPdf(c, reporte, graficas.EsCantidad));
            });
            pagina.Footer().PaddingTop(6).AlignCenter().Text(t =>
            {
                t.Span("Página ").FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
            });
        })).GeneratePdf();
    }

    public byte[] Excel(string tipoReporte, ReporteOperacionWebDTO reporte, DateTime generadoEnEquipo)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Reporte");
        var graficas = ResumenGraficasReporteOperacion.Crear(tipoReporte, reporte.Filas);

        hoja.Range(1, 1, 1, 8).Merge();
        hoja.Cell(1, 1).Value = reporte.Titulo;
        hoja.Cell(1, 1).Style.Font.Bold = true;
        hoja.Cell(1, 1).Style.Font.FontSize = 16;
        hoja.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(Verde);
        hoja.Cell(2, 1).Value = $"Período: {reporte.Desde?.ToString("dd/MM/yyyy") ?? "sin límite"} al {reporte.Hasta?.ToString("dd/MM/yyyy") ?? "sin límite"}";
        hoja.Cell(3, 1).Value = $"Generado en el equipo: {generadoEnEquipo:dd/MM/yyyy HH:mm}";
        hoja.Cell(3, 1).Style.Font.FontColor = XLColor.Gray;

        var filaIndicadores = 5;
        for (var i = 0; i < reporte.Indicadores.Count; i++)
        {
            var columna = 1 + (i % 4) * 2;
            var fila = filaIndicadores + (i / 4) * 2;
            var etiqueta = hoja.Range(fila, columna, fila, columna + 1);
            etiqueta.Merge();
            etiqueta.Value = reporte.Indicadores[i].Etiqueta;
            etiqueta.Style.Fill.BackgroundColor = XLColor.FromHtml(Fondo);
            etiqueta.Style.Font.Bold = true;
            etiqueta.Style.Font.FontColor = XLColor.FromHtml(Verde);
            var valor = hoja.Range(fila + 1, columna, fila + 1, columna + 1);
            valor.Merge();
            valor.Value = reporte.Indicadores[i].Valor;
            valor.Style.Font.Bold = true;
            valor.Style.Font.FontSize = 13;
            valor.Style.NumberFormat.Format = reporte.Indicadores[i].Formato == "cantidad" ? "#,##0.00" : "₡#,##0.00";
        }

        var cabecera = filaIndicadores + ((reporte.Indicadores.Count + 3) / 4) * 2 + 2;
        var titulos = new[] { "Fecha", "Referencia", "Entidad", "Detalle", "Cantidad", "Monto", "Saldo / existencia", "Estado" };
        for (var i = 0; i < titulos.Length; i++) hoja.Cell(cabecera, i + 1).Value = titulos[i];

        for (var i = 0; i < reporte.Filas.Count; i++)
        {
            var fila = reporte.Filas[i];
            var destino = cabecera + 1 + i;
            hoja.Cell(destino, 1).Value = fila.Fecha;
            hoja.Cell(destino, 2).Value = fila.Referencia;
            hoja.Cell(destino, 3).Value = fila.Entidad;
            hoja.Cell(destino, 4).Value = fila.Descripcion;
            hoja.Cell(destino, 5).Value = fila.Cantidad;
            hoja.Cell(destino, 6).Value = fila.Monto;
            hoja.Cell(destino, 7).Value = fila.Saldo;
            hoja.Cell(destino, 8).Value = fila.Estado;
        }

        if (reporte.Filas.Count > 0)
        {
            var tabla = hoja.Range(cabecera, 1, cabecera + reporte.Filas.Count, 8).CreateTable("DetalleReporteOperacion");
            tabla.Theme = XLTableTheme.TableStyleMedium2;
        }

        hoja.Column(1).Style.NumberFormat.Format = "dd/MM/yyyy HH:mm";
        hoja.Columns(5, 7).Style.NumberFormat.Format = graficas.EsCantidad ? "#,##0.00" : "₡#,##0.00";
        hoja.SheetView.FreezeRows(cabecera);
        hoja.Range(1, 1, Math.Max(cabecera + reporte.Filas.Count, 3), 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        AjustarColumnas(hoja, 8);

        var visual = libro.Worksheets.Add("Gráficas");
        visual.Cell(1, 1).Value = reporte.Titulo;
        visual.Cell(1, 1).Style.Font.Bold = true;
        visual.Cell(1, 1).Style.Font.FontSize = 16;
        visual.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(Verde);
        visual.Cell(2, 1).Value = "Las barras son gráficas nativas de Excel y se basan en las mismas filas del reporte.";
        visual.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
        AgregarGraficaExcel(visual, 4, graficas.TituloEvolucion, graficas.Evolucion, graficas.EsCantidad, Verde);
        AgregarGraficaExcel(visual, 16, graficas.TituloEstados, graficas.Estados, graficas.EsCantidad, Azul);
        AjustarColumnas(visual, 3);

        using var stream = new MemoryStream();
        libro.SaveAs(stream);
        return stream.ToArray();
    }

    private static void EncabezadoPdf(IContainer container, ReporteOperacionWebDTO reporte, DateTime generadoEnEquipo) => container.Column(col =>
    {
        col.Item().Text(reporte.Titulo).FontSize(16).SemiBold().FontColor(Verde);
        col.Item().PaddingTop(2).Text($"Período: {reporte.Desde?.ToString("dd/MM/yyyy") ?? "sin límite"} al {reporte.Hasta?.ToString("dd/MM/yyyy") ?? "sin límite"}")
            .FontSize(8).FontColor(Colors.Grey.Darken1);
        col.Item().Text($"Generado en el equipo: {generadoEnEquipo:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Medium);
        col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Verde);
    });

    private static void IndicadoresPdf(IContainer container, ReporteOperacionWebDTO reporte) => container.Table(tabla =>
    {
        tabla.ColumnsDefinition(columnas =>
        {
            foreach (var _ in reporte.Indicadores) columnas.RelativeColumn();
        });
        foreach (var indicador in reporte.Indicadores)
        {
            tabla.Cell().Background(Fondo).Border(0.5f).BorderColor("#D9E3D4").Padding(5).Column(col =>
            {
                col.Item().Text(indicador.Etiqueta).FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(1).Text(Formatear(indicador.Valor, indicador.Formato == "cantidad")).SemiBold().FontSize(10).FontColor(Verde);
            });
        }
    });

    private static void GraficoPdf(IContainer container, string titulo, IReadOnlyList<PuntoGraficoOperacion> puntos, bool esCantidad) => container.Border(0.5f).BorderColor("#D9E3D4").Padding(6).Column(col =>
    {
        col.Item().Text(titulo).SemiBold().FontSize(8).FontColor(Verde);
        if (puntos.Count == 0)
        {
            col.Item().PaddingTop(5).Text("No hay datos para graficar.").FontColor(Colors.Grey.Medium);
            return;
        }
        var maximo = puntos.Max(x => Math.Abs(x.Valor));
        foreach (var punto in puntos)
        {
            var proporcion = maximo == 0 ? 0f : (float)(Math.Abs(punto.Valor) / maximo);
            col.Item().PaddingTop(3).Row(fila =>
            {
                fila.ConstantItem(72).Text(Abreviar(punto.Etiqueta, 15)).FontSize(6.5f);
                fila.RelativeItem().Height(11).Row(barra =>
                {
                    if (proporcion > 0) barra.RelativeItem(Math.Max(.02f, proporcion)).Background(punto.Valor < 0 ? "#B42318" : Verde);
                    if (proporcion < 1) barra.RelativeItem(Math.Max(.02f, 1 - proporcion)).Background("#E7EFE3");
                });
                fila.ConstantItem(62).AlignRight().Text(Formatear(punto.Valor, esCantidad)).FontSize(6.5f);
            });
        }
    });

    private static void TablaPdf(IContainer container, ReporteOperacionWebDTO reporte, bool esCantidad) => container.Column(col =>
    {
        col.Item().Text("Detalle").SemiBold().FontSize(9).FontColor(Verde);
        col.Item().PaddingTop(3).Table(tabla =>
        {
            tabla.ColumnsDefinition(columnas =>
            {
                columnas.RelativeColumn(1.15f);
                columnas.RelativeColumn(1.1f);
                columnas.RelativeColumn(1.2f);
                columnas.RelativeColumn(1.55f);
                columnas.RelativeColumn(.75f);
                columnas.RelativeColumn(.95f);
                columnas.RelativeColumn(1.1f);
                columnas.RelativeColumn(1f);
            });
            var titulos = new[] { "Fecha", "Referencia", "Entidad", "Detalle", "Cantidad", "Monto", "Saldo", "Estado" };
            tabla.Header(cabecera =>
            {
                foreach (var titulo in titulos) cabecera.Cell().Background(Fondo).Padding(3).Text(titulo).SemiBold().FontSize(6.3f);
            });
            foreach (var fila in reporte.Filas)
            {
                CeldaPdf(tabla.Cell(), fila.Fecha.ToString("dd/MM/yy HH:mm"));
                CeldaPdf(tabla.Cell(), fila.Referencia);
                CeldaPdf(tabla.Cell(), fila.Entidad);
                CeldaPdf(tabla.Cell(), fila.Descripcion);
                CeldaPdf(tabla.Cell().AlignRight(), fila.Cantidad.ToString("N2", Cultura));
                CeldaPdf(tabla.Cell().AlignRight(), Formato.Importe(fila.Monto));
                CeldaPdf(tabla.Cell().AlignRight(), esCantidad ? fila.Saldo.ToString("N2", Cultura) : Formato.Importe(fila.Saldo));
                CeldaPdf(tabla.Cell(), fila.Estado);
            }
        });
    });

    private static void CeldaPdf(IContainer container, string texto) => container.BorderBottom(.25f).BorderColor("#E5E7EB").PaddingVertical(2).PaddingHorizontal(1).Text(texto).FontSize(6.1f);

    private static void AgregarGraficaExcel(IXLWorksheet hoja, int filaInicial, string titulo, IReadOnlyList<PuntoGraficoOperacion> puntos, bool esCantidad, string color)
    {
        hoja.Cell(filaInicial, 1).Value = titulo;
        hoja.Cell(filaInicial, 1).Style.Font.Bold = true;
        hoja.Cell(filaInicial, 1).Style.Font.FontColor = XLColor.FromHtml(Verde);
        hoja.Cell(filaInicial + 1, 1).Value = "Grupo";
        hoja.Cell(filaInicial + 1, 2).Value = esCantidad ? "Cantidad" : "Importe";
        hoja.Range(filaInicial + 1, 1, filaInicial + 1, 2).Style.Fill.BackgroundColor = XLColor.FromHtml(Fondo);
        hoja.Range(filaInicial + 1, 1, filaInicial + 1, 2).Style.Font.Bold = true;
        for (var i = 0; i < puntos.Count; i++)
        {
            hoja.Cell(filaInicial + 2 + i, 1).Value = puntos[i].Etiqueta;
            hoja.Cell(filaInicial + 2 + i, 2).Value = puntos[i].Valor;
        }
        if (puntos.Count == 0) return;
        var valores = hoja.Range(filaInicial + 2, 2, filaInicial + 1 + puntos.Count, 2);
        valores.Style.NumberFormat.Format = esCantidad ? "#,##0.00" : "₡#,##0.00";
        valores.AddConditionalFormat().DataBar(XLColor.FromHtml(color)).LowestValue().HighestValue();
    }

    private static void AjustarColumnas(IXLWorksheet hoja, int hastaColumna)
    {
        for (var columna = 1; columna <= hastaColumna; columna++)
        {
            hoja.Column(columna).AdjustToContents();
            if (hoja.Column(columna).Width > 36) hoja.Column(columna).Width = 36;
        }
    }

    private static string Formatear(decimal valor, bool esCantidad) => esCantidad ? valor.ToString("N2", Cultura) : Formato.Importe(valor);
    private static string Abreviar(string texto, int maximo) => texto.Length <= maximo ? texto : $"{texto[..Math.Max(1, maximo - 1)]}…";
}
