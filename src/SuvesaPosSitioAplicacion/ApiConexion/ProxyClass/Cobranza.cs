using System.Globalization;
using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Cobranza;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICobranza" />
public sealed class Cobranza : ProxyBase, ICobranza
{
    private readonly HttpClient _api;

    public Cobranza(IHttpClientFactory factory, IContextoSesion sesion, ILogger<Cobranza> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    /// <summary>Arma la cadena de consulta con los filtros presentes; todo valor se codifica.</summary>
    internal static string Consulta(IEnumerable<(string Clave, object? Valor)> parametros)
    {
        var partes = parametros
            .Where(p => p.Valor is not null && !(p.Valor is string s && string.IsNullOrWhiteSpace(s)))
            .Select(p => $"{p.Clave}={Uri.EscapeDataString(Convert.ToString(p.Valor is Enum e ? (int)(object)e : p.Valor, CultureInfo.InvariantCulture)!)}");
        var texto = string.Join('&', partes);
        return texto.Length == 0 ? string.Empty : "?" + texto;
    }

    public Task<ResponseGeneric<PaginaFacturasVencidasWeb>> FacturasVencidas(FiltroFacturasVencidasWeb f)
        => Ejecutar(async () => await LecturaEnvelope.Leer<PaginaFacturasVencidasWeb>(await _api.GetAsync(
            "api/cobranza/facturas-vencidas" + Consulta(new (string, object?)[]
            {
                ("DiasMoraMin", f.DiasMoraMin), ("DiasMoraMax", f.DiasMoraMax), ("IdCliente", f.IdCliente),
                ("Cobrador", f.Cobrador), ("Gestion", f.Gestion), ("Pagina", f.Pagina), ("Tamano", f.Tamano),
            }))), "consultar las facturas vencidas");

    public Task<ResponseGeneric<List<SeguimientoResumenWebDTO>>> Seguimientos(string? cobrador, EstadoSeguimientoWeb? estado, long? idCliente)
        => Ejecutar(async () => await LecturaEnvelope.Leer<List<SeguimientoResumenWebDTO>>(await _api.GetAsync(
            "api/cobranza/seguimientos" + Consulta(new (string, object?)[]
            {
                ("cobrador", cobrador), ("estado", estado), ("idCliente", idCliente),
            }))), "consultar los seguimientos");

    public Task<ResponseGeneric<SeguimientoDetalleWebDTO>> Detalle(long idSeguimiento)
        => Ejecutar(async () => await LecturaEnvelope.Leer<SeguimientoDetalleWebDTO>(
            await _api.GetAsync($"api/cobranza/seguimientos/{idSeguimiento}")), "consultar el seguimiento");

    public Task<ResponseGeneric<long>> Abrir(AbrirSeguimientoWebComando comando)
        => Ejecutar(async () => await LecturaEnvelope.Leer<long>(
            await _api.PostAsJsonAsync("api/cobranza/seguimientos", comando, LecturaEnvelope.Json)), "abrir el seguimiento");

    public Task<ResponseGeneric<bool>> AgregarFacturas(long idSeguimiento, AgregarFacturasWebComando comando)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PostAsJsonAsync($"api/cobranza/seguimientos/{idSeguimiento}/facturas", comando, LecturaEnvelope.Json)), "agregar facturas al seguimiento");

    public Task<ResponseGeneric<long>> RegistrarInteraccion(long idSeguimiento, NuevaInteraccionWebComando comando)
        => Ejecutar(async () => await LecturaEnvelope.Leer<long>(
            await _api.PostAsJsonAsync($"api/cobranza/seguimientos/{idSeguimiento}/interacciones", comando, LecturaEnvelope.Json)), "registrar la interacción");

    public Task<ResponseGeneric<long>> RegistrarPromesa(long idSeguimiento, NuevaPromesaWebComando comando)
        => Ejecutar(async () => await LecturaEnvelope.Leer<long>(
            await _api.PostAsJsonAsync($"api/cobranza/seguimientos/{idSeguimiento}/promesas", comando, LecturaEnvelope.Json)), "registrar la promesa de pago");

    public Task<ResponseGeneric<bool>> Reasignar(long idSeguimiento, ReasignarWebComando comando)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PostAsJsonAsync($"api/cobranza/seguimientos/{idSeguimiento}/reasignar", comando, LecturaEnvelope.Json)), "reasignar el seguimiento");

    public Task<ResponseGeneric<DashboardCobradorWebDTO>> DashboardCobrador()
        => Ejecutar(async () => await LecturaEnvelope.Leer<DashboardCobradorWebDTO>(
            await _api.GetAsync("api/cobranza/dashboard/cobrador")), "consultar el tablero del cobrador");

    public Task<ResponseGeneric<DashboardGerencialWebDTO>> DashboardGerencial(DateOnly desde, DateOnly hasta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<DashboardGerencialWebDTO>(
            await _api.GetAsync($"api/cobranza/dashboard/gerencial?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}")), "consultar el tablero gerencial");
}
