using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Caja;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class InstrumentosConfirmacion : ProxyBase, IInstrumentosConfirmacion
{
    private readonly HttpClient _api;
    public InstrumentosConfirmacion(IHttpClientFactory factory, IContextoSesion sesion, ILogger<InstrumentosConfirmacion> logger) : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");
    public Task<ResponseGeneric<ICollection<InstrumentoConfirmacionWebDTO>>> Buscar(FiltroInstrumentosConfirmacionWebDTO f)
    {
        var q = new List<string>(); if (!string.IsNullOrWhiteSpace(f.Estado)) q.Add($"estado={Uri.EscapeDataString(f.Estado)}"); if (!string.IsNullOrWhiteSpace(f.Tipo)) q.Add($"tipo={Uri.EscapeDataString(f.Tipo)}"); if (f.NumApertura.HasValue) q.Add($"numApertura={f.NumApertura}");
        return Ejecutar(async () => await LecturaEnvelope.Leer<ICollection<InstrumentoConfirmacionWebDTO>>(await _api.GetAsync($"api/instrumentos-confirmacion{(q.Count == 0 ? "" : "?" + string.Join("&", q))}")), "consultar instrumentos pendientes");
    }
    public Task<ResponseGeneric<InstrumentoConfirmacionWebDTO>> CambiarEstado(long id, CambioEstadoInstrumentoWebDTO cambio)
        => Ejecutar(async () => await LecturaEnvelope.Leer<InstrumentoConfirmacionWebDTO>(await _api.PostAsJsonAsync($"api/instrumentos-confirmacion/{id}/estado", cambio, LecturaEnvelope.Json)), "actualizar el estado del instrumento");
    public Task<ResponseGeneric<ResumenInstrumentosCajaWebDTO>> ResumenCaja(long apertura)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ResumenInstrumentosCajaWebDTO>(await _api.GetAsync($"api/instrumentos-confirmacion/caja/{apertura}/resumen")), "consultar los instrumentos de caja");
}
