using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>Contrato de los controles de integridad contable contra el API: rutas, filtros, cuerpo y lectura de la respuesta.</summary>
public sealed class IntegridadContableProxyTests
{
    private sealed class Captura(string json) : HttpMessageHandler
    {
        public HttpMethod? Metodo { get; private set; }
        public string? Ruta { get; private set; }
        public string? Cuerpo { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Metodo = request.Method;
            Ruta = request.RequestUri?.PathAndQuery;
            Cuerpo = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        }
    }

    private static (Contabilidad Proxy, Captura Handler) Nuevo(string json)
    {
        var handler = new Captura(json);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://seepos.test/") };
        return (new Contabilidad(new ContabilidadApiCliente(http), new SesionFalsa(), NullLogger<Contabilidad>.Instance), handler);
    }

    [Fact]
    public async Task DocumentosSinEvento_PideElLibroYElRangoYLeeLaLista()
    {
        var (proxy, handler) = Nuevo("""
            {"status":0,"responses":{"idLibroContable":7,"desde":"2026-09-01","hasta":"2026-11-05","total":1,
             "documentos":[{"tipo":"Venta","idDocumento":60,"numero":"7","fecha":"2026-11-02T19:54:25","total":5650.5,"idSucursal":3,"motivo":"Aceptada por Hacienda y sin evento contable."}]}}
            """);

        var r = await proxy.DocumentosSinEvento(7, new DateOnly(2026, 10, 1), new DateOnly(2026, 11, 5));

        Assert.True(r.EsCorrecta);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal("/api/contabilidad/integridad/documentos-sin-evento?idLibroContable=7&desde=2026-10-01&hasta=2026-11-05", handler.Ruta);
        var documento = Assert.Single(r.Responses!.Documentos);
        Assert.Equal(("Venta", 60L, 5650.5m), (documento.Tipo, documento.IdDocumento, documento.Total));
        Assert.Equal(1, r.Responses.Total);
    }

    [Fact]
    public async Task RegularizarDocumentos_EnviaElRangoPorPostYLeeElResultado()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":{"regularizados":2,"sinCambio":0,"errores":[]}}""");

        var r = await proxy.RegularizarDocumentos(7);

        Assert.True(r.EsCorrecta);
        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/contabilidad/integridad/regularizar-documentos", handler.Ruta);
        Assert.Contains("\"idLibroContable\":7", handler.Cuerpo);
        Assert.Equal(2, r.Responses!.Regularizados);
    }

    [Fact]
    public async Task DiagnosticarPlantillas_LeeLosProblemasConSuGravedad()
    {
        var (proxy, handler) = Nuevo("""
            {"status":0,"responses":{"idLibroContable":7,"fecha":"2026-11-05","eventosEnErrorConfiguracion":1,"sano":false,
             "problemas":[{"tipoEvento":"CompraRegistrada","gravedad":"Error","detalle":"Ninguna versión activa cubre las fechas anteriores al 2026-12-01."}]}}
            """);

        var r = await proxy.DiagnosticarPlantillas(7);

        Assert.Equal("/api/contabilidad/integridad/plantillas?idLibroContable=7", handler.Ruta);
        Assert.False(r.Responses!.Sano);
        var problema = Assert.Single(r.Responses.Problemas);
        Assert.Equal(("CompraRegistrada", "Error"), (problema.TipoEvento, problema.Gravedad));
    }

    [Fact]
    public async Task ReencolarErroresConfiguracion_EnviaLibroYTipo()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":{"reencolados":3}}""");

        var r = await proxy.ReencolarErroresConfiguracion(7, "CompraRegistrada");

        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/contabilidad/eventos/reintentar-configuracion", handler.Ruta);
        Assert.Contains("\"tipoEvento\":\"CompraRegistrada\"", handler.Cuerpo);
        Assert.Equal(3, r.Responses!.Reencolados);
    }

    [Fact]
    public async Task DuplicarVersionPlantilla_EnviaLaVigenciaPorPostALaRutaDeLaVersion()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":{"id":9,"idPlantillaContable":3,"version":3,"prioridad":1,"estado":"Borrador","efectivaDesde":"2026-09-30T00:00:00+00:00"}}""");

        var r = await proxy.DuplicarVersionPlantilla(5, new DateOnly(2026, 9, 30));

        Assert.True(r.EsCorrecta);
        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/contabilidad/plantillas/versiones/5/duplicar", handler.Ruta);
        Assert.Contains("\"efectivaDesde\":\"2026-09-30\"", handler.Cuerpo);
        Assert.Equal((3, "Borrador"), (r.Responses!.Version, r.Responses.Estado.ToString()));
    }

    [Fact]
    public async Task FijarPuestaEnMarcha_EnviaLaFechaPorPutALaRutaDeIntegridad()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":true}""");

        var r = await proxy.FijarPuestaEnMarcha(7, new DateOnly(2026, 9, 30));

        Assert.True(r.EsCorrecta);
        Assert.Equal(HttpMethod.Put, handler.Metodo);
        Assert.Equal("/api/contabilidad/integridad/puesta-en-marcha", handler.Ruta);
        Assert.Contains("\"fecha\":\"2026-09-30\"", handler.Cuerpo);
    }

    [Fact]
    public async Task ComprasRechazadas_PideElLibroYLeeLaLista()
    {
        var (proxy, handler) = Nuevo("""
            {"status":0,"responses":{"idLibroContable":7,"desde":"2026-09-01","hasta":"2026-11-05","total":1,
             "documentos":[{"tipo":"Compra","idDocumento":14,"numero":"F-14","fecha":"2026-11-02T00:00:00","total":1130.0,"idSucursal":3,"motivo":"Hacienda rechazó el Mensaje Receptor: la compra no está en el mayor."}]}}
            """);

        var r = await proxy.ComprasRechazadas(7);

        Assert.Equal("/api/contabilidad/integridad/compras-rechazadas?idLibroContable=7", handler.Ruta);
        var compra = Assert.Single(r.Responses!.Documentos);
        Assert.Equal(("Compra", 14L, 1130m), (compra.Tipo, compra.IdDocumento, compra.Total));
    }

    [Fact]
    public async Task ForzarCompra_EnviaElMotivoPorPostALaRutaDeLaCompra()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":true}""");

        var r = await proxy.ForzarCompra(7, 14, "La mercaderia entro.");

        Assert.True(r.EsCorrecta);
        Assert.True(r.Responses);
        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/contabilidad/integridad/compras/14/forzar", handler.Ruta);
        Assert.Contains("\"motivo\":\"La mercaderia entro.\"", handler.Cuerpo);
        Assert.Contains("\"idLibroContable\":7", handler.Cuerpo);
    }

    [Fact]
    public async Task PropuestaProrrataIva_PideElPeriodoYLeeLasLineasDeLaPoliza()
    {
        var (proxy, handler) = Nuevo("""
            {"status":0,"responses":{"idLibroContable":7,"ejercicio":2026,"mes":10,"tipo":"Mensual","fechaContable":"2026-10-31","ivaSujetoProrrata":180.0,
             "porcentajeAplicado":60.0,"origenPorcentaje":"Fijado manualmente para 2026","creditoAcreditable":108.0,"gastoNoAcreditable":72.0,"periodoCompleto":true,
             "idSolicitud":null,"estadoSolicitud":null,
             "lineas":[{"orden":1,"tipoMovimiento":"Debito","codigoCuenta":"1.1.7.1","cuenta":"IVA soportado","monto":108.0,"descripcion":"Prorrata IVA 2026-10"},
                       {"orden":2,"tipoMovimiento":"Credito","codigoCuenta":"1.1.7.3","cuenta":"IVA soportado sujeto a proporcionalidad","monto":180.0,"descripcion":"Traslado"}],
             "advertencias":["Confirme con el contador."]}}
            """);

        var r = await proxy.PropuestaProrrataIva(7, 2026, 10);

        Assert.True(r.EsCorrecta);
        Assert.Equal("/api/contabilidad/integridad/prorrata-iva?idLibroContable=7&ejercicio=2026&mes=10", handler.Ruta);
        Assert.Equal((180m, 60m, 2), (r.Responses!.IvaSujetoProrrata, r.Responses.PorcentajeAplicado, r.Responses.Lineas.Count));
        Assert.Equal(("Debito", "1.1.7.1", 108m), (r.Responses.Lineas[0].TipoMovimiento, r.Responses.Lineas[0].CodigoCuenta, r.Responses.Lineas[0].Monto));
        Assert.Null(r.Responses.EstadoSolicitud);
    }

    [Fact]
    public async Task SolicitarProrrataIva_EnviaElPeriodoPorPostYDevuelveLaSolicitud()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":{"id":42,"idLibroContable":7,"tipo":"AjusteProrrataIva","estado":"Pendiente","motivo":"Prorrata IVA 2026-10","solicitadoPor":"asistente","fechaSolicitudUtc":"2026-11-05T12:00:00Z"}}""");

        var r = await proxy.SolicitarProrrataIva(7, 2026, 13);

        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/contabilidad/integridad/prorrata-iva/solicitar", handler.Ruta);
        Assert.Contains("\"mes\":13", handler.Cuerpo);
        Assert.Equal((42L, "Pendiente"), (r.Responses!.Id, r.Responses.Estado));
    }

    private sealed class SesionFalsa : IContextoSesion
    {
        public bool Autenticado => true;
        public string? Token => "token-prueba";
        public string? Usuario => "pruebas";
        public bool EsSuperAdministrador => true;
        public bool EsAdministrador => true;
        public string? PerfilCodigo => "SUPER_ADMIN";
        public bool EsCostaPets => false;
        public bool EsAgente => false;
        public bool EsServicioAlCliente => false;
        public bool PermitirExistenciaNegativa => false;
        public bool HabilitaImportaciones => false;
        public int IdSucursal => 1;
        public string? NombreSucursal => "Central";
        public bool TieneSucursal => true;
        public IReadOnlyCollection<string> Menus => [];
        public IReadOnlyCollection<PermisoFuncion> Permisos => [];
        public bool PuedeVer(string funcionCodigo) => true;
        public bool EstaGobernada(string funcionCodigo) => true;
        public bool Puede(string funcionCodigo, AccionPantalla accion) => true;
        public Task CargarAsync() => Task.CompletedTask;
    }
}
