namespace SuvesaPosSitioAplicacion.Views.Reportes;

/// <summary>Un dominio de reportes (una tarjeta en el catálogo de <c>ModuloReportes.razor</c>,
/// una pestaña de dominio dentro de <c>ReporteOperacionPanel</c>).</summary>
public sealed record GrupoReportes(string Id, string Nombre, string Icono, (string Valor, string Texto, string Codigo)[] Opciones);

/// <summary>Catálogo único de reportes agrupados por dominio, compartido entre el catálogo
/// de entrada (<c>ModuloReportes.razor</c>) y el selector rápido dentro de cada reporte
/// (<c>ReporteOperacionPanel</c>) — antes vivía duplicado en el propio panel.</summary>
public static class CatalogoReportes
{
    // El Codigo de cada opción es la función de MenuSeePos.cs (MODULO_REPORTES.*) que le
    // corresponde — generado por tools/anotar_codigos_menu.py. "comisiones" reutiliza el
    // permiso propio de esa pantalla (VENTAS.COMISIONES), no uno de Reportes, porque ya
    // tenía su menú y su controlador aparte.
    public static readonly GrupoReportes[] Grupos =
    [
        new("gerencial", "Panel ejecutivo", "bi-speedometer2",
        [
            ("panel-ejecutivo", "KPIs consolidados", "MODULO_REPORTES.PANEL_EJECUTIVO"),
        ]),
        new("ventas-cartera", "Ventas y cartera", "bi-receipt",
        [
            ("ventas", "Ventas", "MODULO_REPORTES.VENTAS"),
            ("ventas-detalle", "Ventas detalladas", "MODULO_REPORTES.VENTAS_DETALLE"),
            ("ventas-horas", "Ventas entre horas", "MODULO_REPORTES.VENTAS_ENTRE_HORAS"),
            ("clientes", "Clientes y comportamiento", "MODULO_REPORTES.COMPORTAMIENTO_DE_CLIENTES"),
            ("rentabilidad", "Rentabilidad", "MODULO_REPORTES.RENTABILIDAD"),
            ("ventas-compras", "Ventas vs. compras", "MODULO_REPORTES.VENTAS_VS_COMPRAS"),
            ("cuentas-por-cobrar", "CxC y antigüedad", "MODULO_REPORTES.CUENTAS_POR_COBRAR"),
            ("recuperacion-cxc", "Recuperación CxC", "MODULO_REPORTES.RECUPERACION_DE_CARTERA"),
            ("cabys", "Cumplimiento CABYS", "MODULO_REPORTES.CUMPLIMIENTO_CABYS"),
            ("apartados", "Apartados y préstamos", "MODULO_REPORTES.APARTADOS_Y_PRESTAMOS"),
            ("kpi-rutas", "KPI por ruta", "MODULO_REPORTES.KPI_POR_RUTA_COMERCIAL"),
            ("comisiones", "Comisiones", "VENTAS.COMISIONES"),
        ]),
        new("caja-bancos", "Caja y bancos", "bi-cash-stack",
        [
            ("caja", "Movimientos", "MODULO_REPORTES.CAJA"),
            ("arqueos-cierres", "Arqueos y cierres", "MODULO_REPORTES.ARQUEOS_Y_CIERRES"),
            ("depositos", "Depósitos", "MODULO_REPORTES.DEPOSITOS"),
        ]),
        new("compras", "Compras", "bi-cart-check",
        [
            ("compras", "Compras y devoluciones", "MODULO_REPORTES.COMPRAS"),
            ("gastos", "Gastos", "MODULO_REPORTES.GASTOS"),
            ("cuentas-por-pagar", "Abonos CxP", "MODULO_REPORTES.CUENTAS_POR_PAGAR"),
        ]),
        new("inventario", "Inventario", "bi-box-seam",
        [
            ("inventario", "Existencias y lotes", "MODULO_REPORTES.INVENTARIO"),
            ("inventario-abc", "ABC y Pareto", "MODULO_REPORTES.INVENTARIO_ABC"),
            ("rotacion-inventario", "Rotación y reorden", "MODULO_REPORTES.ROTACION_DE_INVENTARIO"),
            ("lotes", "Vencimiento de lotes", "MODULO_REPORTES.LOTES_Y_VENCIMIENTOS"),
            ("trazabilidad", "Trazabilidad por lotes", "MODULO_REPORTES.TRAZABILIDAD"),
            ("bonificaciones", "Bonificaciones y regalías", "MODULO_REPORTES.BONIFICACIONES"),
            ("empaquetado", "Empaquetado y maquila", "MODULO_REPORTES.EMPAQUETADO_Y_MAQUILA"),
            ("mermas", "Mermas", "MODULO_REPORTES.MERMAS"),
            ("auditoria", "Auditoría", "MODULO_REPORTES.AUDITORIA"),
        ]),
    ];
}
