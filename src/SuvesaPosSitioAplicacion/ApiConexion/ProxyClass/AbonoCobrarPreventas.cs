using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IAbonoCobrarPreventas" />
public sealed class AbonoCobrarPreventas : ProxyBase, IAbonoCobrarPreventas
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public AbonoCobrarPreventas(IHttpClientFactory factory, IContextoSesion sesion, ILogger<AbonoCobrarPreventas> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ICollection<PreventaResumenWebDTO>>> PreventasPendientes(long codCliente)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ICollection<PreventaResumenWebDTO>>(
            await _api.PostAsync($"venta/PreventasPendientesPorCliente?codCliente={codCliente}", null)),
            "consultar las preventas pendientes del cliente");

    public Task<ResponseGeneric<ResultadoEmisionWebDTO>> EmitirFactura(long idVenta)
        => Emitir($"api/comprobantes-electronicos/v44/pos/ventas/{idVenta}/facturas/emitir");

    public Task<ResponseGeneric<ResultadoEmisionWebDTO>> EmitirTiquete(long idVenta)
        => Emitir($"api/comprobantes-electronicos/v44/pos/ventas/{idVenta}/tiquetes/emitir");

    private Task<ResponseGeneric<ResultadoEmisionWebDTO>> Emitir(string ruta)
        => Ejecutar(async () =>
        {
            var respuesta = await _api.PostAsync(ruta, null);
            var cuerpo = await respuesta.Content.ReadAsStringAsync();

            // Este controlador NO usa el envelope: devuelve el DTO plano
            // (200 = EsValido; 422 = con errores).
            ResultadoEmisionWebDTO? dto;
            try { dto = JsonSerializer.Deserialize<ResultadoEmisionWebDTO>(cuerpo, Json); }
            catch (JsonException) { dto = null; }

            if (dto is null)
                return new ResponseGeneric<ResultadoEmisionWebDTO>(
                    $"El API respondió {(int)respuesta.StatusCode}: {cuerpo}");

            return new ResponseGeneric<ResultadoEmisionWebDTO>(dto);
        }, "emitir el comprobante a Hacienda");
}
