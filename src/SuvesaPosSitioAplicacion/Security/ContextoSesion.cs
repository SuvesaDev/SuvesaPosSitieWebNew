namespace SuvesaPosSitioAplicacion.Security;

/// <inheritdoc cref="IContextoSesion" />
public sealed class ContextoSesion : IContextoSesion
{
    private DatosSesion? _datos;

    public bool Autenticado => _datos is not null;
    public string? Token => _datos?.Token;
    public string? Usuario => _datos?.Usuario;
    public int IdSucursal => _datos?.IdSucursal ?? 0;
    public string? NombreSucursal => _datos?.NombreSucursal;
    public bool EsAdministrador => _datos?.EsAdministrador ?? false;

    public IReadOnlyCollection<string> Modulos => _datos?.Modulos ?? Array.Empty<string>();
    public IReadOnlyCollection<string> Pantallas => _datos?.Pantallas ?? Array.Empty<string>();

    public bool PuedeVer(string pantalla)
        => EsAdministrador || Pantallas.Contains(pantalla, StringComparer.OrdinalIgnoreCase);

    public bool Puede(string pantalla, string accion)
    {
        if (EsAdministrador)
        {
            return true;
        }

        if (_datos is null || !_datos.AccionesPorPantalla.TryGetValue(pantalla, out var acciones))
        {
            return false;
        }

        return acciones.Contains(accion, StringComparer.OrdinalIgnoreCase);
    }

    public void Iniciar(DatosSesion datos) => _datos = datos;

    public void Cerrar() => _datos = null;
}
