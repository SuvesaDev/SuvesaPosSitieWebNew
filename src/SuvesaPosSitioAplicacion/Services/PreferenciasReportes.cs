using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Services;

public interface IPreferenciasReportes
{
    IReadOnlyCollection<string> Favoritos { get; }
    IReadOnlyList<string> Recientes { get; }
    bool EsFavorito(string tipo);
    Task CargarAsync();
    Task AlternarFavoritoAsync(string tipo);
    Task RegistrarRecienteAsync(string tipo);
}

/// <summary>
/// Preferencias pequeñas del catálogo. Se guardan cifradas en el navegador y
/// separadas por usuario; los resultados del reporte permanecen en servidor.
/// </summary>
public sealed class PreferenciasReportes : IPreferenciasReportes
{
    private readonly ProtectedLocalStorage _almacen;
    private readonly IContextoSesion _sesion;
    private readonly HashSet<string> _favoritos = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _recientes = [];
    private bool _cargadas;

    public PreferenciasReportes(ProtectedLocalStorage almacen, IContextoSesion sesion)
    {
        _almacen = almacen;
        _sesion = sesion;
    }

    public IReadOnlyCollection<string> Favoritos => _favoritos;
    public IReadOnlyList<string> Recientes => _recientes;
    public bool EsFavorito(string tipo) => _favoritos.Contains(tipo);

    public async Task CargarAsync()
    {
        if (_cargadas) return;
        _cargadas = true;
        await _sesion.CargarAsync();
        try
        {
            var r = await _almacen.GetAsync<PreferenciasReportesGuardadas>(Llave());
            if (!r.Success || r.Value is null) return;
            _favoritos.UnionWith(r.Value.Favoritos);
            _recientes.AddRange(r.Value.Recientes.Where(x => !_recientes.Contains(x, StringComparer.OrdinalIgnoreCase)).Take(6));
        }
        catch
        {
            // Las preferencias nunca deben impedir abrir un reporte.
        }
    }

    public async Task AlternarFavoritoAsync(string tipo)
    {
        await CargarAsync();
        if (!_favoritos.Remove(tipo)) _favoritos.Add(tipo);
        await GuardarAsync();
    }

    public async Task RegistrarRecienteAsync(string tipo)
    {
        await CargarAsync();
        _recientes.RemoveAll(x => string.Equals(x, tipo, StringComparison.OrdinalIgnoreCase));
        _recientes.Insert(0, tipo);
        if (_recientes.Count > 6) _recientes.RemoveRange(6, _recientes.Count - 6);
        await GuardarAsync();
    }

    private async Task GuardarAsync()
    {
        try
        {
            await _almacen.SetAsync(Llave(), new PreferenciasReportesGuardadas(_favoritos.Order().ToArray(), _recientes.ToArray()));
        }
        catch
        {
            // La consulta sigue funcionando aunque el navegador rechace almacenamiento.
        }
    }

    private string Llave()
    {
        var usuario = string.IsNullOrWhiteSpace(_sesion.Usuario) ? "anonimo" : _sesion.Usuario;
        return $"seepos.reportes.preferencias.{usuario}";
    }
}

internal sealed record PreferenciasReportesGuardadas(string[] Favoritos, string[] Recientes);
