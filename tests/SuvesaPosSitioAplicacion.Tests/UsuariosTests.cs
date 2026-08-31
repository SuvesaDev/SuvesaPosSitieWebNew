using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

public class UsuariosTests
{
    [Fact]
    public async Task ObtenerUno_UsaElClienteAutenticadoYConLaUrlBase()
    {
        var handler = new RegistroHandler();
        var http = new HttpClient(
            new SuvesaPosSitioAplicacion.Helpers.ApiAuthHeaderHandler(
                NullLogger<SuvesaPosSitioAplicacion.Helpers.ApiAuthHeaderHandler>.Instance)
            { InnerHandler = handler })
        {
            BaseAddress = new Uri("https://api.ejemplo/")
        };

        var usuarios = new Usuarios(
            api: null!,
            seguridad: null!,
            clientes: new FabricaClientes(http),
            sesion: new SesionPrueba("token-prueba"),
            log: NullLogger<Usuarios>.Instance);

        var respuesta = await usuarios.ObtenerUno("ana@empresa");

        Assert.True(respuesta.EsCorrecta, respuesta.Excepcion);
        Assert.Equal(42L, respuesta.Responses!.Id!.Value);
        Assert.Equal("https://api.ejemplo/usuario/ObtenerUnUsuario?id=ana%40empresa", handler.Url);
        Assert.Equal("Bearer", handler.EsquemaAutorizacion);
        Assert.Equal("token-prueba", handler.Token);
    }

    private sealed class FabricaClientes(HttpClient cliente) : IHttpClientFactory
    {
        public HttpClient CreateClient(string nombre)
        {
            Assert.Equal("SeePosApi", nombre);
            return cliente;
        }
    }

    private sealed class RegistroHandler : HttpMessageHandler
    {
        public string? Url { get; private set; }
        public string? EsquemaAutorizacion { get; private set; }
        public string? Token { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage solicitud,
            CancellationToken cancellationToken)
        {
            Url = solicitud.RequestUri?.ToString();
            EsquemaAutorizacion = solicitud.Headers.Authorization?.Scheme;
            Token = solicitud.Headers.Authorization?.Parameter;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"status\":0,\"responses\":{\"id\":42,\"idUsuario\":\"ana@empresa\",\"nombre\":\"Ana\",\"activo\":true}}",
                    Encoding.UTF8,
                    "application/json")
            });
        }
    }

    private sealed class SesionPrueba(string token) : IContextoSesion
    {
        public bool Autenticado => true;
        public string? Token => token;
        public string? Usuario => "pruebas";
        public bool EsSuperAdministrador => true;
        public bool EsAdministrador => true;
        public string? PerfilCodigo => "SUPER_ADMIN";
        public bool EsCostaPets => false;
        public bool EsAgenteCostaPets => false;
        public int IdSucursal => 0;
        public string? NombreSucursal => null;
        public bool TieneSucursal => false;
        public IReadOnlyCollection<string> Menus => Array.Empty<string>();
        public IReadOnlyCollection<PermisoFuncion> Permisos => Array.Empty<PermisoFuncion>();
        public bool PuedeVer(string pantalla) => true;
        public bool EstaGobernada(string pantalla) => true;
        public bool Puede(string pantalla, AccionPantalla accion) => true;
        public Task CargarAsync() => Task.CompletedTask;
    }
}
