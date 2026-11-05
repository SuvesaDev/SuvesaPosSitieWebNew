using ClosedXML.Excel;
using SuvesaPosSitioAplicacion.DTOs.Reportes;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>Excel del IVA repercutido: hoja de resumen por tarifa (con bases por tipo de operación) y hoja de detalle por documento.</summary>
public static class GeneradorIvaExcel
{
    private const string Verde = "#2F6B1F";

    public static byte[] IvaRepercutido(IvaRepercutidoResultado r, DateTime generadoEnEquipo)
    {
        using var libro = new XLWorkbook();

        var h = libro.Worksheets.Add("Resumen");
        h.Cell(1, 1).Value = "IVA Repercutido";
        h.Cell(1, 1).Style.Font.Bold = true; h.Cell(1, 1).Style.Font.FontSize = 16; h.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(Verde);
        h.Cell(2, 1).Value = $"{r.Emisor} · {r.Identificacion}";
        h.Cell(3, 1).Value = $"Período: {r.Desde:dd/MM/yyyy} al {r.Hasta:dd/MM/yyyy} · montos en colones";
        h.Cell(4, 1).Value = $"Generado en el equipo: {generadoEnEquipo:dd/MM/yyyy HH:mm}";
        h.Range(2, 1, 4, 1).Style.Font.FontColor = XLColor.Gray;

        var fila = 6;
        string[] cab = { "Código tarifa", "Tarifa", "%", "Tipo de operación", "Base ventas", "IVA ventas", "Base N/C", "IVA N/C", "Base neta", "IVA neto" };
        for (var i = 0; i < cab.Length; i++) h.Cell(fila, i + 1).Value = cab[i];
        EstiloCabecera(h.Range(fila, 1, fila, cab.Length));
        foreach (var t in r.Tarifas)
        {
            fila++;
            h.Cell(fila, 1).Value = t.CodigoTarifa; h.Cell(fila, 2).Value = t.Descripcion; h.Cell(fila, 3).Value = t.Porcentaje; h.Cell(fila, 4).Value = Tipo(t.TipoOperacion);
            h.Cell(fila, 5).Value = t.BaseVentas; h.Cell(fila, 6).Value = t.IvaVentas; h.Cell(fila, 7).Value = t.BaseNotasCredito; h.Cell(fila, 8).Value = t.IvaNotasCredito;
            h.Cell(fila, 9).Value = t.BaseNeta; h.Cell(fila, 10).Value = t.IvaNeto;
        }
        fila++;
        h.Cell(fila, 2).Value = "TOTAL";
        h.Cell(fila, 9).Value = r.TotalBaseNeta; h.Cell(fila, 10).Value = r.TotalIvaNeto;
        h.Range(fila, 1, fila, cab.Length).Style.Font.Bold = true;
        h.Range(7, 5, fila, 10).Style.NumberFormat.Format = "#,##0.00";

        fila += 2;
        h.Cell(fila, 1).Value = "Bases netas por tipo de operación (insumo de la prorrata)"; h.Cell(fila, 1).Style.Font.Bold = true;
        void Linea(string etiqueta, decimal valor) { fila++; h.Cell(fila, 2).Value = etiqueta; h.Cell(fila, 9).Value = valor; h.Cell(fila, 9).Style.NumberFormat.Format = "#,##0.00"; }
        Linea("Gravadas", r.Operaciones.BaseGravada);
        Linea("Exentas con derecho a crédito", r.Operaciones.BaseExentaConDerecho);
        Linea("Exentas sin derecho a crédito", r.Operaciones.BaseExentaSinDerecho);
        Linea("No sujetas", r.Operaciones.BaseNoSujeta);
        Linea("Sin clasificar", r.Operaciones.BaseSinClasificar);
        Linea("Informativo: base exonerada", r.Operaciones.BaseExonerada);
        fila++;
        Linea("IVA según encabezados de factura (cuadre)", r.IvaSegunEncabezados);
        Linea("IVA de bonificaciones asumido por el emisor (aparte)", r.IvaBonificacionesAsumido);

        if (r.Advertencias.Count > 0)
        {
            fila += 2;
            h.Cell(fila, 1).Value = "Advertencias"; h.Cell(fila, 1).Style.Font.Bold = true;
            foreach (var a in r.Advertencias) { fila++; h.Cell(fila, 1).Value = a; }
        }
        h.Columns().AdjustToContents(1, 10);

        var d = libro.Worksheets.Add("Detalle");
        string[] cabD = { "Fecha", "Tipo", "Documento", "Clave", "Cliente", "Código tarifa", "%", "Base", "IVA", "Moneda extranjera" };
        for (var i = 0; i < cabD.Length; i++) d.Cell(1, i + 1).Value = cabD[i];
        EstiloCabecera(d.Range(1, 1, 1, cabD.Length));
        var f = 1;
        foreach (var x in r.Detalle)
        {
            f++;
            d.Cell(f, 1).Value = x.Fecha.ToString("dd/MM/yyyy"); d.Cell(f, 2).Value = x.Tipo; d.Cell(f, 3).Value = x.Documento; d.Cell(f, 4).Value = x.Clave;
            d.Cell(f, 5).Value = x.Cliente; d.Cell(f, 6).Value = x.CodigoTarifa; d.Cell(f, 7).Value = x.Porcentaje; d.Cell(f, 8).Value = x.Base; d.Cell(f, 9).Value = x.Iva;
            d.Cell(f, 10).Value = x.MonedaExtranjera ? "Sí" : "";
        }
        if (f > 1) d.Range(2, 8, f, 9).Style.NumberFormat.Format = "#,##0.00";
        d.SheetView.FreezeRows(1);
        d.Columns().AdjustToContents(1, Math.Min(f, 200));

        using var ms = new MemoryStream();
        libro.SaveAs(ms);
        return ms.ToArray();
    }

    /// <summary>CSV del detalle (separador coma, importes con punto, texto entre comillas).</summary>
    public static string IvaRepercutidoCsv(IvaRepercutidoResultado r)
    {
        static string Q(string? t) => "\"" + (t ?? "").Replace("\"", "\"\"") + "\"";
        static string N(decimal v) => v.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        var lineas = new List<string> { "Fecha,Tipo,Documento,Clave,Cliente,Código tarifa,Porcentaje,Base,IVA,Moneda extranjera" };
        lineas.AddRange(r.Detalle.Select(x => string.Join(',', x.Fecha.ToString("yyyy-MM-dd"), Q(x.Tipo), Q(x.Documento), Q(x.Clave), Q(x.Cliente), Q(x.CodigoTarifa),
            x.Porcentaje.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture), N(x.Base), N(x.Iva), x.MonedaExtranjera ? "Sí" : "No")));
        return string.Join("\n", lineas);
    }

    public static byte[] IvaSoportado(IvaSoportadoResultado r, DateTime generadoEnEquipo)
    {
        using var libro = new XLWorkbook();
        var h = libro.Worksheets.Add("Resumen");
        h.Cell(1, 1).Value = "IVA Soportado";
        h.Cell(1, 1).Style.Font.Bold = true; h.Cell(1, 1).Style.Font.FontSize = 16; h.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(Verde);
        h.Cell(2, 1).Value = $"{r.Emisor} · {r.Identificacion}";
        h.Cell(3, 1).Value = $"Período: {r.Desde:dd/MM/yyyy} al {r.Hasta:dd/MM/yyyy} · montos en colones";
        h.Cell(4, 1).Value = $"Generado en el equipo: {generadoEnEquipo:dd/MM/yyyy HH:mm}";
        h.Range(2, 1, 4, 1).Style.Font.FontColor = XLColor.Gray;

        var fila = 6;
        string[] cab = { "Origen", "Documentos", "IVA total", "Acreditable", "Sujeto a prorrata", "No acreditable", "Pendiente de Mensaje Receptor" };
        for (var i = 0; i < cab.Length; i++) h.Cell(fila, i + 1).Value = cab[i];
        EstiloCabecera(h.Range(fila, 1, fila, cab.Length));
        foreach (var o in r.PorOrigen)
        {
            fila++;
            h.Cell(fila, 1).Value = o.Origen; h.Cell(fila, 2).Value = o.Documentos; h.Cell(fila, 3).Value = o.IvaTotal; h.Cell(fila, 4).Value = o.IvaAcreditable;
            h.Cell(fila, 5).Value = o.IvaSujetoProrrata; h.Cell(fila, 6).Value = o.IvaNoAcreditable; h.Cell(fila, 7).Value = o.IvaPendiente;
        }
        h.Range(7, 3, fila, 7).Style.NumberFormat.Format = "#,##0.00";

        fila += 2;
        void Linea(string etiqueta, decimal valor, bool negrita = false)
        {
            fila++; h.Cell(fila, 1).Value = etiqueta; h.Cell(fila, 4).Value = valor; h.Cell(fila, 4).Style.NumberFormat.Format = "#,##0.00";
            if (negrita) h.Range(fila, 1, fila, 4).Style.Font.Bold = true;
        }
        Linea("IVA soportado total (sin devoluciones)", r.IvaTotal);
        Linea("IVA acreditable", r.IvaAcreditable);
        Linea("   de ello, bienes de capital", r.IvaBienesCapital);
        Linea("Menos: devoluciones de compra", r.IvaDevolucionesCompra);
        Linea("IVA acreditable neto", r.IvaAcreditableNeto, true);
        Linea("IVA sujeto a prorrata (condición 05)", r.IvaSujetoProrrata);
        Linea("IVA no acreditable", r.IvaNoAcreditable);
        Linea("IVA pendiente de Mensaje Receptor", r.IvaPendiente);

        if (r.Advertencias.Count > 0)
        {
            fila += 2;
            h.Cell(fila, 1).Value = "Advertencias"; h.Cell(fila, 1).Style.Font.Bold = true;
            foreach (var a in r.Advertencias) { fila++; h.Cell(fila, 1).Value = a; }
        }
        h.Columns().AdjustToContents(1, 7);

        var d = libro.Worksheets.Add("Detalle");
        string[] cabD = { "Fecha", "Origen", "Documento", "Proveedor", "Clave", "Condición", "Estado", "IVA total", "Acreditable", "Sujeto a prorrata", "No acreditable", "Pendiente", "Moneda extranjera" };
        for (var i = 0; i < cabD.Length; i++) d.Cell(1, i + 1).Value = cabD[i];
        EstiloCabecera(d.Range(1, 1, 1, cabD.Length));
        var f = 1;
        foreach (var x in r.Detalle)
        {
            f++;
            d.Cell(f, 1).Value = x.Fecha.ToString("dd/MM/yyyy"); d.Cell(f, 2).Value = x.Origen; d.Cell(f, 3).Value = x.Documento; d.Cell(f, 4).Value = x.Proveedor;
            d.Cell(f, 5).Value = x.Clave; d.Cell(f, 6).Value = x.Condicion; d.Cell(f, 7).Value = x.Estado; d.Cell(f, 8).Value = x.IvaTotal; d.Cell(f, 9).Value = x.IvaAcreditable;
            d.Cell(f, 10).Value = x.IvaSujetoProrrata; d.Cell(f, 11).Value = x.IvaNoAcreditable; d.Cell(f, 12).Value = x.IvaPendiente; d.Cell(f, 13).Value = x.MonedaExtranjera ? "Sí" : "";
        }
        if (f > 1) d.Range(2, 8, f, 12).Style.NumberFormat.Format = "#,##0.00";
        d.SheetView.FreezeRows(1);
        d.Columns().AdjustToContents(1, Math.Min(f, 200));

        using var ms = new MemoryStream();
        libro.SaveAs(ms);
        return ms.ToArray();
    }

    public static string IvaSoportadoCsv(IvaSoportadoResultado r)
    {
        static string Q(string? t) => "\"" + (t ?? "").Replace("\"", "\"\"") + "\"";
        static string N(decimal v) => v.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        var lineas = new List<string> { "Fecha,Origen,Documento,Proveedor,Clave,Condición,Estado,IVA total,Acreditable,Sujeto a prorrata,No acreditable,Pendiente,Moneda extranjera" };
        lineas.AddRange(r.Detalle.Select(x => string.Join(',', x.Fecha.ToString("yyyy-MM-dd"), Q(x.Origen), Q(x.Documento), Q(x.Proveedor), Q(x.Clave), Q(x.Condicion), Q(x.Estado),
            N(x.IvaTotal), N(x.IvaAcreditable), N(x.IvaSujetoProrrata), N(x.IvaNoAcreditable), N(x.IvaPendiente), x.MonedaExtranjera ? "Sí" : "No")));
        return string.Join("\n", lineas);
    }

    private static readonly string[] Meses = { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Setiembre", "Octubre", "Noviembre", "Diciembre" };
    public static string NombreMes(int mes) => mes is >= 1 and <= 12 ? Meses[mes] : mes.ToString();

    public static byte[] Prorrata(IvaProrrataResultado r, DateTime generadoEnEquipo)
    {
        using var libro = new XLWorkbook();
        var h = libro.Worksheets.Add("Prorrata");
        h.Cell(1, 1).Value = "Prorrata del IVA";
        h.Cell(1, 1).Style.Font.Bold = true; h.Cell(1, 1).Style.Font.FontSize = 16; h.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(Verde);
        h.Cell(2, 1).Value = $"{r.Emisor} · {r.Identificacion}";
        h.Cell(3, 1).Value = $"Año {r.Anio}, de enero a {NombreMes(r.HastaMes).ToLowerInvariant()} · montos en colones";
        h.Cell(4, 1).Value = $"Generado en el equipo: {generadoEnEquipo:dd/MM/yyyy HH:mm}";
        h.Range(2, 1, 4, 1).Style.Font.FontColor = XLColor.Gray;

        var fila = 6;
        string[] cab = { "Mes", "Base con derecho", "Base sin derecho", "Base no sujeta", "IVA sujeto a prorrata", "Crédito provisional" };
        for (var i = 0; i < cab.Length; i++) h.Cell(fila, i + 1).Value = cab[i];
        EstiloCabecera(h.Range(fila, 1, fila, cab.Length));
        foreach (var m in r.Meses)
        {
            fila++;
            h.Cell(fila, 1).Value = NombreMes(m.Mes); h.Cell(fila, 2).Value = m.BaseConDerecho; h.Cell(fila, 3).Value = m.BaseSinDerecho; h.Cell(fila, 4).Value = m.BaseNoSujeta;
            h.Cell(fila, 5).Value = m.IvaSujetoProrrata; h.Cell(fila, 6).Value = m.CreditoProvisional;
        }
        fila++;
        h.Cell(fila, 1).Value = "TOTAL";
        h.Cell(fila, 2).Value = r.BaseConDerecho; h.Cell(fila, 3).Value = r.BaseSinDerecho; h.Cell(fila, 4).Value = r.BaseNoSujeta;
        h.Cell(fila, 5).Value = r.IvaSujetoProrrataAcumulado; h.Cell(fila, 6).Value = r.CreditoProvisionalAcumulado;
        h.Range(fila, 1, fila, cab.Length).Style.Font.Bold = true;
        h.Range(7, 2, fila, 6).Style.NumberFormat.Format = "#,##0.00";

        fila += 2;
        void Linea(string etiqueta, string valor, bool negrita = false) { fila++; h.Cell(fila, 1).Value = etiqueta; h.Cell(fila, 4).Value = valor; if (negrita) h.Range(fila, 1, fila, 4).Style.Font.Bold = true; }
        Linea("¿Aplica prorrata?", r.Aplica ? "Sí" : "No (100%)");
        Linea("Porcentaje provisional", $"{r.PorcentajeProvisional:0.##}% — {r.OrigenProvisional}");
        Linea("Porcentaje acumulado al mes", $"{r.PorcentajeAcumulado:0.##}%");
        Linea("Crédito según el acumulado (estimación)", r.CreditoSegunAcumulado.ToString("#,##0.00"));
        if (r.PorcentajeDefinitivo is { } def)
        {
            Linea("Porcentaje definitivo", $"{def:0.##}% — {r.OrigenDefinitivo}");
            Linea("Crédito definitivo", (r.CreditoDefinitivo ?? 0).ToString("#,##0.00"));
            Linea("Regularización (+ crédito adicional / − devolver)", (r.Regularizacion ?? 0).ToString("#,##0.00"), true);
        }
        if (r.Advertencias.Count > 0)
        {
            fila += 2;
            h.Cell(fila, 1).Value = "Advertencias"; h.Cell(fila, 1).Style.Font.Bold = true;
            foreach (var a in r.Advertencias) { fila++; h.Cell(fila, 1).Value = a; }
        }
        h.Columns().AdjustToContents(1, 6);
        using var ms = new MemoryStream();
        libro.SaveAs(ms);
        return ms.ToArray();
    }

    public static string ProrrataCsv(IvaProrrataResultado r)
    {
        static string N(decimal v) => v.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        var lineas = new List<string> { "Mes,Base con derecho,Base sin derecho,Base no sujeta,IVA sujeto a prorrata,Crédito provisional" };
        lineas.AddRange(r.Meses.Select(m => string.Join(',', NombreMes(m.Mes), N(m.BaseConDerecho), N(m.BaseSinDerecho), N(m.BaseNoSujeta), N(m.IvaSujetoProrrata), N(m.CreditoProvisional))));
        lineas.Add(string.Join(',', "Total", N(r.BaseConDerecho), N(r.BaseSinDerecho), N(r.BaseNoSujeta), N(r.IvaSujetoProrrataAcumulado), N(r.CreditoProvisionalAcumulado)));
        return string.Join("\n", lineas);
    }

    public static string Tipo(string t) => t switch
    {
        "GRAVADA" => "Gravada",
        "EXENTA_CON_DERECHO" => "Exenta con derecho a crédito",
        "EXENTA_SIN_DERECHO" => "Exenta sin derecho a crédito",
        "NO_SUJETA" => "No sujeta",
        "SIN_CLASIFICAR" => "Sin clasificar",
        _ => t,
    };

    private static void EstiloCabecera(IXLRange rango)
    {
        rango.Style.Font.Bold = true;
        rango.Style.Font.FontColor = XLColor.White;
        rango.Style.Fill.BackgroundColor = XLColor.FromHtml(Verde);
    }
}
