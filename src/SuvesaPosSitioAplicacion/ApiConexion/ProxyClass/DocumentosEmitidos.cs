using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IDocumentosEmitidos" />
public sealed class DocumentosEmitidos : ProxyBase, IDocumentosEmitidos
{
    private readonly IVentaApiCliente _api;

    public DocumentosEmitidos(
        IVentaApiCliente api,
        IContextoSesion sesion,
        ILogger<DocumentosEmitidos> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<FacturaDTO>>> PorFechas(DateTime desde, DateTime hasta)
        => Ejecutar(async () =>
        {
            // El API espera las fechas como texto. Se manda ISO corta, que es lo que
            // usa el sistema actual desde el input date del navegador.
            var r = await _api.SeleccionarFacturaFechasAsync(new FiltroFacturaFechas
            {
                Desde = desde.ToString("yyyy-MM-dd"),
                Hasta = hasta.ToString("yyyy-MM-dd")
            });

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los documentos por fechas");

    public Task<ResponseGeneric<ICollection<FacturaDTO>>> PorCliente(string codCliente)
        => Ejecutar(async () =>
        {
            var r = await _api.SeleccionarFacturaClienteAsync(
                new FiltroFacturaCliente { CodCliente = codCliente });

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los documentos del cliente");

    public Task<ResponseGeneric<FacturaDTO>> PorNumero(string numero)
        => Ejecutar(async () =>
        {
            var r = await _api.SeleccionarFacturaNumeroAsync(numero);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el documento");
}
