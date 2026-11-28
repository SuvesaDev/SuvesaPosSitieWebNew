using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>Reenvío de la bandeja de correo: sin correo nuevo no manda cuerpo; con correo nuevo manda solo ese campo.</summary>
public sealed class EnviosCorreoProxyTests
{
    private sealed class Captura : HttpMessageHandler
    {
        public string? Ruta { get; private set; }
        public string? Cuerpo { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Ruta = request.RequestUri?.PathAndQuery;
            Cuerpo = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"status":0,"responses":true}""", Encoding.UTF8, "application/json") };
        }
    }

    private sealed class Fabrica(HttpClient cliente) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => cliente;
    }

    private static (EnviosCorreo Proxy, Captura Handler) Nuevo()
    {
        var handler = new Captura();
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://seepos.test/") };
        return (new EnviosCorreo(new Fabrica(http), new SesionFalsa(), NullLogger<EnviosCorreo>.Instance), handler);
    }

    [Fact]
    public async Task Reenviar_SinCorreo_NoEnviaCuerpo()
    {
        var (proxy, h) = Nuevo();
        var r = await proxy.Reenviar("CLAVE1");
        Assert.True(r.Responses);
        Assert.Equal("/api/envios-correo/comprobantes/CLAVE1/reenviar", h.Ruta);
        Assert.Null(h.Cuerpo);
    }

    [Fact]
    public async Task Reenviar_ConCorreoNuevo_EnviaLosDestinatarios()
    {
        var (proxy, h) = Nuevo();
        await proxy.Reenviar("CLAVE1", "nuevo@x.com; otro@y.com");
        Assert.Contains("\"destinatarios\":\"nuevo@x.com; otro@y.com\"", h.Cuerpo);
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
