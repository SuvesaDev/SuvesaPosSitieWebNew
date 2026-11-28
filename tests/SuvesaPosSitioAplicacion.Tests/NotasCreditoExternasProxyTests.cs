using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>Contrato del proxy de notas de crédito externas contra el API (api/notas-credito-externas): rutas, cuerpo y lectura del sobre.</summary>
public sealed class NotasCreditoExternasProxyTests
{
    private sealed class Captura : HttpMessageHandler
    {
        private readonly string _json;
        public Captura(string json) => _json = json;
        public HttpMethod? Metodo { get; private set; }
        public string? Ruta { get; private set; }
        public string? Cuerpo { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Metodo = request.Method;
            Ruta = request.RequestUri?.PathAndQuery;
            Cuerpo = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(_json, Encoding.UTF8, "application/json") };
        }
    }

    private sealed class Fabrica(HttpClient cliente) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal("SeePosApi", name);
            return cliente;
        }
    }

    private static (NotasCreditoExternas Proxy, Captura Handler) Nuevo(string json)
    {
        var handler = new Captura(json);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://seepos.test/") };
        return (new NotasCreditoExternas(new Fabrica(http), new SesionFalsa(), NullLogger<NotasCreditoExternas>.Instance), handler);
    }

    [Fact]
    public async Task Validar_EnviaLosDosXmlYLeeLosCandidatos()
    {
        var (proxy, handler) = Nuevo("""
            {"status":0,"responses":{"valido":true,"errores":[],"bloqueo":null,"clave":"506","tipoDocumento":"01","consecutivo":"00100001010000000725",
              "fechaEmision":"2026-01-16T07:23:22","receptorIdentificacion":"3101054264","receptorNombre":"CLIENTE SA","totalOrigen":2260.00,"totalImpuesto":260.00,
              "acreditadoPrevio":0,"disponible":2260.00,
              "lineas":[{"numeroLinea":1,"codigoCabys":"2331100000100","detalle":"Alimento","unidadMedida":"Unid","cantidad":2.0,"precioUnitario":1000.00,"subTotal":2000.00,"tarifa":13,"montoImpuesto":260.00,"montoTotalLinea":2260.00,"cantidadAcreditadaPrevia":0.5,"cantidadDisponible":1.5}],
              "clientesCandidatos":[{"idCliente":172,"nombre":"TRES CIENTO UNO S.A.","cedula":"3101054264","activo":true,"nota":null},
                                    {"idCliente":56,"nombre":"SOCIEDAD ANONIMA","cedula":"3101054264","activo":false,"nota":"Unificado en 52"}],
              "ventasCandidatas":[{"idVenta":500,"numFactura":725,"fecha":"2026-01-16T00:00:00","total":2260.02,"saldo":2260.02,"idCliente":172,"tieneDevolucionInterna":false,"coincidencia":"Alta"}]}}
            """);

        var r = await proxy.Validar(new ValidarNotaCreditoExternaWebComando { IdEmisor = 1, IdSucursal = 3, XmlFactura = "<FE/>", XmlAcuse = "<AHC/>" });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/notas-credito-externas/validar", handler.Ruta);
        Assert.Contains("\"xmlFactura\":\"\\u003CFE/\\u003E\"", handler.Cuerpo!.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"idSucursal\":3", handler.Cuerpo);
        var v = r.Responses!;
        Assert.True(v.Valido);
        Assert.Equal(2260.00m, v.Disponible);
        Assert.Equal(2, v.ClientesCandidatos.Count);
        Assert.False(v.ClientesCandidatos[1].Activo);
        Assert.Equal("Unificado en 52", v.ClientesCandidatos[1].Nota);
        var venta = Assert.Single(v.VentasCandidatas);
        Assert.Equal(500, venta.IdVenta);
        Assert.Equal("Alta", venta.Coincidencia);
        var linea = Assert.Single(v.Lineas);
        Assert.Equal(0.5m, linea.CantidadAcreditadaPrevia);
        Assert.Equal(1.5m, linea.CantidadDisponible);
    }

    [Fact]
    public async Task Crear_EnviaElComandoYLeeLaNota()
    {
        var (proxy, handler) = Nuevo("""
            {"status":0,"responses":{"id":9,"nombreCliente":"TRES CIENTO UNO S.A.","razon":"Crédito","claveOrigen":"506","clavedgt":"","consecutivodgt":"","estadodgt":"",
              "monto":2260.00,"fecha":"2026-11-26T10:00:00","idCliente":172,"idVenta":null,"esSoloFiscal":false,"motivoSoloFiscal":null,"fueReintento":false}}
            """);

        var r = await proxy.Crear(new CrearNotaCreditoExternaWebComando
        {
            ClaveIdempotencia = "abc", IdEmisor = 1, IdSucursal = 3, XmlFactura = "<FE/>", XmlAcuse = "<AHC/>", IdCliente = 172, IdVenta = 500, Razon = "Crédito", SoloFiscal = false,
            Lineas = new() { new LineaCreditoNotaCreditoExternaWebComando { NumeroLinea = 1, Cantidad = 0.75m } },
        });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/notas-credito-externas", handler.Ruta);
        Assert.Contains("\"claveIdempotencia\":\"abc\"", handler.Cuerpo);
        Assert.Contains("\"idVenta\":500", handler.Cuerpo);
        Assert.Contains("\"lineas\":[{\"numeroLinea\":1,\"cantidad\":0.75}]", handler.Cuerpo);
        Assert.DoesNotContain("\"monto\"", handler.Cuerpo);
        Assert.Equal(9, r.Responses!.Id);
        Assert.Equal(2260.00m, r.Responses.Monto);
        Assert.Null(r.Responses.IdVenta);
    }

    [Fact]
    public async Task Crear_UnaRespuestaConErrorLlegaComoErrorSinLanzar()
    {
        var (proxy, _) = Nuevo("""{"status":1,"currentException":"El cliente está inactivo (¿unificado en otro?). Elija el cliente activo.","responses":null}""");

        var r = await proxy.Crear(new CrearNotaCreditoExternaWebComando { IdCliente = 56 });

        Assert.False(r.EsCorrecta);
        Assert.Contains("inactivo", r.Excepcion);
    }

    [Fact]
    public async Task Listar_EnviaSoloLosFiltrosPresentes()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":{"total":0,"items":[]}}""");

        var r = await proxy.Listar("Aceptado", null, 2, 20);

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal("/api/notas-credito-externas?estado=Aceptado&pagina=2&tamano=20", handler.Ruta);
    }

    [Fact]
    public async Task Obtener_PideLaNotaPorId()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":{"id":9,"monto":2260.00,"estadodgt":"Aceptado","mensajeMh":"Los datos del emisor no concuerdan (código -37)","mensajeMhGravedad":"Aviso"}}""");

        var r = await proxy.Obtener(9);

        Assert.Equal("/api/notas-credito-externas/9", handler.Ruta);
        Assert.Equal("Aceptado", r.Responses!.Estadodgt);
        Assert.Equal("Aviso", r.Responses.MensajeMhGravedad);
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
