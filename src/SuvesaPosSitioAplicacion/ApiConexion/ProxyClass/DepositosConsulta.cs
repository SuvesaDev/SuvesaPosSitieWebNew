using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IDepositosConsulta" />
public sealed class DepositosConsulta : ProxyBase, IDepositosConsulta
{
    private readonly IBancosApiCliente _api;

    public DepositosConsulta(IBancosApiCliente api, IContextoSesion sesion, ILogger<DepositosConsulta> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<DepositosBuscarDTO>>> Buscar(
        string? numero, DateTime? desde, DateTime? hasta)
        => Ejecutar(async () =>
        {
            var numeroNormalizado = string.IsNullOrWhiteSpace(numero) ? null : numero.Trim();

            // El API no combina criterios: sus consultas esperan numero O fechas,
            // pero no ambos a la vez. Si hay numero, este tiene prioridad.
            //
            // Ademas, la implementacion vigente del API conserva los limites de
            // fecha invertidos (Fecha <= Desde && Fecha >= Hasta). Adaptamos aqui
            // el contrato heredado y hacemos inclusivo el ultimo dia solicitado.
            var filtro = numeroNormalizado is not null
                ? new FiltroBusquedaDepositosDTO { Numero = numeroNormalizado }
                : new FiltroBusquedaDepositosDTO
                {
                    Desde = hasta?.Date.AddDays(1).AddTicks(-1),
                    Hasta = desde?.Date
                };

            var r = await _api.ObtenerDepositosAsync(filtro);

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los depositos");
}
