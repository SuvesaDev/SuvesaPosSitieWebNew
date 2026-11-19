using SuvesaPosSitioAplicacion.Services;
using static SuvesaPosSitioAplicacion.Services.PreparacionPagoVenta;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>REQ-03: varias líneas bajo una misma modalidad de pago (efectivo, tarjeta, depósito, cheque), cada una con su referencia, banco, fecha y monto.</summary>
public class PagoVariasLineasTests
{
    private static PagoExtra Extra(string codigo, decimal monto, string? referencia = null, string? cheque = null, int? banco = null, DateTime? fecha = null)
        => new() { Codigo = codigo, Monto = monto, Referencia = referencia, NumeroCheque = cheque, IdBanco = banco, FechaPago = fecha };

    private static readonly DateTime Ayer = HoyCostaRica().AddDays(-1);

    [Fact]
    public void ExtraEnEfectivo_NoPideReferenciaNiBanco_Y_PuedeDarVuelto()
    {
        var l = LineaDeExtra(esEfectivo: true, tarjeta: false, deposito: false, cheque: false, Extra("EFE", 5000m), pedirFecha: true);

        Assert.True(l.EsEfectivo);
        Assert.False(l.RequiereReferencia);
        Assert.False(l.RequiereBanco);
        Assert.False(l.RequiereFecha);
        // Dos líneas de efectivo suman y el exceso es vuelto.
        var r = Calcular(8000m, new[] { l, LineaDeExtra(true, false, false, false, Extra("EFE", 5000m), true) });
        Assert.True(r.Cubre100);
        Assert.Equal(2000m, r.Vuelto);
    }

    [Fact]
    public void ExtraConTarjeta_PideReferencia_PeroNoBancoNiFecha()
    {
        var sin = LineaDeExtra(false, tarjeta: true, false, false, Extra("TAR", 100m), pedirFecha: true);
        Assert.True(sin.RequiereReferencia);
        Assert.False(sin.RequiereBanco);
        Assert.False(sin.RequiereFecha);
        Assert.Contains(ValidarInstrumentos(new[] { sin }), e => e.Contains("referencia"));
        Assert.Empty(ValidarInstrumentos(new[] { LineaDeExtra(false, true, false, false, Extra("TAR", 100m, referencia: "A1"), true) }));
    }

    [Fact]
    public void DosDepositos_CadaUnoConSuReferenciaBancoFechaYMonto()
    {
        var uno = new LineaPago("DEP", 60000m, false, true, "R-1", RequiereBanco: true, IdBanco: 1, RequiereFecha: true, Fecha: Ayer);
        var dos = LineaDeExtra(false, false, deposito: true, false, Extra("DEP", 40000m, referencia: "R-2", banco: 2, fecha: Ayer), pedirFecha: true);

        var r = Calcular(100000m, new[] { uno, dos });

        Assert.True(r.Cubre100);
        Assert.Empty(r.Errores);
    }

    [Fact]
    public void UnDeposito_SinFecha_SeRechaza_Y_ConFechaFutura_Tambien()
    {
        var sinFecha = LineaDeExtra(false, false, true, false, Extra("DEP", 100m, referencia: "R", banco: 1), pedirFecha: true);
        Assert.Contains(ValidarInstrumentos(new[] { sinFecha }), e => e.Contains("fecha"));

        var futura = LineaDeExtra(false, false, true, false, Extra("DEP", 100m, referencia: "R", banco: 1, fecha: HoyCostaRica().AddDays(3)), pedirFecha: true);
        Assert.Contains(ValidarInstrumentos(new[] { futura }), e => e.Contains("futura"));
    }

    [Fact]
    public void UnChequePosfechado_NoEsFuturaIlegal_Y_SinPedirFecha_NoSeExige()
    {
        var posfechado = LineaDeExtra(false, false, false, cheque: true, Extra("CHQ", 100m, cheque: "55", banco: 1, fecha: HoyCostaRica().AddDays(30)), pedirFecha: true);
        Assert.Empty(ValidarInstrumentos(new[] { posfechado }));

        // Una pantalla que no envía la fecha (pedirFecha = false) no la exige en el depósito.
        var deposito = LineaDeExtra(false, false, true, false, Extra("DEP", 100m, referencia: "R", banco: 1), pedirFecha: false);
        Assert.Empty(ValidarInstrumentos(new[] { deposito }));
    }

    [Fact]
    public void MismoBancoYReferencia_EnDosLineas_SigueSiendoDuplicado()
    {
        var a = LineaDeExtra(false, false, true, false, Extra("DEP", 50m, referencia: "R", banco: 1, fecha: Ayer), true);
        var b = LineaDeExtra(false, false, true, false, Extra("DEP", 50m, referencia: " r ", banco: 1, fecha: Ayer), true);
        Assert.Contains(ValidarInstrumentos(new[] { a, b }), e => e.Contains("mismo banco"));
    }
}
