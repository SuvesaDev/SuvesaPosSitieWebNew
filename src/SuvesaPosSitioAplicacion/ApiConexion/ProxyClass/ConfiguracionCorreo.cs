using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Correo;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IConfiguracionCorreo" />
public sealed class ConfiguracionCorreo : ProxyBase, IConfiguracionCorreo
{
    private readonly HttpClient _api;

    public ConfiguracionCorreo(IHttpClientFactory factory, IContextoSesion sesion, ILogger<ConfiguracionCorreo> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ConfiguracionCorreoVistaDTO>> Obtener(int idEmisor)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ConfiguracionCorreoVistaDTO>(
            await _api.GetAsync($"api/configuracion-correo/{idEmisor}")), "consultar la configuración de correo");

    public Task<ResponseGeneric<bool>> Guardar(ConfiguracionCorreoGuardarDTO datos)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PutAsJsonAsync($"api/configuracion-correo/{datos.IdEmisor}", datos, LecturaEnvelope.Json)),
            "guardar la configuración de correo");

    public Task<ResponseGeneric<bool>> Probar(int idEmisor, string destino)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PostAsJsonAsync($"api/configuracion-correo/{idEmisor}/probar",
                new ProbarCorreoDTO { Destino = destino }, LecturaEnvelope.Json)),
            "enviar el correo de prueba");
}
