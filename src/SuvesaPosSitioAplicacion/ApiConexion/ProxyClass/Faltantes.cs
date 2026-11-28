using System.Globalization;
using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class Faltantes : ProxyBase, IFaltantes
{
    private readonly HttpClient _api;
    public Faltantes(IHttpClientFactory factory, IContextoSesion sesion, ILogger<Faltantes> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<SolicitudFaltanteDTO>> Crear(CrearSolicitudFaltanteRequest solicitud)
        => Ejecutar(async () => await LecturaEnvelope.Leer<SolicitudFaltanteDTO>(
            await _api.PostAsJsonAsync("api/comercial/faltantes", solicitud, LecturaEnvelope.Json)), "registrar la solicitud de faltante");

    public Task<ResponseGeneric<IReadOnlyList<SolicitudFaltanteDTO>>> Mis(bool soloAbiertas)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<SolicitudFaltanteDTO>>(
            await _api.GetAsync("api/comercial/faltantes/mis?soloAbiertas=" + (soloAbiertas ? "true" : "false"))), "consultar sus solicitudes de faltantes");

    public Task<ResponseGeneric<SolicitudFaltanteDTO>> Concretar(long id, long idVenta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<SolicitudFaltanteDTO>(
            await _api.PutAsJsonAsync($"api/comercial/faltantes/{id}/concretar", new ConcretarSolicitudRequest { IdVenta = idVenta }, LecturaEnvelope.Json)), "concretar la solicitud");

    public Task<ResponseGeneric<SolicitudFaltanteDTO>> Cancelar(long id, string? motivo)
        => Ejecutar(async () => await LecturaEnvelope.Leer<SolicitudFaltanteDTO>(
            await _api.PutAsJsonAsync($"api/comercial/faltantes/{id}/cancelar", new CancelarSolicitudRequest { Motivo = motivo }, LecturaEnvelope.Json)), "cancelar la solicitud");

    public Task<ResponseGeneric<IReadOnlyList<GrupoAbastecimientoDTO>>> Bandeja()
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<GrupoAbastecimientoDTO>>(
            await _api.GetAsync("api/comercial/faltantes/bandeja")), "consultar la bandeja de abastecimiento");

    public Task<ResponseGeneric<int>> VincularOrdenCompra(VincularOrdenCompraRequest solicitud)
        => Ejecutar(async () => await LecturaEnvelope.Leer<int>(
            await _api.PutAsJsonAsync("api/comercial/faltantes/orden-compra", solicitud, LecturaEnvelope.Json)), "vincular la orden de compra");

    public Task<ResponseGeneric<HistorialFaltantesDTO>> Historial(FiltroHistorialFaltantesDTO filtro)
    {
        var partes = new List<string>();
        if (filtro.Desde is DateTime d) partes.Add("Desde=" + d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (filtro.Hasta is DateTime h) partes.Add("Hasta=" + h.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (filtro.IdRuta is int r) partes.Add("IdRuta=" + r.ToString(CultureInfo.InvariantCulture));
        if (filtro.CodMarca is int m) partes.Add("CodMarca=" + m.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrWhiteSpace(filtro.IdUsuario)) partes.Add("IdUsuario=" + Uri.EscapeDataString(filtro.IdUsuario));
        if (filtro.IdCliente is long c) partes.Add("IdCliente=" + c.ToString(CultureInfo.InvariantCulture));
        if (filtro.IdInventario is long i) partes.Add("IdInventario=" + i.ToString(CultureInfo.InvariantCulture));
        var consulta = partes.Count == 0 ? string.Empty : "?" + string.Join("&", partes);
        return Ejecutar(async () => await LecturaEnvelope.Leer<HistorialFaltantesDTO>(
            await _api.GetAsync("api/comercial/faltantes/historial" + consulta)), "consultar el historial de faltantes");
    }
}
