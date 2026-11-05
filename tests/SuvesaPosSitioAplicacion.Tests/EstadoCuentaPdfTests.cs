using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

public sealed class EstadoCuentaPdfTests
{
    static EstadoCuentaPdfTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public void EstadoCuenta_ConSaldosYAntiguedad_GeneraPdfValido()
    {
        var reporte = new EstadoCuentaPdf(
            NombreCliente: "Almacén La Canasta",
            IdentificacionCliente: "3101123456",
            FechaCorte: new DateTime(2026, 9, 5),
            LimiteAprobado: 100000m,
            SaldoAbierto: 24253.19m,
            CreditoAFavor: 0m,
            Disponible: 75746.81m,
            PorVencer: 24253.19m,
            Vencido1a30: 0m,
            Vencido31a60: 0m,
            Vencido61a90: 0m,
            Vencido91oMas: 0m,
            Detalle:
            [
                new LineaEstadoCuentaPdf(
                    Factura: "1",
                    Consecutivo: "00100150010000000001",
                    Fecha: new DateTime(2026, 9, 5),
                    Vence: new DateTime(2026, 10, 5),
                    Original: 24253.19m,
                    NotasCredito: 0m,
                    Pagado: 0m,
                    Saldo: 24253.19m,
                    EstadoMh: "Aceptado")
            ]);

        var pdf = new GeneradorPdfQuestPdf().EstadoCuenta(reporte);

        Assert.True(pdf.Length > 800);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }

    /// <summary>
    /// El estado de cuenta resume varias facturas, no es "de" una sola — no tiene un
    /// creador único por línea. Lo que se pide es dejar constancia de quién generó ESTE
    /// PDF (quien lo exportó), en el pie de página, igual que las demás plantillas dejan
    /// constancia de quién creó el documento.
    /// </summary>
    [Fact]
    public void EstadoCuenta_ConGeneradoPor_GeneraPdfValido()
    {
        var reporte = new EstadoCuentaPdf(
            NombreCliente: "Almacén La Canasta",
            IdentificacionCliente: "3101123456",
            FechaCorte: new DateTime(2026, 9, 5),
            LimiteAprobado: 100000m,
            SaldoAbierto: 0m,
            CreditoAFavor: 0m,
            Disponible: 100000m,
            PorVencer: 0m,
            Vencido1a30: 0m,
            Vencido31a60: 0m,
            Vencido61a90: 0m,
            Vencido91oMas: 0m,
            Detalle: [],
            GeneradoPor: "Maria Jose Fonseca");

        var pdf = new GeneradorPdfQuestPdf().EstadoCuenta(reporte);

        Assert.True(pdf.Length > 800);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public void EstadoCuenta_ConMembreteYDiasMora_GeneraPdfValido()
    {
        var reporte = new EstadoCuentaPdf(
            NombreCliente: "Almacén La Canasta",
            IdentificacionCliente: "3101123456",
            FechaCorte: new DateTime(2026, 11, 2),
            LimiteAprobado: 100000m,
            SaldoAbierto: 24253.19m,
            CreditoAFavor: 0m,
            Disponible: 75746.81m,
            PorVencer: 0m,
            Vencido1a30: 24253.19m,
            Vencido31a60: 0m,
            Vencido61a90: 0m,
            Vencido91oMas: 0m,
            Detalle:
            [
                new LineaEstadoCuentaPdf("1", null, new DateTime(2026, 9, 5), new DateTime(2026, 10, 5),
                    24253.19m, 0m, 0m, 24253.19m, "Aceptado", DiasMora: 28)
            ])
        {
            Empresa = new EncabezadoEmpresaPdf(
                NombreEmisor: "Costa Pets S.A.",
                Identificacion: "3102886777",
                Telefono: "2222-2222",
                Correo: "info@costapets.example",
                Direccion: "San José")
        };

        var pdf = new GeneradorPdfQuestPdf().EstadoCuenta(reporte);

        Assert.True(pdf.Length > 800);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public void EstadoCuenta_ConSucursalDelCliente_GeneraPdfValido()
    {
        var reporte = new EstadoCuentaPdf("GUANAVET S.A.", "3101999999", new DateTime(2026, 11, 4), 2000000m, 585544.70m, 0m, 1414455.30m,
            88951.34m, 496593.36m, 0m, 0m, 0m,
            Detalle:
            [
                new LineaEstadoCuentaPdf("1308", null, new DateTime(2026, 9, 19), new DateTime(2026, 10, 19), 496593.36m, 0m, 0m, 496593.36m, "Aceptado", DiasMora: 16, Sucursal: "Heredia — Guanavet Heredia"),
                new LineaEstadoCuentaPdf("1356", null, new DateTime(2026, 10, 7), new DateTime(2026, 11, 6), 28341.53m, 0m, 0m, 28341.53m, "Aceptado"),
            ])
        { SucursalFiltro = "Heredia — Guanavet Heredia" };

        var pdf = new GeneradorPdfQuestPdf().EstadoCuenta(reporte);

        Assert.True(pdf.Length > 800);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }
}
