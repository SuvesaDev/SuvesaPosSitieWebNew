using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

public class CotizacionesTests
{
    [Fact]
    public async Task ObtenerPorId_RecuperaIdentificadorYDetalleDesdeElListado()
    {
        var handler = new RespuestaHandler(
            """{"status":0,"responses":{"cotizacion1":0,"nombreCliente":"Cliente prueba","detalle":[]}}""");
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.ejemplo/") };
        var api = new Cotizaciones(
            new CotizacionApiCliente(http),
            new SesionPrueba(),
            NullLogger<Cotizaciones>.Instance);
        var resumen = new CotizacionesDTO
        {
            Cotizacion1 = 123,
            Detalle =
            [
                new DetalleCotizacionDTO
                {
                    Numero = 77,
                    Cotizacion = 123,
                    Codigo = 456,
                    CodArticulo = "ART-456",
                    Descripcion = "Articulo guardado",
                    Cantidad = 2
                }
            ]
        };

        var respuesta = await api.ObtenerPorId(123, resumen);

        Assert.True(respuesta.EsCorrecta, respuesta.Excepcion);
        Assert.Equal(123, respuesta.Responses!.Cotizacion1);
        var linea = Assert.Single(respuesta.Responses.Detalle!);
        Assert.Equal(77, linea.Numero);
        Assert.Equal("ART-456", linea.CodArticulo);
        Assert.Equal("https://api.ejemplo/Cotizacion/ObtenerCotizacionPorID?id=123", handler.Url);
    }

    private sealed class RespuestaHandler(string respuesta) : HttpMessageHandler
    {
        public string? Url { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage solicitud,
            CancellationToken cancellationToken)
        {
            Url = solicitud.RequestUri?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(respuesta, Encoding.UTF8, "application/json")
            });
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
        public bool EsAgenteCostaPets => false;
        public bool PermitirExistenciaNegativa => false;
        public int IdSucursal => 1;
        public string? NombreSucursal => "Central";
        public bool TieneSucursal => true;
        public IReadOnlyCollection<string> Menus => Array.Empty<string>();
        public IReadOnlyCollection<PermisoFuncion> Permisos => Array.Empty<PermisoFuncion>();
        public bool PuedeVer(string pantalla) => true;
        public bool EstaGobernada(string pantalla) => true;
        public bool Puede(string pantalla, AccionPantalla accion) => true;
        public Task CargarAsync() => Task.CompletedTask;
    }
}
