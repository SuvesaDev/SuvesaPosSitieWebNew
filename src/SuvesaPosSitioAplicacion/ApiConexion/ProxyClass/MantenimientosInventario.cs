using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Parametros;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <summary>Cliente manual temporal para los mantenimientos que aún no están en el OpenAPI generado.</summary>
public sealed class MantenimientosInventario : ProxyBase, IMantenimientosInventario
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly IHttpClientFactory _clientes;

    public MantenimientosInventario(IHttpClientFactory clientes, IContextoSesion sesion, ILogger<MantenimientosInventario> log) : base(sesion, log) => _clientes = clientes;

    public Task<ResponseGeneric<ICollection<BodegaMantenimientoDTO>>> Bodegas() => Enviar<ICollection<BodegaMantenimientoDTO>>(HttpMethod.Get, "api/mantenimientos/bodegas", "consultar las bodegas");
    public Task<ResponseGeneric<BodegaMantenimientoDTO>> CrearBodega(BodegaMantenimientoDTO bodega) => Enviar<BodegaMantenimientoDTO>(HttpMethod.Post, "api/mantenimientos/bodegas", "crear la bodega", bodega);
    public Task<ResponseGeneric<BodegaMantenimientoDTO>> EditarBodega(BodegaMantenimientoDTO bodega) => Enviar<BodegaMantenimientoDTO>(HttpMethod.Put, $"api/mantenimientos/bodegas/{bodega.IdBodega}", "editar la bodega", bodega);
    public Task<ResponseGeneric<bool>> DesactivarBodega(int idBodega) => Enviar<bool>(HttpMethod.Delete, $"api/mantenimientos/bodegas/{idBodega}", "desactivar la bodega");
    public Task<ResponseGeneric<ICollection<AreaMantenimientoDTO>>> Areas() => Enviar<ICollection<AreaMantenimientoDTO>>(HttpMethod.Get, "api/mantenimientos/areas", "consultar las áreas");
    public Task<ResponseGeneric<AreaMantenimientoDTO>> CrearArea(AreaMantenimientoDTO area) => Enviar<AreaMantenimientoDTO>(HttpMethod.Post, "api/mantenimientos/areas", "crear el área", area);
    public Task<ResponseGeneric<AreaMantenimientoDTO>> EditarArea(AreaMantenimientoDTO area) => Enviar<AreaMantenimientoDTO>(HttpMethod.Put, $"api/mantenimientos/areas/{area.IdArea}", "editar el área", area);
    public Task<ResponseGeneric<bool>> EliminarArea(decimal idArea) => Enviar<bool>(HttpMethod.Delete, $"api/mantenimientos/areas/{idArea}", "eliminar el área");

    private Task<ResponseGeneric<T>> Enviar<T>(HttpMethod metodo, string ruta, string operacion, object? cuerpo = null)
        => Ejecutar(async () =>
        {
            var cliente = _clientes.CreateClient("SeePosApi");
            using var solicitud = new HttpRequestMessage(metodo, ruta);
            if (cuerpo is not null) solicitud.Content = JsonContent.Create(cuerpo, options: Json);
            using var respuesta = await cliente.SendAsync(solicitud);
            var contenido = await respuesta.Content.ReadAsStringAsync();
            var envelope = JsonSerializer.Deserialize<SeguridadEnvelope<T>>(contenido, Json)
                ?? new SeguridadEnvelope<T> { Status = ResponseStatus._1, CurrentException = "Respuesta vacía del API." };
            return EnvelopeApi.A(envelope.Status, envelope.CurrentException, envelope.ValidationErrors, envelope.Responses);
        }, operacion);
}
