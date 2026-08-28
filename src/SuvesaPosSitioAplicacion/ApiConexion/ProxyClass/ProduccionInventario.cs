using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IProduccionInventario" />
public sealed class ProduccionInventario : ProxyBase, IProduccionInventario
{
    private readonly IBodegaApiCliente _bodegas;
    private readonly IArticulosRelacionadosApiCliente _relacionados;
    private readonly ICalculadoraProduccionLotesApiCliente _calculadora;

    public ProduccionInventario(
        IBodegaApiCliente bodegas,
        IArticulosRelacionadosApiCliente relacionados,
        ICalculadoraProduccionLotesApiCliente calculadora,
        IContextoSesion sesion,
        ILogger<ProduccionInventario> log)
        : base(sesion, log)
    {
        _bodegas = bodegas;
        _relacionados = relacionados;
        _calculadora = calculadora;
    }

    public Task<ResponseGeneric<ICollection<Bodega>>> Bodegas(bool costaPets)
        => Ejecutar(async () =>
        {
            var r = costaPets
                ? await _bodegas.ObtenerBodegasCostaPetsAsync()
                : await _bodegas.ObtenerBodegasAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las bodegas de producción");

    public Task<ResponseGeneric<ICollection<ArticulosRelacionadosDTO>>> Formula(long articuloPrincipal)
        => Ejecutar(async () =>
        {
            var r = await _relacionados.BuscarArticulosRelacionadosFormulaAsync(articuloPrincipal);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la fórmula del artículo");

    public Task<ResponseGeneric<bool>> GuardarFormula(long principal, long componente, float cantidad, bool activa)
        => Ejecutar(async () =>
        {
            var r = await _relacionados.PutArticuloRelacionadoFormulaAsync(principal, componente, cantidad, activa);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, activa ? "guardar un componente de fórmula" : "quitar un componente de fórmula");

    public Task<ResponseGeneric<int>> Calcular(CalculadoraDTO calculo)
        => Ejecutar(async () =>
        {
            var r = await _calculadora.CalculadoraProductivaAsync(calculo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, calculo.ConvertirCantidad ? "convertir producción por lotes" : "calcular producción disponible");
}
