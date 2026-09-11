using SuvesaPosSitioAplicacion.DTOs.Reportes;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>Centraliza los datos de las gráficas para pantalla y exportaciones.</summary>
public static class ResumenGraficasReporteOperacion
{
    public static ResumenGraficoOperacion Crear(string tipoReporte, IReadOnlyList<FilaReporteOperacionWebDTO> filas)
    {
        var esCantidad = tipoReporte is "inventario" or "lotes" or "trazabilidad" or "auditoria";
        var evolucion = ConstruirEvolucion(tipoReporte, filas);
        var comparativo = ConstruirComparativo(tipoReporte, filas);

        return new ResumenGraficoOperacion(
            TituloEvolucion(tipoReporte),
            TituloComparativo(tipoReporte),
            DescripcionEvolucion(tipoReporte),
            DescripcionComparativo(tipoReporte),
            esCantidad,
            evolucion,
            comparativo);
    }

    private static IReadOnlyList<PuntoGraficoOperacion> ConstruirEvolucion(string tipoReporte, IReadOnlyList<FilaReporteOperacionWebDTO> filas)
    {
        if (tipoReporte == "cuentas-por-cobrar")
        {
            // La consulta ya llegó filtrada con la fecha de negocio del equipo;
            // para la gráfica usamos el último hecho del resultado, sin leer el
            // reloj del servidor desde esta capa compartida con exportaciones.
            var referencia = filas.Select(x => x.Fecha.Date).DefaultIfEmpty(DateTime.MinValue).Max();
            return filas.GroupBy(x => BandaAntiguedad(x, referencia))
                .OrderBy(x => OrdenBandaAntiguedad(x.Key))
                .Select(x => new PuntoGraficoOperacion(x.Key, x.Sum(f => f.Saldo))).ToList();
        }

        if (tipoReporte is "inventario" or "lotes")
        {
            return filas
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Referencia) ? "Sin referencia" : x.Referencia)
                .Select(x => new PuntoGraficoOperacion(x.Key, x.Sum(f => Valor(tipoReporte, f))))
                .OrderByDescending(x => Math.Abs(x.Valor))
                .Take(8)
                .ToList();
        }

        if (tipoReporte == "cuentas-por-pagar")
        {
            return filas.GroupBy(x => string.IsNullOrWhiteSpace(x.Estado) ? "Sin antigüedad" : x.Estado)
                .OrderBy(x => OrdenBandaCxP(x.Key))
                .Select(x => new PuntoGraficoOperacion(x.Key, x.Sum(f => f.Saldo)))
                .ToList();
        }

        return filas
            .Where(x => x.Fecha != default)
            .GroupBy(x => x.Fecha.Date)
            .OrderByDescending(x => x.Key)
            .Take(8)
            .OrderBy(x => x.Key)
            .Select(x => new PuntoGraficoOperacion(x.Key.ToString("dd/MM"), x.Sum(f => Valor(tipoReporte, f))))
            .ToList();
    }

    private static IReadOnlyList<PuntoGraficoOperacion> ConstruirComparativo(string tipoReporte, IReadOnlyList<FilaReporteOperacionWebDTO> filas)
    {
        if (tipoReporte is "ventas" or "compras")
            return Agrupar(filas, x => x.Entidad, x => Valor(tipoReporte, x));

        if (tipoReporte == "cuentas-por-pagar")
            return Agrupar(filas, x => x.Entidad, x => x.Saldo);

        if (tipoReporte == "cuentas-por-cobrar")
            return Agrupar(filas, x => x.Entidad, x => x.Saldo);

        if (tipoReporte == "auditoria")
            return Agrupar(filas, x => x.Estado, _ => 1m);

        var puntos = filas
            .GroupBy(x => string.IsNullOrWhiteSpace(x.Estado) ? "Sin estado" : x.Estado.Trim())
            .Select(x => new PuntoGraficoOperacion(x.Key, x.Sum(f => Valor(tipoReporte, f))))
            .OrderByDescending(x => Math.Abs(x.Valor))
            .Take(8)
            .ToList();

        return puntos.All(x => x.Valor == 0)
            ? filas.GroupBy(x => string.IsNullOrWhiteSpace(x.Estado) ? "Sin estado" : x.Estado.Trim())
                .Select(x => new PuntoGraficoOperacion(x.Key, x.Count()))
                .OrderByDescending(x => x.Valor)
                .Take(8)
                .ToList()
            : puntos;
    }

    private static IReadOnlyList<PuntoGraficoOperacion> Agrupar(IReadOnlyList<FilaReporteOperacionWebDTO> filas, Func<FilaReporteOperacionWebDTO, string> etiqueta, Func<FilaReporteOperacionWebDTO, decimal> valor)
        => filas.GroupBy(x => string.IsNullOrWhiteSpace(etiqueta(x)) ? "Sin dato" : etiqueta(x).Trim())
            .Select(x => new PuntoGraficoOperacion(x.Key, x.Sum(valor)))
            .OrderByDescending(x => Math.Abs(x.Valor)).Take(8).ToList();

    private static string BandaAntiguedad(FilaReporteOperacionWebDTO fila, DateTime referencia)
    {
        if (!fila.FechaVencimiento.HasValue) return "Sin vencimiento";
        var dias = (referencia.Date - fila.FechaVencimiento.Value.Date).Days;
        return dias switch { <= 0 => "Por vencer", <= 30 => "1–30 días", <= 60 => "31–60 días", _ => "+60 días" };
    }

    private static int OrdenBandaAntiguedad(string banda) => banda switch
    {
        "Por vencer" => 0, "1–30 días" => 1, "31–60 días" => 2, "+60 días" => 3, _ => 4
    };

    private static int OrdenBandaCxP(string banda) => banda switch
    {
        "0–30 días" => 0, "31–60 días" => 1, "61–90 días" => 2, "+90 días" => 3, _ => 4
    };

    private static decimal Valor(string tipoReporte, FilaReporteOperacionWebDTO fila) => tipoReporte switch
    {
        "cuentas-por-cobrar" or "caja" => fila.Saldo,
        "inventario" or "lotes" or "trazabilidad" => fila.Cantidad,
        "auditoria" => 1m,
        _ => fila.Monto
    };

    private static string TituloEvolucion(string tipoReporte) => tipoReporte switch
    {
        "ventas" => "Ventas por día",
        "cuentas-por-cobrar" => "Antigüedad de la cartera",
        "cuentas-por-pagar" => "Antigüedad de cuentas por pagar",
        "caja" => "Flujo neto de caja por día",
        "arqueos-cierres" => "Arqueos y cierres por día",
        "depositos" => "Depósitos por día",
        "compras" => "Compras por día",
        "inventario" => "Existencia por artículo",
        "lotes" => "Existencia por lote",
        "trazabilidad" => "Movimientos por día",
        "auditoria" => "Eventos de auditoría por día",
        _ => "Evolución del reporte"
    };

    private static string TituloComparativo(string tipoReporte) => tipoReporte switch
    {
        "ventas" => "Clientes con mayor venta",
        "cuentas-por-cobrar" => "Clientes con mayor saldo",
        "cuentas-por-pagar" => "Proveedores con mayor saldo",
        "compras" => "Proveedores con mayor compra",
        "inventario" => "Existencia por condición",
        "lotes" => "Unidades por vencimiento",
        "trazabilidad" => "Unidades por movimiento",
        "auditoria" => "Acciones registradas",
        _ => "Distribución por estado"
    };

    private static string DescripcionEvolucion(string tipoReporte) => tipoReporte switch
    {
        "cuentas-por-cobrar" => "Saldo pendiente agrupado por días transcurridos desde su vencimiento.",
        "inventario" or "lotes" => "Los ocho artículos o lotes con mayor existencia dentro del resultado.",
        "trazabilidad" => "Unidades netas movidas por fecha.",
        "auditoria" => "Cantidad de cambios registrados por fecha.",
        _ => "Evolución diaria de los valores del reporte."
    };

    private static string DescripcionComparativo(string tipoReporte) => tipoReporte switch
    {
        "ventas" or "compras" => "Los ocho terceros con el mayor importe dentro del período.",
        "cuentas-por-pagar" => "Los ocho proveedores con mayor saldo pendiente.",
        "cuentas-por-cobrar" => "Los ocho clientes con mayor saldo pendiente.",
        "auditoria" => "Cantidad de eventos por tipo de acción.",
        _ => "Distribución del resultado por condición operativa."
    };
}

public sealed record ResumenGraficoOperacion(
    string TituloEvolucion,
    string TituloEstados,
    string DescripcionEvolucion,
    string DescripcionEstados,
    bool EsCantidad,
    IReadOnlyList<PuntoGraficoOperacion> Evolucion,
    IReadOnlyList<PuntoGraficoOperacion> Estados);

public sealed record PuntoGraficoOperacion(string Etiqueta, decimal Valor);
