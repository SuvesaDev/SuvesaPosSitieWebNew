namespace SuvesaPosSitioAplicacion.Views.Reportes;

[Flags]
public enum FiltroReporteVisible
{
    Ninguno = 0,
    Periodo = 1 << 0,
    Texto = 1 << 1,
    Sucursal = 1 << 2,
    Empresa = 1 << 3,
    Cliente = 1 << 4,
    Proveedor = 1 << 5,
    Articulo = 1 << 6,
    Bodega = 1 << 7,
    Lote = 1 << 8,
    Agente = 1 << 9,
    Familia = 1 << 10,
    TipoVenta = 1 << 11,
    IncluirAnuladas = 1 << 12,
    EstadoLiquidacion = 1 << 13,
    RutaComercial = 1 << 14,
    BeneficiarioComision = 1 << 15,
    Corte = 1 << 16,
}

/// <summary>
/// Definición de experiencia de usuario de un reporte. Mantiene en un solo sitio
/// el título, propósito, permiso y filtros que realmente entiende el API.
/// </summary>
public sealed record ReporteCatalogo(
    string Tipo,
    string Titulo,
    string Codigo,
    string Descripcion,
    string Icono,
    FiltroReporteVisible Filtros,
    FiltroReporteVisible FiltrosBasicos,
    bool MostrarGraficas = true,
    string EtiquetaPeriodo = "Periodo")
{
    public string Ruta => $"/moduloReportes/{Tipo}";
    public FiltroReporteVisible FiltrosAvanzados => Filtros & ~FiltrosBasicos;
    public bool Tiene(FiltroReporteVisible filtro) => Filtros.HasFlag(filtro);
}

/// <summary>Dominio funcional mostrado en el catálogo de entrada.</summary>
public sealed record GrupoReportes(string Id, string Nombre, string Icono, ReporteCatalogo[] Opciones);

/// <summary>
/// Catálogo único de los 28 reportes hoja. Se comparte entre el catálogo, cada
/// pantalla, las preferencias y las pruebas; no introduce códigos de permiso.
/// </summary>
public static class CatalogoReportes
{
    private const FiltroReporteVisible Periodo = FiltroReporteVisible.Periodo;
    private const FiltroReporteVisible Texto = FiltroReporteVisible.Texto;
    private const FiltroReporteVisible Sucursal = FiltroReporteVisible.Sucursal;
    private const FiltroReporteVisible Empresa = FiltroReporteVisible.Empresa;
    private const FiltroReporteVisible Cliente = FiltroReporteVisible.Cliente;
    private const FiltroReporteVisible Proveedor = FiltroReporteVisible.Proveedor;
    private const FiltroReporteVisible Articulo = FiltroReporteVisible.Articulo;
    private const FiltroReporteVisible Bodega = FiltroReporteVisible.Bodega;
    private const FiltroReporteVisible Lote = FiltroReporteVisible.Lote;
    private const FiltroReporteVisible Agente = FiltroReporteVisible.Agente;
    private const FiltroReporteVisible Familia = FiltroReporteVisible.Familia;
    private const FiltroReporteVisible TipoVenta = FiltroReporteVisible.TipoVenta;
    private const FiltroReporteVisible Anuladas = FiltroReporteVisible.IncluirAnuladas;
    private const FiltroReporteVisible Liquidacion = FiltroReporteVisible.EstadoLiquidacion;
    private const FiltroReporteVisible RutaComercial = FiltroReporteVisible.RutaComercial;
    private const FiltroReporteVisible BeneficiarioComision = FiltroReporteVisible.BeneficiarioComision;
    private const FiltroReporteVisible Corte = FiltroReporteVisible.Corte;

    public static readonly GrupoReportes[] Grupos =
    [
        new("gerencia", "Gerencia", "bi-speedometer2",
        [
            R("panel-ejecutivo", "Panel ejecutivo", "MODULO_REPORTES.PANEL_EJECUTIVO",
                "Indicadores consolidados para revisar la operación del negocio.", "bi-speedometer2",
                Periodo | Sucursal | Empresa, Periodo | Sucursal, false),
            R("rentabilidad", "Rentabilidad comercial", "MODULO_REPORTES.RENTABILIDAD",
                "Margen bruto y utilidad después de comisiones por factura.", "bi-graph-up-arrow",
                Periodo | Sucursal | Cliente | Articulo | Texto, Periodo | Sucursal | Cliente),
            R("ventas-compras", "Ventas vs. compras", "MODULO_REPORTES.VENTAS_VS_COMPRAS",
                "Compara diariamente la facturación y las compras del periodo.", "bi-arrow-left-right",
                Periodo, Periodo),
        ]),
        new("ventas-clientes", "Ventas y clientes", "bi-receipt",
        [
            R("ventas", "Ventas", "MODULO_REPORTES.VENTAS",
                "Facturación neta, devoluciones y saldos por documento.", "bi-receipt",
                Periodo | Sucursal | Empresa | Cliente | Agente | TipoVenta | Anuladas | Texto,
                Periodo | Sucursal | Cliente),
            R("ventas-detalle", "Ventas detalladas", "MODULO_REPORTES.VENTAS_DETALLE",
                "Artículos vendidos, cantidades, descuentos y margen bruto.", "bi-list-columns-reverse",
                Periodo | Sucursal | Cliente | Articulo | Agente | Familia | TipoVenta | Anuladas | Texto,
                Periodo | Sucursal | Cliente | Articulo),
            R("ventas-horas", "Ventas entre horas", "MODULO_REPORTES.VENTAS_ENTRE_HORAS",
                "Distribución de documentos y montos por franja horaria.", "bi-clock-history",
                Periodo | Sucursal | Empresa | Agente | TipoVenta, Periodo | Sucursal),
            R("clientes", "Clientes y comportamiento", "MODULO_REPORTES.COMPORTAMIENTO_DE_CLIENTES",
                "Ranking, ticket promedio y clientes sin ventas en el periodo.", "bi-people",
                Periodo | Sucursal | Cliente | Texto, Periodo | Sucursal | Cliente),
            R("kpi-rutas", "KPI por ruta", "MODULO_REPORTES.KPI_POR_RUTA_COMERCIAL",
                "Ventas, clientes atendidos y comisión generada por ruta.", "bi-signpost-split",
                Periodo | Sucursal | Texto, Periodo | Sucursal),
            R("comisiones", "Comisiones", "VENTAS.COMISIONES",
                "Comisiones generadas y estado de liquidación por beneficiario.", "bi-cash-coin",
                Periodo | Sucursal | Liquidacion | RutaComercial | BeneficiarioComision | Corte,
                Periodo | Sucursal | Liquidacion, false),
        ]),
        new("cartera", "Cartera", "bi-wallet2",
        [
            R("cuentas-por-cobrar", "Cuentas por cobrar", "MODULO_REPORTES.CUENTAS_POR_COBRAR",
                "Saldos pendientes y antigüedad de la cartera por cliente.", "bi-wallet2",
                Periodo | Cliente | Texto, Periodo | Cliente, true, "Movimientos de cartera"),
            R("recuperacion-cxc", "Recuperación de cartera", "MODULO_REPORTES.RECUPERACION_DE_CARTERA",
                "Abonos aplicados y saldo posterior de cuentas por cobrar.", "bi-arrow-repeat",
                Periodo | Sucursal | Cliente | Texto, Periodo | Sucursal | Cliente),
            R("apartados", "Apartados y préstamos", "MODULO_REPORTES.APARTADOS_Y_PRESTAMOS",
                "Mercancía comprometida, vencimientos y saldos por cliente.", "bi-box2-heart",
                Periodo | Sucursal | Cliente | Texto, Periodo | Sucursal | Cliente),
        ]),
        new("caja-bancos", "Caja y bancos", "bi-cash-stack",
        [
            R("caja", "Movimientos de caja", "MODULO_REPORTES.CAJA",
                "Entradas, salidas y neto de los movimientos no anulados.", "bi-cash-stack",
                Periodo | Sucursal | Texto, Periodo | Sucursal),
            R("arqueos-cierres", "Arqueos y cierres", "MODULO_REPORTES.ARQUEOS_Y_CIERRES",
                "Arqueos, cierres y comparación entre sistema y dinero contado.", "bi-calculator",
                Periodo | Texto, Periodo),
            R("depositos", "Depósitos y cheques", "MODULO_REPORTES.DEPOSITOS",
                "Instrumentos confirmados, cobrados y pendientes de recuperación.", "bi-bank",
                Periodo | Empresa | Texto, Periodo | Empresa),
        ]),
        new("compras-pagos", "Compras y pagos", "bi-cart-check",
        [
            R("compras", "Compras y devoluciones", "MODULO_REPORTES.COMPRAS",
                "Facturas de compra, devoluciones y saldos pendientes.", "bi-cart-check",
                Periodo | Proveedor | Texto, Periodo | Proveedor),
            R("gastos", "Gastos", "MODULO_REPORTES.GASTOS",
                "Compras clasificadas como gasto operacional y su saldo.", "bi-receipt-cutoff",
                Periodo | Proveedor | Texto, Periodo | Proveedor),
            R("cuentas-por-pagar", "Cuentas por pagar", "MODULO_REPORTES.CUENTAS_POR_PAGAR",
                "Compras pendientes y antigüedad desde la fecha de compra.", "bi-credit-card-2-front",
                Periodo | Proveedor | Texto, Periodo | Proveedor),
        ]),
        new("inventario", "Inventario", "bi-box-seam",
        [
            R("inventario", "Existencias y lotes", "MODULO_REPORTES.INVENTARIO",
                "Existencia actual, lotes y valoración a costo por artículo.", "bi-box-seam",
                Articulo | Texto, Articulo),
            R("inventario-abc", "ABC y Pareto", "MODULO_REPORTES.INVENTARIO_ABC",
                "Clasificación de artículos por participación en la venta neta.", "bi-bar-chart-steps",
                Periodo | Articulo, Periodo | Articulo),
            R("rotacion-inventario", "Rotación y reorden", "MODULO_REPORTES.ROTACION_DE_INVENTARIO",
                "Días de inventario, artículos sin venta y necesidad de reorden.", "bi-arrow-repeat",
                Periodo | Articulo | Texto, Periodo | Articulo),
            R("bonificaciones", "Bonificaciones y regalías", "MODULO_REPORTES.BONIFICACIONES",
                "Unidades entregadas sin costo y costo asumido por la empresa.", "bi-gift",
                Periodo | Cliente | Articulo | Texto, Periodo | Cliente | Articulo),
            R("mermas", "Mermas", "MODULO_REPORTES.MERMAS",
                "Diferencias negativas detectadas en tomas físicas.", "bi-clipboard2-x",
                Periodo | Articulo | Bodega | Lote | Texto, Periodo | Bodega | Articulo),
        ]),
        new("lotes-trazabilidad", "Lotes y trazabilidad", "bi-upc-scan",
        [
            R("lotes", "Vencimiento de lotes", "MODULO_REPORTES.LOTES_Y_VENCIMIENTOS",
                "Lotes vigentes, próximos a vencer y vencidos.", "bi-calendar2-x",
                Periodo | Articulo | Lote | Texto, Periodo | Articulo | Lote, true, "Rango de vencimiento"),
            R("trazabilidad", "Trazabilidad por lotes", "MODULO_REPORTES.TRAZABILIDAD",
                "Kardex de entradas, salidas y existencias por lote.", "bi-upc-scan",
                Periodo | Articulo | Bodega | Lote | Texto, Periodo | Articulo | Bodega | Lote),
            R("empaquetado", "Empaquetado y maquila", "MODULO_REPORTES.EMPAQUETADO_Y_MAQUILA",
                "Productos ensamblados, combos y maquila registrados.", "bi-boxes",
                Periodo | Texto, Periodo),
        ]),
        new("cumplimiento", "Cumplimiento y auditoría", "bi-shield-check",
        [
            R("cabys", "Cumplimiento CABYS", "MODULO_REPORTES.CUMPLIMIENTO_CABYS",
                "Cobertura y validez del código CABYS en artículos activos.", "bi-patch-check",
                Articulo | Texto, Articulo, false),
            R("auditoria", "Auditoría", "MODULO_REPORTES.AUDITORIA",
                "Cambios registrados por usuario, acción y documento.", "bi-shield-check",
                Periodo | Texto, Periodo),
        ]),
    ];

    public static IReadOnlyList<ReporteCatalogo> Todos { get; } =
        Grupos.SelectMany(g => g.Opciones).ToArray();

    public static ReporteCatalogo? Buscar(string tipo)
        => Todos.FirstOrDefault(r => string.Equals(r.Tipo, tipo, StringComparison.OrdinalIgnoreCase));

    public static GrupoReportes? GrupoDe(string tipo)
        => Grupos.FirstOrDefault(g => g.Opciones.Any(r => string.Equals(r.Tipo, tipo, StringComparison.OrdinalIgnoreCase)));

    public static string NombreFiltro(FiltroReporteVisible filtro) => filtro switch
    {
        FiltroReporteVisible.Periodo => "Periodo",
        FiltroReporteVisible.Texto => "Búsqueda libre",
        FiltroReporteVisible.Sucursal => "Sucursal",
        FiltroReporteVisible.Empresa => "Empresa",
        FiltroReporteVisible.Cliente => "Cliente",
        FiltroReporteVisible.Proveedor => "Proveedor",
        FiltroReporteVisible.Articulo => "Artículo",
        FiltroReporteVisible.Bodega => "Bodega",
        FiltroReporteVisible.Lote => "Lote",
        FiltroReporteVisible.Agente => "Agente",
        FiltroReporteVisible.Familia => "Familia",
        FiltroReporteVisible.TipoVenta => "Tipo de venta",
        FiltroReporteVisible.IncluirAnuladas => "Anuladas",
        FiltroReporteVisible.EstadoLiquidacion => "Liquidación",
        FiltroReporteVisible.RutaComercial => "Ruta comercial",
        FiltroReporteVisible.BeneficiarioComision => "Beneficiario",
        FiltroReporteVisible.Corte => "N° de corte",
        _ => string.Empty,
    };

    public static IEnumerable<string> NombresFiltros(ReporteCatalogo reporte)
    {
        foreach (var filtro in Enum.GetValues<FiltroReporteVisible>())
        {
            if (filtro != FiltroReporteVisible.Ninguno && reporte.Tiene(filtro))
                yield return NombreFiltro(filtro);
        }
    }

    private static ReporteCatalogo R(
        string tipo,
        string titulo,
        string codigo,
        string descripcion,
        string icono,
        FiltroReporteVisible filtros,
        FiltroReporteVisible filtrosBasicos,
        bool mostrarGraficas = true,
        string etiquetaPeriodo = "Periodo")
        => new(tipo, titulo, codigo, descripcion, icono, filtros, filtrosBasicos, mostrarGraficas, etiquetaPeriodo);
}
