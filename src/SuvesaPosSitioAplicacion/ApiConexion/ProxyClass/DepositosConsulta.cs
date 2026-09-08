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
            var r = await _api.ObtenerDepositosAsync(new FiltroBusquedaDepositosDTO
            {
                Numero = string.IsNullOrWhiteSpace(numero) ? null : numero,
                Desde = desde,
                Hasta = hasta
            });

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los depositos");
}
