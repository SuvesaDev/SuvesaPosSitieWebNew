using SuvesaPosSitioAplicacion.DTOs.Ventas;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>Lo que quien hace la nota va eligiendo en la pantalla, antes de enviarlo al API.</summary>
public sealed class BorradorNotaCreditoExterna
{
    public long IdCliente { get; set; }
    public long? IdVenta { get; set; }
    /// <summary>Cantidad a acreditar de cada línea del comprobante original (por número de línea). Una línea ausente o en cero no se acredita.</summary>
    public Dictionary<int, decimal> Cantidades { get; set; } = new();
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

    /// <summary>Decimales que admite la cantidad de una línea en el esquema de Hacienda.</summary>
    public const int DecimalesCantidad = 3;

    // ------------------------------------------------------------------ Líneas y monto

    /// <summary>Al validar, se propone acreditar todo lo que queda de cada línea; la persona baja lo que no corresponda.</summary>
    public static Dictionary<int, decimal> CantidadesPorDefecto(ValidacionNotaCreditoExternaWebDTO validacion)
        => validacion.Lineas.Where(l => l.CantidadDisponible > 0m).ToDictionary(l => l.NumeroLinea, l => l.CantidadDisponible);

    public static decimal CantidadDe(BorradorNotaCreditoExterna borrador, int numeroLinea) => borrador.Cantidades.GetValueOrDefault(numeroLinea);

    /// <summary>
    /// Importe de una línea con la cantidad indicada, igual que lo calcula el API: la cantidad completa de la línea vale lo que dice el XML; una
    /// cantidad menor conserva el precio unitario y toma la proporción del IVA de la línea.
    /// </summary>
    public static decimal MontoLinea(LineaNotaCreditoExternaWebDTO linea, decimal cantidad)
    {
        if (cantidad <= 0m) return 0m;
        if (cantidad == linea.Cantidad) return linea.MontoTotalLinea;
        var subtotal = Redondear5(cantidad * linea.PrecioUnitario);
        var impuesto = linea.Cantidad > 0m ? Redondear5(linea.MontoImpuesto * cantidad / linea.Cantidad) : 0m;
        return Redondear5(subtotal + impuesto);
    }

    private static decimal Redondear5(decimal valor) => Math.Round(valor, 5, MidpointRounding.AwayFromZero);

    /// <summary>Monto de la nota: la suma de las líneas elegidas.</summary>
    public static decimal MontoEfectivo(ValidacionNotaCreditoExternaWebDTO validacion, BorradorNotaCreditoExterna borrador)
        => validacion.Lineas.Sum(l => MontoLinea(l, CantidadDe(borrador, l.NumeroLinea)));

    /// <summary>Las líneas que se mandan al API: solo las que tienen cantidad.</summary>
    public static List<LineaCreditoNotaCreditoExternaWebComando> LineasParaEnviar(ValidacionNotaCreditoExternaWebDTO validacion, BorradorNotaCreditoExterna borrador)
        => validacion.Lineas
            .Where(l => CantidadDe(borrador, l.NumeroLinea) > 0m)
            .Select(l => new LineaCreditoNotaCreditoExternaWebComando { NumeroLinea = l.NumeroLinea, Cantidad = CantidadDe(borrador, l.NumeroLinea) })
            .ToList();

    /// <summary>Motivo por el que la cantidad de una línea no se puede enviar; null si está bien (cero también: esa línea no se acredita).</summary>
    public static string? ErrorDeCantidad(LineaNotaCreditoExternaWebDTO linea, decimal cantidad)
    {
        if (cantidad < 0m) return $"La cantidad de la línea {linea.NumeroLinea} no puede ser negativa.";
        if (decimal.Round(cantidad, DecimalesCantidad) != cantidad) return $"La cantidad de la línea {linea.NumeroLinea} admite hasta {DecimalesCantidad} decimales.";
        if (cantidad > linea.CantidadDisponible)
            return $"La línea {linea.NumeroLinea} solo tiene {linea.CantidadDisponible:0.###} por acreditar (se pidieron {cantidad:0.###}).";
        return null;
    }

    // ------------------------------------------------------------------ Sugerencias

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

    // ------------------------------------------------------------------ Lo que bloquea

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

        foreach (var linea in validacion.Lineas)
            if (ErrorDeCantidad(linea, CantidadDe(borrador, linea.NumeroLinea)) is { } errorLinea) return errorLinea;
        if (LineasParaEnviar(validacion, borrador).Count == 0) return "Indique la cantidad a acreditar de al menos una línea.";

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
