using SuvesaPosSitioAplicacion.DTOs.Ventas;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>Lo que quien hace la nota va eligiendo en la pantalla, antes de enviarlo al API.</summary>
public sealed class BorradorNotaCreditoExterna
{
    public long IdCliente { get; set; }
    public long? IdVenta { get; set; }
    /// <summary>null = todo lo disponible del documento.</summary>
    public decimal? Monto { get; set; }
    public string? Razon { get; set; }
    public bool SoloFiscal { get; set; }
    public string? MotivoSoloFiscal { get; set; }
}

/// <summary>
/// Reglas de la pantalla de nota de crédito externa. El API las vuelve a aplicar (es quien manda): aquí se evita enviar lo que se sabe
/// que va a rechazar y se explica el motivo antes de que el usuario confirme. Sin dependencias de interfaz, para poder probarlas.
/// </summary>
public static class ReglasNotaCreditoExterna
{
    /// <summary>Largo máximo del motivo: es la razón de la referencia a Hacienda.</summary>
    public const int MaximoRazon = 180;

    /// <summary>Una venta que ya se devolvió por la vía interna ya acreditó al cliente: la nota solo puede ser fiscal.</summary>
    public static bool SoloFiscalForzado(VentaCandidataNotaCreditoExternaWebDTO? venta) => venta is { TieneDevolucionInterna: true };

    /// <summary>El cliente se sugiere solo cuando hay uno solo activo con esa identificación; si hay varios, lo elige la persona.</summary>
    public static long ClienteSugerido(ValidacionNotaCreditoExternaWebDTO validacion)
    {
        var activos = validacion.ClientesCandidatos.Where(c => c.Activo).ToList();
        return activos.Count == 1 ? activos[0].IdCliente : 0;
    }

    /// <summary>Hay una venta interna con el mismo número y total: no se debe acreditar «a ciegas» como si no existiera.</summary>
    public static bool HayCoincidenciaFuerte(ValidacionNotaCreditoExternaWebDTO validacion) => validacion.VentasCandidatas.Any(v => v.Coincidencia == "Alta");

    /// <summary>Monto efectivo de la nota: lo indicado, o todo lo disponible del documento.</summary>
    public static decimal MontoEfectivo(ValidacionNotaCreditoExternaWebDTO validacion, BorradorNotaCreditoExterna borrador) => borrador.Monto ?? validacion.Disponible;

    /// <summary>Primer motivo por el que todavía no se puede crear la nota; null si está lista.</summary>
    public static string? Error(ValidacionNotaCreditoExternaWebDTO? validacion, BorradorNotaCreditoExterna borrador)
    {
        if (validacion is null || !validacion.Valido) return "Suba la factura y su acuse y valídelos antes de continuar.";

        if (borrador.IdCliente <= 0) return "Elija el cliente que recibirá el crédito.";
        var cliente = validacion.ClientesCandidatos.FirstOrDefault(c => c.IdCliente == borrador.IdCliente);
        if (cliente is null || !cliente.Activo) return "El cliente elegido no está activo: elija el cliente activo.";

        VentaCandidataNotaCreditoExternaWebDTO? venta = null;
        if (borrador.IdVenta is { } idVenta)
        {
            venta = validacion.VentasCandidatas.FirstOrDefault(v => v.IdVenta == idVenta);
            if (venta is null) return "La venta elegida no es una de las candidatas de este comprobante.";
            if (venta.IdCliente != borrador.IdCliente) return "La venta elegida es de otro cliente.";
        }

        var forzado = SoloFiscalForzado(venta);
        var soloFiscal = borrador.SoloFiscal || forzado;
        if (borrador.SoloFiscal && !forzado && string.IsNullOrWhiteSpace(borrador.MotivoSoloFiscal))
            return "Indique el motivo del modo «solo fiscal».";
        if (borrador.IdVenta is null && !soloFiscal && HayCoincidenciaFuerte(validacion))
            return "Esta factura parece existir como venta interna: vincúlela, o use «solo fiscal» si ya se acreditó al cliente.";

        var monto = MontoEfectivo(validacion, borrador);
        if (monto <= 0m) return "El monto de la nota debe ser mayor que cero.";
        if (monto > validacion.Disponible + 0.005m)
            return $"El monto ({monto:N2}) supera lo disponible de este comprobante ({validacion.Disponible:N2}).";

        var razon = (borrador.Razon ?? string.Empty).Trim();
        if (razon.Length == 0) return "Indique el motivo de la nota de crédito.";
        if (razon.Length > MaximoRazon) return $"El motivo no puede pasar de {MaximoRazon} caracteres.";

        return null;
    }

    /// <summary>Texto de qué le va a pasar al saldo del cliente con esta nota, para el resumen antes de confirmar.</summary>
    public static string EfectoFinanciero(ValidacionNotaCreditoExternaWebDTO validacion, BorradorNotaCreditoExterna borrador)
    {
        var venta = borrador.IdVenta is { } id ? validacion.VentasCandidatas.FirstOrDefault(v => v.IdVenta == id) : null;
        if (borrador.SoloFiscal || SoloFiscalForzado(venta))
            return "Solo fiscal: se emite a Hacienda, pero no cambia el saldo ni deja crédito a favor.";

        var monto = MontoEfectivo(validacion, borrador);
        if (venta is null) return $"El total ({monto:N2}) queda como crédito a favor del cliente.";

        var alSaldo = Math.Min(monto, venta.Saldo);
        var aFavor = monto - alSaldo;
        return aFavor > 0.005m
            ? $"Baja el saldo de la factura en {alSaldo:N2} y deja {aFavor:N2} como crédito a favor del cliente."
            : $"Baja el saldo de la factura en {alSaldo:N2}.";
    }
}
