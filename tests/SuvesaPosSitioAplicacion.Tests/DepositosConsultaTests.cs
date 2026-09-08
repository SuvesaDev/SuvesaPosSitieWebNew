using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

public class DepositosConsultaTests
{
    [Fact]
    public async Task Buscar_PorNumero_NoEnviaTambienLasFechas()
    {
        var (consulta, handler) = CrearConsulta();

        var respuesta = await consulta.Buscar(
            " 1231 ", new DateTime(2026, 8, 31), new DateTime(2026, 9, 7));

        Assert.True(respuesta.EsCorrecta, respuesta.Excepcion);
        using var json = JsonDocument.Parse(handler.Cuerpo!);
        Assert.Equal("1231", json.RootElement.GetProperty("numero").GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("desde").ValueKind);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("hasta").ValueKind);
    }

    [Fact]
    public async Task Buscar_PorFechas_AdaptaLosLimitesInvertidosDelApiEIncluyeElUltimoDia()
    {
        var (consulta, handler) = CrearConsulta();

        var respuesta = await consulta.Buscar(
            null, new DateTime(2026, 8, 31), new DateTime(2026, 9, 7));

        Assert.True(respuesta.EsCorrecta, respuesta.Excepcion);
        using var json = JsonDocument.Parse(handler.Cuerpo!);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("numero").ValueKind);
        Assert.Equal(
            new DateTime(2026, 9, 7, 23, 59, 59, 999).AddTicks(9_999),
            json.RootElement.GetProperty("desde").GetDateTime());
        Assert.Equal(
            new DateTime(2026, 8, 31),
            json.RootElement.GetProperty("hasta").GetDateTime());
    }

    private static (DepositosConsulta Consulta, RegistroHandler Handler) CrearConsulta()
    {
        var handler = new RegistroHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.ejemplo/") };
        var api = new BancosApiCliente(http);
        var consulta = new DepositosConsulta(
            api,
            new SesionPrueba(),
            NullLogger<DepositosConsulta>.Instance);

        return (consulta, handler);
    }

    private sealed class RegistroHandler : HttpMessageHandler
    {
        public string? Cuerpo { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage solicitud,
            CancellationToken cancellationToken)
        {
            Assert.Equal(HttpMethod.Post, solicitud.Method);
            Assert.Equal("https://api.ejemplo/Bancos/ObtenerDepositos", solicitud.RequestUri?.ToString());
            Cuerpo = await solicitud.Content!.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"status\":0,\"responses\":[]}",
                    Encoding.UTF8,
                    "application/json")
            };
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
