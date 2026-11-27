using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Pantalla de nota de crédito externa: lo que se puede enviar y lo que se le explica a la persona antes de confirmar. El API vuelve a validar todo;
/// aquí se evita mandar lo que se sabe que va a rechazar.
/// </summary>
public sealed class ReglasNotaCreditoExternaTests
{
    private const long ClienteVivo = 172;

    private static ValidacionNotaCreditoExternaWebDTO Validacion(Action<ValidacionNotaCreditoExternaWebDTO>? ajustar = null)
    {
        var v = new ValidacionNotaCreditoExternaWebDTO
        {
            Valido = true, Clave = new string('5', 50), Consecutivo = "00100001010000000725", TotalOrigen = 7595.72m, Disponible = 7595.72m,
            Lineas =
            {
                new LineaNotaCreditoExternaWebDTO { NumeroLinea = 1, Cantidad = 4m, PrecioUnitario = 1180.47m, SubTotal = 4721.88m, MontoImpuesto = 613.84m, MontoTotalLinea = 5335.72m, CantidadDisponible = 4m },
                new LineaNotaCreditoExternaWebDTO { NumeroLinea = 2, Cantidad = 2m, PrecioUnitario = 1000m, SubTotal = 2000m, MontoImpuesto = 260m, MontoTotalLinea = 2260m, CantidadDisponible = 2m },
            },
            ClientesCandidatos =
            {
                new ClienteCandidatoNotaCreditoExternaWebDTO { IdCliente = ClienteVivo, Nombre = "TRES CIENTO UNO S.A.", Cedula = "3101054264", Activo = true },
                new ClienteCandidatoNotaCreditoExternaWebDTO { IdCliente = 56, Nombre = "SOCIEDAD ANONIMA", Cedula = "3101054264", Activo = false, Nota = "Unificado en 52" },
            },
        };
        ajustar?.Invoke(v);
        return v;
    }

    private static VentaCandidataNotaCreditoExternaWebDTO Venta(long id = 500, decimal saldo = 2260m, string coincidencia = "Alta", bool devolucionInterna = false, long cliente = ClienteVivo)
        => new() { IdVenta = id, NumFactura = 725, Total = 2260m, Saldo = saldo, IdCliente = cliente, Coincidencia = coincidencia, TieneDevolucionInterna = devolucionInterna };

    private static BorradorNotaCreditoExterna Borrador(Action<BorradorNotaCreditoExterna>? ajustar = null)
    {
        var b = new BorradorNotaCreditoExterna
        {
            IdCliente = ClienteVivo, Razon = "Crédito por factura del sistema anterior",
            Cantidades = new Dictionary<int, decimal> { [1] = 4m, [2] = 2m },   // todo lo que queda, como al validar
        };
        ajustar?.Invoke(b);
        return b;
    }

    // ------------------------------------------------------------------ Sugerencias

    [Fact]
    public void ElClienteSeSugiereSoloSiHayUnoActivo()
    {
        Assert.Equal(ClienteVivo, ReglasNotaCreditoExterna.ClienteSugerido(Validacion()));   // el inactivo no cuenta

        var dos = Validacion(v => v.ClientesCandidatos.Add(new ClienteCandidatoNotaCreditoExternaWebDTO { IdCliente = 300, Nombre = "Otro", Activo = true }));
        Assert.Equal(0, ReglasNotaCreditoExterna.ClienteSugerido(dos));                      // varios activos: elige la persona

        var ninguno = Validacion(v => v.ClientesCandidatos.Clear());
        Assert.Equal(0, ReglasNotaCreditoExterna.ClienteSugerido(ninguno));
    }

    [Fact]
    public void SoloFiscalEsObligatorioSiLaVentaYaSeDevolvioDeFormaInterna()
    {
        Assert.True(ReglasNotaCreditoExterna.SoloFiscalForzado(Venta(devolucionInterna: true)));
        Assert.False(ReglasNotaCreditoExterna.SoloFiscalForzado(Venta()));
        Assert.False(ReglasNotaCreditoExterna.SoloFiscalForzado(null));
    }

    // ------------------------------------------------------------------ Lo que bloquea

    [Fact]
    public void SinValidarOConErrores_NoSePuedeCrear()
    {
        Assert.Contains("valídelos", ReglasNotaCreditoExterna.Error(null, Borrador())!);
        Assert.Contains("valídelos", ReglasNotaCreditoExterna.Error(Validacion(v => v.Valido = false), Borrador())!);
    }

    [Fact]
    public void ElClienteEsObligatorioYTieneQueEstarActivo()
    {
        Assert.Contains("Elija el cliente", ReglasNotaCreditoExterna.Error(Validacion(), Borrador(b => b.IdCliente = 0))!);
        Assert.Contains("no está activo", ReglasNotaCreditoExterna.Error(Validacion(), Borrador(b => b.IdCliente = 56))!);
        Assert.Contains("no está activo", ReglasNotaCreditoExterna.Error(Validacion(), Borrador(b => b.IdCliente = 999))!);
    }

    [Fact]
    public void LaVentaTieneQueSerCandidataYDelMismoCliente()
    {
        var v = Validacion(x => x.VentasCandidatas.Add(Venta(cliente: 300)));

        Assert.Contains("no es una de las candidatas", ReglasNotaCreditoExterna.Error(v, Borrador(b => b.IdVenta = 777))!);
        Assert.Contains("de otro cliente", ReglasNotaCreditoExterna.Error(v, Borrador(b => b.IdVenta = 500))!);
    }

    [Fact]
    public void SoloFiscalElegidoExigeMotivo_PeroForzadoNo()
    {
        var sinDevolucion = Validacion(x => x.VentasCandidatas.Add(Venta()));
        Assert.Contains("motivo del modo", ReglasNotaCreditoExterna.Error(sinDevolucion, Borrador(b => { b.IdVenta = 500; b.SoloFiscal = true; }))!);
        Assert.Null(ReglasNotaCreditoExterna.Error(sinDevolucion, Borrador(b => { b.IdVenta = 500; b.SoloFiscal = true; b.MotivoSoloFiscal = "Compensada aparte"; })));

        var conDevolucion = Validacion(x => x.VentasCandidatas.Add(Venta(devolucionInterna: true)));
        Assert.Null(ReglasNotaCreditoExterna.Error(conDevolucion, Borrador(b => b.IdVenta = 500)));   // el motivo lo pone el API
    }

    [Fact]
    public void SiParecidaExisteComoVentaInterna_NoSeAcreditaACiegas()
    {
        var v = Validacion(x => x.VentasCandidatas.Add(Venta(coincidencia: "Alta")));

        Assert.Contains("parece existir", ReglasNotaCreditoExterna.Error(v, Borrador())!);
        Assert.Null(ReglasNotaCreditoExterna.Error(v, Borrador(b => b.IdVenta = 500)));                                        // vinculándola
        Assert.Null(ReglasNotaCreditoExterna.Error(v, Borrador(b => { b.SoloFiscal = true; b.MotivoSoloFiscal = "Ya acreditada"; })));   // o solo fiscal

        var media = Validacion(x => x.VentasCandidatas.Add(Venta(coincidencia: "Media")));
        Assert.Null(ReglasNotaCreditoExterna.Error(media, Borrador()));                                                        // coincidencia débil: no bloquea
    }

    [Fact]
    public void AlValidar_SePropone_TodoLoQueQuedaDeCadaLinea()
    {
        var v = Validacion(x => x.Lineas[1].CantidadDisponible = 0m);   // la línea 2 ya se acreditó por completo

        var porDefecto = ReglasNotaCreditoExterna.CantidadesPorDefecto(v);

        Assert.Equal(new Dictionary<int, decimal> { [1] = 4m }, porDefecto);
    }

    [Fact]
    public void ElMontoEsLaSumaDeLasLineasElegidas_Y_LaCantidadCompletaVaigualAlXml()
    {
        var v = Validacion();

        Assert.Equal(7595.72m, ReglasNotaCreditoExterna.MontoEfectivo(v, Borrador()));                                  // todo: lo que dice el XML
        Assert.Equal(2260m, ReglasNotaCreditoExterna.MontoEfectivo(v, Borrador(b => b.Cantidades = new() { [2] = 2m })));  // solo la línea 2
        Assert.Equal(0m, ReglasNotaCreditoExterna.MontoEfectivo(v, Borrador(b => b.Cantidades = new())));
    }

    [Fact]
    public void UnaCantidadMenor_ConservaElPrecio_YProporcionaElIva_IgualQueElApi()
    {
        var v = Validacion();

        // Mismo cálculo que ServicioNotaCreditoExterna.ConstruirLineas: 3 × 1.180,47 = 3.541,41 de base y 613,84 × 3 / 4 = 460,38 de IVA.
        Assert.Equal(4001.79m, ReglasNotaCreditoExterna.MontoLinea(v.Lineas[0], 3m));
        Assert.Equal(5335.72m, ReglasNotaCreditoExterna.MontoLinea(v.Lineas[0], 4m));
        Assert.Equal(0m, ReglasNotaCreditoExterna.MontoLinea(v.Lineas[0], 0m));
        Assert.Equal(565m, ReglasNotaCreditoExterna.MontoLinea(v.Lineas[1], 0.5m));
    }

    [Fact]
    public void LasLineasSinCantidadNoSeEnvian()
    {
        var v = Validacion();

        var enviar = ReglasNotaCreditoExterna.LineasParaEnviar(v, Borrador(b => b.Cantidades = new() { [1] = 3m, [2] = 0m }));

        var linea = Assert.Single(enviar);
        Assert.Equal(1, linea.NumeroLinea);
        Assert.Equal(3m, linea.Cantidad);
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, false)]          // cero = esa línea no se acredita
    [InlineData(0.5, false)]
    [InlineData(4, false)]
    [InlineData(4.001, true)]       // más de lo que queda
    [InlineData(1.2345, true)]      // más de 3 decimales: el esquema de Hacienda no lo admite
    public void LaCantidadDeUnaLineaDebeEstarEntreCeroYLoQueQueda_ConTresDecimalesComoMaximo(double cantidad, bool bloquea)
    {
        var v = Validacion();

        Assert.Equal(bloquea, ReglasNotaCreditoExterna.ErrorDeCantidad(v.Lineas[0], (decimal)cantidad) is not null);
        Assert.Equal(bloquea, ReglasNotaCreditoExterna.Error(v, Borrador(b => b.Cantidades = new() { [1] = (decimal)cantidad, [2] = 2m })) is not null);
    }

    [Fact]
    public void SinNingunaLineaConCantidad_NoSePuedeCrear()
        => Assert.Contains("al menos una línea", ReglasNotaCreditoExterna.Error(Validacion(), Borrador(b => b.Cantidades = new() { [1] = 0m, [2] = 0m }))!);

    [Fact]
    public void ElMotivoEsObligatorioYTieneUnTope()
    {
        Assert.Contains("motivo de la nota", ReglasNotaCreditoExterna.Error(Validacion(), Borrador(b => b.Razon = "   "))!);
        Assert.Contains("180", ReglasNotaCreditoExterna.Error(Validacion(), Borrador(b => b.Razon = new string('x', 181)))!);
        Assert.Null(ReglasNotaCreditoExterna.Error(Validacion(), Borrador(b => b.Razon = new string('x', 180))));
    }

    [Fact]
    public void ConTodoEnOrden_NoHayError()
        => Assert.Null(ReglasNotaCreditoExterna.Error(Validacion(), Borrador()));

    // ------------------------------------------------------------------ Resumen del efecto

    [Fact]
    public void SinVenta_TodoQuedaComoCreditoAFavor()
        => Assert.Contains("crédito a favor del cliente", ReglasNotaCreditoExterna.EfectoFinanciero(Validacion(), Borrador()));

    [Fact]
    public void ConVenta_BajaElSaldoYElExcedenteQuedaAFavor()
    {
        var v = Validacion(x => x.VentasCandidatas.Add(Venta(saldo: 1000m)));

        var texto = ReglasNotaCreditoExterna.EfectoFinanciero(v, Borrador(b => b.IdVenta = 500));

        Assert.Contains("Baja el saldo de la factura en 1,000.00", texto.Replace("1.000,00", "1,000.00"));
        Assert.Contains("crédito a favor", texto);
    }

    [Fact]
    public void ConVentaConSaldoSuficiente_SoloBajaElSaldo()
    {
        var v = Validacion(x => x.VentasCandidatas.Add(Venta(saldo: 8000m)));

        var texto = ReglasNotaCreditoExterna.EfectoFinanciero(v, Borrador(b => b.IdVenta = 500));

        Assert.StartsWith("Baja el saldo de la factura", texto);
        Assert.DoesNotContain("crédito a favor", texto);
    }

    [Fact]
    public void SoloFiscal_NoTocaNada()
    {
        var elegido = ReglasNotaCreditoExterna.EfectoFinanciero(Validacion(), Borrador(b => b.SoloFiscal = true));
        var forzado = ReglasNotaCreditoExterna.EfectoFinanciero(
            Validacion(x => x.VentasCandidatas.Add(Venta(devolucionInterna: true))), Borrador(b => b.IdVenta = 500));

        Assert.Contains("Solo fiscal", elegido);
        Assert.Contains("no cambia el saldo", forzado);
    }
}
