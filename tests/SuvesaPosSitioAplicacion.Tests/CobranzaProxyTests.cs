using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.DTOs.Cobranza;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Contrato del proxy de cobranzas contra el API (api/cobranza): rutas, filtros en la cadena de consulta,
/// enums numéricos y fechas. El JSON de ejemplo replica lo que serializa ApiSuvesaPos.
/// </summary>
public sealed class CobranzaProxyTests
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

    private static (Cobranza Proxy, Captura Handler) Nuevo(string json)
    {
        var handler = new Captura(json);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://seepos.test/") };
        return (new Cobranza(new Fabrica(http), new SesionFalsa(), NullLogger<Cobranza>.Instance), handler);
    }

    [Fact]
    public async Task FacturasVencidas_EnviaSoloLosFiltrosPresentes_ConElEnumNumerico()
    {
        var (proxy, handler) = Nuevo("""
            {"status":0,"responses":{"total":1,"pagina":2,"tamano":25,"saldoTotal":1500.5,"items":[
              {"idVenta":7,"numFactura":1007,"idCliente":8888888,"nombreCliente":"Cliente Uno","fecha":"2026-07-01T00:00:00",
               "vence":"2026-08-01T00:00:00","diasMora":88,"codMoneda":1,"montoOriginal":2000,"saldo":1500.5,
               "idSeguimiento":3,"cobrador":"ana","estadoSeguimiento":2,"tienePromesaIncumplida":true}]}}
            """);

        var r = await proxy.FacturasVencidas(new FiltroFacturasVencidasWeb
        {
            DiasMoraMin = 30, IdCliente = 8888888, Cobrador = "ana maría", Gestion = FiltroGestionWeb.ConSeguimiento, Pagina = 2, Tamano = 25,
        });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal(
            "/api/cobranza/facturas-vencidas?DiasMoraMin=30&IdCliente=8888888&Cobrador=ana%20mar%C3%ADa&Gestion=2&Pagina=2&Tamano=25",
            handler.Ruta);
        var f = Assert.Single(r.Responses!.Items);
        Assert.Equal(EstadoSeguimientoWeb.EnPromesa, f.EstadoSeguimiento);
        Assert.True(f.TienePromesaIncumplida);
        Assert.Equal(1500.5m, r.Responses.SaldoTotal);
    }

    [Fact]
    public async Task Detalle_LeeTimelineConEnumsNumericosYFechasDeLaPromesa()
    {
        var (proxy, handler) = Nuevo("""
            {"status":0,"responses":{
              "resumen":{"id":3,"idCliente":8888888,"nombreCliente":"Cliente Uno","codMoneda":1,"cobrador":"ana","estado":1,
                         "motivoCierre":null,"fechaCreacionUtc":"2026-10-28T15:00:00","fechaCierreUtc":null,"facturasActivas":1,"saldoActivo":600},
              "version":"AAAAAAAAB9E=",
              "facturas":[{"idVenta":7,"numFactura":1007,"fecha":"2026-07-01T00:00:00","vence":null,"montoOriginal":1000,"saldoAlAgregar":1000,
                           "saldoVigente":600,"activa":true,"fechaAgregadaUtc":"2026-10-28T15:00:00","fechaSalidaUtc":null,"motivoSalida":null}],
              "timeline":[
                {"id":9,"tipo":6,"resultado":20,"observacion":"Se aplicaron 400.00","usuario":"sistema","fechaUtc":"2026-10-29T10:00:00","idVenta":7,"monto":400,"idCorrige":null},
                {"id":8,"tipo":1,"resultado":1,"observacion":"Dijo que paga el viernes","usuario":"ana","fechaUtc":"2026-10-28T15:30:00","idVenta":null,"monto":null,"idCorrige":null}],
              "promesas":[{"id":1,"idSeguimiento":3,"fechaPrometida":"2026-11-05","montoAcordado":600,"montoAbonado":0,"motivo":"Espera depósito",
                           "estado":3,"usuario":"ana","fechaRegistroUtc":"2026-10-28T15:31:00","fechaResolucionUtc":"2026-11-06T00:00:00"}],
              "reasignaciones":[]}}
            """);

        var r = await proxy.Detalle(3);

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal("/api/cobranza/seguimientos/3", handler.Ruta);
        var d = r.Responses!;
        Assert.Equal(EstadoSeguimientoWeb.Abierto, d.Resumen.Estado);
        Assert.Equal(TipoInteraccionWeb.Sistema, d.Timeline[0].Tipo);
        Assert.Equal(ResultadoInteraccionWeb.AbonoRegistrado, d.Timeline[0].Resultado);
        Assert.Equal(new DateOnly(2026, 11, 5), d.Promesas[0].FechaPrometida);
        Assert.Equal(EstadoPromesaWeb.Incumplida, d.Promesas[0].Estado);
    }

    [Fact]
    public async Task RegistrarInteraccion_PublicaEnLaRutaDelSeguimiento_ConLosEnumsComoNumero()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":41}""");

        var r = await proxy.RegistrarInteraccion(3, new NuevaInteraccionWebComando
        {
            Tipo = TipoInteraccionWeb.Whatsapp, Resultado = ResultadoInteraccionWeb.PrometioPago, Observacion = "Confirmó el pago del viernes",
        });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(41, r.Responses);
        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/cobranza/seguimientos/3/interacciones", handler.Ruta);
        Assert.Contains("\"tipo\":3", handler.Cuerpo);
        Assert.Contains("\"resultado\":4", handler.Cuerpo);
        Assert.Contains("\"observacion\":\"Confirm", handler.Cuerpo);
    }

    [Fact]
    public async Task AgregarFacturas_PublicaLasFacturasYLaObservacion()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":true}""");

        var r = await proxy.AgregarFacturas(3, new AgregarFacturasWebComando { IdsVenta = { 7, 9 }, Observacion = "El cliente reconoce también la 1009" });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/cobranza/seguimientos/3/facturas", handler.Ruta);
        Assert.Contains("\"idsVenta\":[7,9]", handler.Cuerpo);
        Assert.Contains("\"observacion\":\"El cliente reconoce", handler.Cuerpo);
    }

    [Fact]
    public async Task CorregirNota_EnviaIdCorrigeSinTocarLaOriginal()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":52}""");

        await proxy.RegistrarInteraccion(3, new NuevaInteraccionWebComando
        {
            Tipo = TipoInteraccionWeb.NotaInterna, Resultado = ResultadoInteraccionWeb.SinResultado,
            Observacion = "Corrección: dijo que paga 5000", IdCorrige = 41,
        });

        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/cobranza/seguimientos/3/interacciones", handler.Ruta);
        Assert.Contains("\"idCorrige\":41", handler.Cuerpo);
    }

    [Fact]
    public async Task RegistrarPromesa_EnviaLaFechaComoDateOnly()
    {
        var (proxy, handler) = Nuevo("""{"status":0,"responses":5}""");

        await proxy.RegistrarPromesa(3, new NuevaPromesaWebComando
        {
            FechaPrometida = new DateOnly(2026, 11, 5), MontoAcordado = 600m, Motivo = "Espera depósito",
        });

        Assert.Equal("/api/cobranza/seguimientos/3/promesas", handler.Ruta);
        Assert.Contains("\"fechaPrometida\":\"2026-11-05\"", handler.Cuerpo);
        Assert.Contains("\"montoAcordado\":600", handler.Cuerpo);
    }

    [Fact]
    public async Task ErrorDelApi_LlegaComoRespuestaIncorrecta_ConElMensajeDelConflicto()
    {
        var (proxy, _) = Nuevo("""
            {"status":1,"currentException":"Ya tienen un seguimiento activo: factura 1007 → seguimiento 3 de ana.","validationErrors":[]}
            """);

        var r = await proxy.Abrir(new AbrirSeguimientoWebComando { IdsVenta = { 7 }, Observacion = "Primera gestión del caso" });

        Assert.False(r.EsCorrecta);
        Assert.Contains("seguimiento 3 de ana", r.Excepcion);
    }

    [Fact]
    public async Task DashboardGerencial_PideElRangoEnFormatoIso()
    {
        var (proxy, handler) = Nuevo("""
            {"status":0,"responses":{"desde":"2026-10-01","hasta":"2026-10-28","generadoUtc":"2026-10-28T15:00:00","seguimientosActivos":2,
             "interaccionesTotales":5,"efectividadPromesasGlobal":50.0,"cobradores":[
               {"cobrador":"ana","seguimientosActivos":1,"seguimientosCerradosPeriodo":1,"interacciones":3,"promesasCumplidas":1,
                "promesasIncumplidas":0,"efectividadPromesas":100.0,"saldoCartera":1000,"dso":42.5}]}}
            """);

        var r = await proxy.DashboardGerencial(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 28));

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal("/api/cobranza/dashboard/gerencial?desde=2026-10-01&hasta=2026-10-28", handler.Ruta);
        Assert.Equal(42.5m, r.Responses!.Cobradores[0].Dso);
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

public sealed class PresentacionCobranzaTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   corto  ", false)]
    [InlineData("Dijo que paga el viernes", true)]
    public void Observacion_ExigeElMinimoDeCaracteresSinContarEspacios(string? texto, bool valida)
        => Assert.Equal(valida, PresentacionCobranza.ObservacionValida(texto));

    [Fact]
    public void PromesaIncumplida_SePintaEnRojo_YLaCumplidaEnVerde()
    {
        Assert.Equal("text-bg-danger", PresentacionCobranza.ClasePromesa(EstadoPromesaWeb.Incumplida));
        Assert.Equal("text-bg-success", PresentacionCobranza.ClasePromesa(EstadoPromesaWeb.Cumplida));
        Assert.Equal("text-bg-warning", PresentacionCobranza.ClasePromesa(EstadoPromesaWeb.Pendiente));
    }

    [Fact]
    public void ElCobradorNoPuedeDigitarTiposNiResultadosDeSistema()
    {
        Assert.DoesNotContain(TipoInteraccionWeb.Sistema, PresentacionCobranza.TiposDigitables);
        Assert.All(PresentacionCobranza.ResultadosDigitables, r => Assert.True((int)r < 20));
        Assert.Equal(5, PresentacionCobranza.TiposDigitables.Length);   // llamada, correo, WhatsApp, visita, nota interna
    }

    [Fact]
    public void LosValoresNumericosCoincidenConLosDelApi()
    {
        Assert.Equal(2, (int)EstadoSeguimientoWeb.EnPromesa);
        Assert.Equal(3, (int)EstadoPromesaWeb.Incumplida);
        Assert.Equal(20, (int)ResultadoInteraccionWeb.AbonoRegistrado);
        Assert.Equal(6, (int)TipoInteraccionWeb.Sistema);
    }
}
