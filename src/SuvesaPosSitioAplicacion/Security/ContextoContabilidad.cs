using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

namespace SuvesaPosSitioAplicacion.Security;

/// <inheritdoc cref="IContextoContabilidad" />
public sealed class ContextoContabilidad : IContextoContabilidad
{
    private readonly IContabilidad _contabilidad;
    private readonly Dictionary<int, bool> _cachePorEmisor = new();

    public ContextoContabilidad(IContabilidad contabilidad) => _contabilidad = contabilidad;

    public bool HabilitadaEmisorActual { get; private set; }

    public async Task ResolverAsync(long idEmpresa, int idEmisor, CancellationToken ct = default)
    {
        if (_cachePorEmisor.TryGetValue(idEmisor, out var cacheada))
        {
            HabilitadaEmisorActual = cacheada;
            return;
        }

        var respuesta = await _contabilidad.EstadoActivacion(idEmpresa, idEmisor);
        var habilitada = respuesta.Responses?.Efectivo == true;
        _cachePorEmisor[idEmisor] = habilitada;
        HabilitadaEmisorActual = habilitada;
    }
}
