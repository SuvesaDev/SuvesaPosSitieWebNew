using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace SuvesaPosSitioAplicacion.Services;

/// <inheritdoc cref="IGeneradorPdf" />
public sealed class GeneradorPdfQuestPdf : IGeneradorPdf
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-CR");
    private const string Azul = "#1072A9";
    private const string AzulOscuro = "#0D5B88";
    private const string FondoSuave = "#EEF5F8";

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

    /// <inheritdoc />
    public byte[] EstadoCuenta(EstadoCuentaPdf reporte)
    {
        return Document.Create(doc =>
        {
            doc.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(1.5f, Unit.Centimetre);
                pagina.DefaultTextStyle(t => t.FontFamily("Lato").FontSize(9).FontColor("#1F2933"));

                pagina.Header().Element(c => EncabezadoEstadoCuenta(c, reporte));
                pagina.Content().PaddingTop(12).Column(col =>
                {
                    col.Item().Element(c => IndicadoresEstadoCuenta(c, reporte));
                    col.Item().PaddingTop(10).Element(c => AntiguedadEstadoCuenta(c, reporte));
                    col.Item().PaddingTop(12).Element(c => TablaEstadoCuenta(c, reporte));
                });
                pagina.Footer().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text($"Estado de cuenta generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(7).FontColor(Colors.Grey.Medium);
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.Span("Página ").FontSize(7).FontColor(Colors.Grey.Medium);
                        t.CurrentPageNumber();
                        t.Span(" de ");
                        t.TotalPages();
                    });
                });
            });
        }).GeneratePdf();
    }

    private static void EncabezadoEstadoCuenta(IContainer container, EstadoCuentaPdf reporte)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(cliente =>
                {
                    cliente.Item().Text("ESTADO DE CUENTA").SemiBold().FontSize(16).FontColor(Azul);
                    cliente.Item().PaddingTop(2).Text("Resumen de facturas y saldos pendientes")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });
                row.ConstantItem(165).BorderLeft(1.25f).BorderColor(Azul).PaddingLeft(8).Column(corte =>
                {
                    corte.Item().Text("FECHA DE CORTE").SemiBold().FontSize(7).FontColor(Colors.Grey.Darken1);
                    corte.Item().PaddingTop(1).Text(reporte.FechaCorte.ToString("dd/MM/yyyy"))
                        .SemiBold().FontSize(11);
                });
            });

            col.Item().PaddingTop(9).Background(FondoSuave).Padding(7).Column(cliente =>
            {
                cliente.Item().Text("CLIENTE").SemiBold().FontSize(7.5f).FontColor(Azul);
                cliente.Item().PaddingTop(1).Text(reporte.NombreCliente).SemiBold().FontSize(11);
                cliente.Item().PaddingTop(1).Text($"Identificación: {reporte.IdentificacionCliente}")
                    .FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            col.Item().PaddingTop(7).LineHorizontal(1).LineColor(Azul);
        });
    }

    private static void IndicadoresEstadoCuenta(IContainer container, EstadoCuentaPdf reporte)
    {
        var datos = new[]
        {
            ("Límite aprobado", reporte.LimiteAprobado, "#FFFFFF", Azul),
            ("Saldo abierto", reporte.SaldoAbierto, FondoSuave, AzulOscuro),
            ("Crédito a favor", reporte.CreditoAFavor, "#FFFFFF", Azul),
            ("Disponible", reporte.Disponible, "#EAF5EE", "#26734D"),
        };

        container.Row(row =>
        {
            foreach (var (etiqueta, valor, fondo, color) in datos)
            {
                row.RelativeItem().Background(fondo).Border(0.5f).BorderColor("#D4E2E8").Padding(6).Column(c =>
                {
                    c.Item().Text(etiqueta).SemiBold().FontSize(7).FontColor(Colors.Grey.Darken1);
                    c.Item().PaddingTop(2).Text(FormatearMonto(valor, reporte.Moneda))
                        .SemiBold().FontSize(11).FontColor(color);
                });
            }
        });
    }

    private static void AntiguedadEstadoCuenta(IContainer container, EstadoCuentaPdf reporte)
    {
        var tramos = new[]
        {
            ("POR VENCER", reporte.PorVencer, Azul),
            ("1 - 30 DÍAS", reporte.Vencido1a30, Azul),
            ("31 - 60 DÍAS", reporte.Vencido31a60, Azul),
            ("61 - 90 DÍAS", reporte.Vencido61a90, Azul),
            ("91 DÍAS O MÁS", reporte.Vencido91oMas, reporte.Vencido91oMas > 0 ? "#B42318" : Azul),
        };

        container.Column(col =>
        {
            col.Item().Text("ANTIGÜEDAD DE SALDOS").SemiBold().FontSize(8.5f).FontColor(Azul);
            col.Item().PaddingTop(4).Row(row =>
            {
                foreach (var (etiqueta, valor, color) in tramos)
                {
                    row.RelativeItem().Border(0.5f).BorderColor("#D4E2E8").Padding(5).Column(c =>
                    {
                        c.Item().Text(etiqueta).SemiBold().FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingTop(2).Text(FormatearMonto(valor, reporte.Moneda))
                            .SemiBold().FontSize(9).FontColor(color);
                    });
                }
            });
        });
    }

    private static void TablaEstadoCuenta(IContainer container, EstadoCuentaPdf reporte)
    {
        container.Column(col =>
        {
            col.Item().Text("FACTURAS CON SALDO PENDIENTE").SemiBold().FontSize(8.5f).FontColor(Azul);
            col.Item().PaddingTop(4).Table(tabla =>
            {
                tabla.ColumnsDefinition(def =>
                {
                    def.RelativeColumn(1.25f);
                    def.RelativeColumn(1.1f);
                    def.RelativeColumn(1.1f);
                    def.RelativeColumn(1.25f);
                    def.RelativeColumn(1.0f);
                    def.RelativeColumn(1.1f);
                    def.RelativeColumn(1.25f);
                    def.RelativeColumn(0.95f);
                });

                tabla.Header(h =>
                {
                    CabeceraEstadoCuenta(h.Cell(), "FACTURA", "izquierda");
                    CabeceraEstadoCuenta(h.Cell(), "FECHA", "centro");
                    CabeceraEstadoCuenta(h.Cell(), "VENCE", "centro");
                    CabeceraEstadoCuenta(h.Cell(), "ORIGINAL", "derecha");
                    CabeceraEstadoCuenta(h.Cell(), "N/C", "derecha");
                    CabeceraEstadoCuenta(h.Cell(), "PAGADO", "derecha");
                    CabeceraEstadoCuenta(h.Cell(), "SALDO", "derecha");
                    CabeceraEstadoCuenta(h.Cell(), "ESTADO", "centro");
                });

                if (reporte.Detalle.Count == 0)
                {
                    tabla.Cell().ColumnSpan(8).Background("#FFFFFF").BorderBottom(0.5f).BorderColor("#D4E2E8")
                        .Padding(8).AlignCenter().Text("No hay facturas con saldo pendiente.")
                        .FontColor(Colors.Grey.Darken1);
                }
                else
                {
                    var indice = 0;
                    foreach (var linea in reporte.Detalle.OrderBy(l => l.Vence ?? l.Fecha))
                    {
                        var fondo = indice++ % 2 == 0 ? "#FFFFFF" : FondoSuave;
                        // El consecutivo fiscal completo se conserva en el CSV y
                        // consulta de pantalla. En un estado de cuenta ocuparía
                        // varias líneas y ocultaría el importe que se debe cobrar.
                        CeldaEstadoCuenta(tabla.Cell(), fondo, "izquierda").Text(linea.Factura).SemiBold();
                        CeldaEstadoCuenta(tabla.Cell(), fondo, "centro").Text(linea.Fecha.ToString("dd/MM/yyyy"));
                        CeldaEstadoCuenta(tabla.Cell(), fondo, "centro").Text(linea.Vence?.ToString("dd/MM/yyyy") ?? "—");
                        CeldaEstadoCuenta(tabla.Cell(), fondo, "derecha").Text(FormatearMonto(linea.Original, reporte.Moneda));
                        CeldaEstadoCuenta(tabla.Cell(), fondo, "derecha").Text(FormatearMonto(linea.NotasCredito, reporte.Moneda));
                        CeldaEstadoCuenta(tabla.Cell(), fondo, "derecha").Text(FormatearMonto(linea.Pagado, reporte.Moneda));
                        CeldaEstadoCuenta(tabla.Cell(), fondo, "derecha").Text(FormatearMonto(linea.Saldo, reporte.Moneda)).SemiBold();
                        CeldaEstadoCuenta(tabla.Cell(), fondo, "centro").Text(linea.EstadoMh ?? "—").FontSize(6.5f);
                    }
                }

            });

            // El total no forma parte de una columna estrecha: se presenta en
            // una banda propia para que un importe grande nunca se parta.
            col.Item().Background(FondoSuave).BorderTop(1).BorderColor(Azul).Padding(6).Row(row =>
            {
                row.RelativeItem().AlignRight().Text("SALDO TOTAL").SemiBold().FontColor(AzulOscuro);
                row.ConstantItem(130).AlignRight().Text(FormatearMonto(reporte.SaldoAbierto, reporte.Moneda))
                    .SemiBold().FontSize(11).FontColor(AzulOscuro);
            });
        });
    }

    private static void CabeceraEstadoCuenta(IContainer celda, string etiqueta, string alineacion) =>
        Alinear(celda.Background(Azul).PaddingVertical(4).PaddingHorizontal(3), alineacion)
            .Text(etiqueta).SemiBold().FontSize(7).FontColor(Colors.White);

    private static IContainer CeldaEstadoCuenta(IContainer celda, string fondo, string alineacion) =>
        Alinear(celda.Background(fondo).BorderBottom(0.5f).BorderColor("#D4E2E8")
            .PaddingVertical(4).PaddingHorizontal(3), alineacion);

    private static IContainer Alinear(IContainer celda, string alineacion) => alineacion switch
    {
        "derecha" => celda.AlignRight(),
        "centro" => celda.AlignCenter(),
        _ => celda.AlignLeft(),
    };

    private static string FormatearMonto(decimal monto, string moneda)
    {
        var simbolo = moneda.Equals("USD", StringComparison.OrdinalIgnoreCase) ? "$" : "₡";
        return simbolo + monto.ToString("#,##0.00", Cultura);
    }
}
