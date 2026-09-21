using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

public sealed class ReportesOperacionProxyTests
{
    [Fact]
    public async Task Cabys_UsaLaRutaPublicadaPorElApi()
    {
        var handler = new CapturaHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://seepos.test/") };
        var proxy = new ReportesOperacion(
            new FabricaHttp(http),
            new SesionPrueba(),
            NullLogger<ReportesOperacion>.Instance);

        var respuesta = await proxy.Consultar("cabys", new FiltroReporteOperacionWebDTO
        {
            IdArticulo = 42,
            TamanoPagina = 25
        });

        Assert.True(respuesta.EsCorrecta, respuesta.Excepcion);
        Assert.Equal(
            "/api/reportes-operacion/cumplimiento-cabys?idArticulo=42&pagina=1&tamanoPagina=25",
            handler.Ruta);
    }

    private sealed class CapturaHandler : HttpMessageHandler
    {
        public string? Ruta { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Ruta = request.RequestUri?.PathAndQuery;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"status":0,"responses":{"titulo":"Cumplimiento CABYS","fuente":"Prueba","indicadores":[],"filas":[],"totalRegistros":0,"pagina":1,"tamanoPagina":25,"limitado":false}}""",
                    Encoding.UTF8,
                    "application/json")
            });
        }
    }

    private sealed class FabricaHttp(HttpClient cliente) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal("SeePosApi", name);
            return cliente;
        }
    }

    private sealed class SesionPrueba : IContextoSesion
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
