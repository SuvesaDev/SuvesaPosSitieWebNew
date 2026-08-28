using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SuvesaPosSitioAplicacion.Services;

/// <inheritdoc cref="IGeneradorPdf" />
public sealed class GeneradorPdfQuestPdf : IGeneradorPdf
{
    public byte[] Tabla(ReporteTabular reporte)
    {
        return Document.Create(doc =>
        {
            doc.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(1.5f, Unit.Centimetre);
                pagina.DefaultTextStyle(t => t.FontSize(9));

                pagina.Header().Column(c =>
                {
                    c.Item().Text(reporte.Titulo).FontSize(14).SemiBold();

                    if (!string.IsNullOrWhiteSpace(reporte.Subtitulo))
                    {
                        c.Item().Text(reporte.Subtitulo).FontSize(9).FontColor(Colors.Grey.Darken1);
                    }

                    c.Item().PaddingTop(4).Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });

                pagina.Content().PaddingVertical(10).Table(tabla =>
                {
                    tabla.ColumnsDefinition(cols =>
                    {
                        foreach (var _ in reporte.Encabezados)
                        {
                            cols.RelativeColumn();
                        }
                    });

                    tabla.Header(h =>
                    {
                        for (var i = 0; i < reporte.Encabezados.Count; i++)
                        {
                            var celda = h.Cell().BorderBottom(1).PaddingVertical(3);

                            (reporte.ColumnasNumericas.Contains(i)
                                ? celda.AlignRight()
                                : celda.AlignLeft())
                                .Text(reporte.Encabezados[i]).SemiBold();
                        }
                    });

                    foreach (var fila in reporte.Filas)
                    {
                        for (var i = 0; i < fila.Count; i++)
                        {
                            var celda = tabla.Cell().BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten2).PaddingVertical(2);

                            (reporte.ColumnasNumericas.Contains(i)
                                ? celda.AlignRight()
                                : celda.AlignLeft())
                                .Text(fila[i]);
                        }
                    }

                    if (reporte.Totales is not null)
                    {
                        for (var i = 0; i < reporte.Totales.Count; i++)
                        {
                            var celda = tabla.Cell().BorderTop(1).PaddingVertical(3);

                            (reporte.ColumnasNumericas.Contains(i)
                                ? celda.AlignRight()
                                : celda.AlignLeft())
                                .Text(reporte.Totales[i]).SemiBold();
                        }
                    }
                });

                pagina.Footer().AlignCenter().Text(t =>
                {
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
