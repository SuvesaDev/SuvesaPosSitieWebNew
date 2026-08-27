using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Guarda las pestanas en el almacenamiento local del navegador, cifrado por el
/// servidor. Es el equivalente en Blazor Server del localStorage que usa hoy
/// el sistema en React.
/// </summary>
public sealed class AlmacenEspacioTrabajoNavegador : IAlmacenEspacioTrabajo
{
    private const string Llave = "seepos.pestanas";

    private readonly ProtectedLocalStorage _almacen;

    public AlmacenEspacioTrabajoNavegador(ProtectedLocalStorage almacen)
    {
        _almacen = almacen;
    }

    public async Task<EstadoEspacioGuardado?> LeerAsync()
    {
        var r = await _almacen.GetAsync<EstadoEspacioGuardado>(Llave);
        return r.Success ? r.Value : null;
    }

    public Task GuardarAsync(EstadoEspacioGuardado estado)
        => _almacen.SetAsync(Llave, estado).AsTask();
}
