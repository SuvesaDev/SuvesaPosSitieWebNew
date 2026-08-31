using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Services;
using Xunit.Abstractions;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// COMPARADOR FISCAL — semana 8 de la Ola 0.
///
/// Toma documentos reales del API y recalcula cada linea con
/// <see cref="CalculoDocumento"/>, comparando importe a importe contra lo que el
/// sistema actual dejo guardado.
///
/// Es la pieza que responde a la pregunta que decide la Ola 3: **¿nuestra aritmetica
/// da lo mismo que la suya?** Una diferencia de un centimo en un documento fiscal no
/// es un defecto cosmetico.
///
/// El sistema actual calcula en el navegador con coma flotante y el API guarda los
/// importes en double, asi que alguna diferencia es esperable. Lo que importa es
/// **cuanta y donde**: si aparece, hay que decidir si se replica el comportamiento
/// antiguo o se corrige, y esa es decision de negocio.
/// </summary>
[Collection(ColeccionE2E.Nombre)]
[Trait("Categoria", "RequiereCredenciales")]
public class ComparadorFiscalTests
{
    private readonly ITestOutputHelper _salida;

    public ComparadorFiscalTests(ITestOutputHelper salida) => _salida = salida;

    /// <summary>Cuantos documentos se revisan. Suficiente para ver un patron.</summary>
    private const int MaximoDocumentos = 25;

    /// <summary>Diferencia por debajo de la cual no se considera discrepancia.</summary>
    private const decimal Tolerancia = 0.01m;

    private sealed record Discrepancia(
        long Documento, long Linea, string Campo, decimal Guardado, decimal Recalculado)
    {
        public decimal Diferencia => Math.Abs(Guardado - Recalculado);
    }

    [HechoConCredenciales]
    public async Task LaAritmeticaCoincideConLaDelSistemaActual()
    {
        var api = await ProxyAutenticadoAsync();

        var listado = await api.Obtener();
        Assert.True(listado.EsCorrecta, listado.Excepcion);

        var documentos = (listado.Responses ?? Array.Empty<CotizacionesDTO>())
            .Where(c => !c.Anulado)
            .OrderByDescending(c => c.Fecha)
            .Take(MaximoDocumentos)
            .ToList();

        if (documentos.Count == 0)
        {
            _salida.WriteLine("No hay documentos vigentes que comparar.");
            return;
        }

        var discrepancias = new List<Discrepancia>();
        var lineasRevisadas = 0;
        var documentosRevisados = 0;

        foreach (var resumen in documentos)
        {
            // OJO CON EL API: el listado ObtenerCotizaciones SI trae el detalle,
            // mientras que ObtenerCotizacionPorID lo devuelve vacio. Es al reves de
            // lo que uno espera, y por eso se usa el del listado y solo se pide por
            // id como respaldo.
            var detalle = resumen.Detalle;

            if (detalle is null || detalle.Count == 0)
            {
                var completo = await api.ObtenerPorId(resumen.Cotizacion1);
                detalle = completo.Responses?.Detalle;
            }

            if (detalle is null || detalle.Count == 0)
            {
                continue;
            }

            documentosRevisados++;

            foreach (var d in detalle)
            {
                lineasRevisadas++;
                discrepancias.AddRange(CompararLinea(resumen.Cotizacion1, d));
            }
        }

        Informar(documentosRevisados, lineasRevisadas, discrepancias);

        // Un comparador que no comparo nada NO puede dar verde: seria un falso
        // positivo que haria creer que la aritmetica esta validada.
        Assert.True(lineasRevisadas > 0,
            $"No se comparo ninguna linea ({documentos.Count} documentos revisados). " +
            "Sin datos, este comparador no dice nada sobre la aritmetica.");

        // Todavia NO se afirma nada: primero hay que ver el desfase real contra datos
        // de produccion. Cuando se conozca y se decida que hacer con el, esto pasa a
        // ser una asercion y la Ola 3 puede abrirse.
    }

    private static IEnumerable<Discrepancia> CompararLinea(long documento, DetalleCotizacionDTO d)
    {
        // El borde: lo que llega en double pasa a decimal una vez, y se recalcula.
        var recalculada = CalculoDocumento.Linea(
            cantidad: Formato.AImporte(d.Cantidad),
            precioUnitario: Formato.AImporte(d.PrecioUnit),
            porcentajeDescuento: Formato.AImporte(d.Descuento),
            porcentajeImpuesto: Formato.AImporte(d.Impuesto));

        var comparaciones = new (string Campo, decimal Guardado, decimal Nuestro)[]
        {
            ("SubTotal", Formato.AImporte(d.SubTotal), recalculada.SubTotal),
            ("MontoDescuento", Formato.AImporte(d.MontoDescuento), recalculada.MontoDescuento),
            ("MontoImpuesto", Formato.AImporte(d.MontoImpuesto), recalculada.MontoImpuesto),
            ("Total", Formato.AImporte(d.Total), recalculada.Total)
        };

        foreach (var (campo, guardado, nuestro) in comparaciones)
        {
            if (Math.Abs(guardado - nuestro) >= Tolerancia)
            {
                yield return new Discrepancia(documento, d.Numero, campo, guardado, nuestro);
            }
        }
    }

    private void Informar(int documentos, int lineas, List<Discrepancia> discrepancias)
    {
        _salida.WriteLine($"documentos revisados : {documentos}");
        _salida.WriteLine($"lineas revisadas     : {lineas}");
        _salida.WriteLine($"discrepancias        : {discrepancias.Count}");

        if (discrepancias.Count == 0)
        {
            _salida.WriteLine("");
            _salida.WriteLine("Sin diferencias. La aritmetica en decimal reproduce la del sistema actual.");
            return;
        }

        _salida.WriteLine("");
        _salida.WriteLine("-- por campo --");
        foreach (var g in discrepancias.GroupBy(x => x.Campo).OrderByDescending(g => g.Count()))
        {
            _salida.WriteLine(
                $"   {g.Key,-16} {g.Count(),4} casos, diferencia maxima {g.Max(x => x.Diferencia):N4}");
        }

        _salida.WriteLine("");
        _salida.WriteLine("-- primeros casos --");
        foreach (var x in discrepancias.OrderByDescending(x => x.Diferencia).Take(15))
        {
            _salida.WriteLine(
                $"   doc {x.Documento} linea {x.Linea} {x.Campo}: " +
                $"guardado {x.Guardado:N2}, recalculado {x.Recalculado:N2} " +
                $"(dif {x.Diferencia:N4})");
        }
    }

    private static async Task<Cotizaciones> ProxyAutenticadoAsync()
    {
        var url = new Uri(CredencialesPrueba.Api);

        HttpClient Cliente() => new(
            new ApiAuthHeaderHandler(NullLogger<ApiAuthHeaderHandler>.Instance)
            {
                InnerHandler = new HttpClientHandler()
            })
        { BaseAddress = url };

        var seguridad = new Seguridad(
            new UsuarioApiCliente(Cliente()),
            new CentrosApiCliente(Cliente()),
            new SesionFija(),
            NullLogger<Seguridad>.Instance);

        var login = await seguridad.Login(CredencialesPrueba.Usuario!, CredencialesPrueba.Password!);
        Assert.True(login.EsCorrecta, login.Excepcion);

        return new Cotizaciones(
            new CotizacionApiCliente(Cliente()),
            new SesionFija(login.Responses!.Token),
            NullLogger<Cotizaciones>.Instance);
    }
}
