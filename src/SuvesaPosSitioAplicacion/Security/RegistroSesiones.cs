using System.Collections.Concurrent;

namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Índice de las sesiones vivas de este proceso. El almacén de tickets no se puede
/// recorrer; sin esto no hay forma de ver quién está conectado ni de cerrar una
/// sesión que no es la propia.
/// </summary>
public sealed class RegistroSesiones
{
    public const int MinutosInactividad = 15;

    private readonly ConcurrentDictionary<string, SesionConectada> _sesiones = new();
    private readonly ConcurrentDictionary<string, byte> _cerradas = new();

    /// <summary>Registra o actualiza la sesión. Devuelve la llave de caché anterior, si cambió.</summary>
    public string? Registrar(SesionConectada nueva)
    {
        _cerradas.TryRemove(nueva.Id, out _);
        string? anterior = null;
        _sesiones.AddOrUpdate(
            nueva.Id,
            nueva,
            (_, previa) =>
            {
                anterior = previa.Llave;
                previa.Llave = nueva.Llave;
                previa.Nombre = string.IsNullOrWhiteSpace(nueva.Nombre) ? previa.Nombre : nueva.Nombre;
                previa.DireccionIp = nueva.DireccionIp ?? previa.DireccionIp;
                previa.UserAgent = nueva.UserAgent ?? previa.UserAgent;
                return previa;
            });
        return anterior is not null && anterior != nueva.Llave ? anterior : null;
    }

    public void Tocar(string id)
    {
        if (_sesiones.TryGetValue(id, out var sesion))
            sesion.UltimaActividad = DateTime.Now;
    }

    public bool FueCerrada(string id) => _cerradas.ContainsKey(id);

    public IReadOnlyList<SesionConectada> Listar()
        => _sesiones.Values.OrderByDescending(s => s.UltimaActividad).ToList();

    /// <summary>Saca la sesión del índice. Devuelve la llave para borrar el ticket.</summary>
    public string? Quitar(string id)
    {
        if (!_sesiones.TryRemove(id, out var sesion))
            return null;
        _cerradas[id] = 0;
        return sesion.Llave;
    }

    public void QuitarPorLlave(string llave)
    {
        var encontrada = _sesiones.Values.FirstOrDefault(s => s.Llave == llave);
        if (encontrada is not null)
            Quitar(encontrada.Id);
    }
}

public sealed class SesionConectada
{
    public required string Id { get; init; }
    public string Llave { get; set; } = "";
    public string Usuario { get; init; } = "";
    public string Nombre { get; set; } = "";
    public string? DireccionIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Inicio { get; init; }
    public DateTime UltimaActividad { get; set; }
}
