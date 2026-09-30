using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

namespace SuvesaPosSitioAplicacion.Security;

/// <inheritdoc cref="IContextoContabilidad" />
public sealed class ContextoContabilidad : IContextoContabilidad
{
    private readonly IContabilidad _contabilidad;
    private readonly Dictionary<(int Emisor, int Sucursal), bool> _cache = new();

    public ContextoContabilidad(IContabilidad contabilidad) => _contabilidad = contabilidad;

    public bool HabilitadaEmisorActual { get; private set; }

    public async Task ResolverAsync(long idEmpresa, int idEmisor, int idSucursal, CancellationToken ct = default)
    {
        if (_cache.TryGetValue((idEmisor, idSucursal), out var cacheada))
        {
            HabilitadaEmisorActual = cacheada;
            return;
        }

        var respuesta = await _contabilidad.EstadoActivacion(idEmpresa, idEmisor, idSucursal);
        var habilitada = respuesta.Responses?.Efectivo == true;
        _cache[(idEmisor, idSucursal)] = habilitada;
        HabilitadaEmisorActual = habilitada;
    }
}
