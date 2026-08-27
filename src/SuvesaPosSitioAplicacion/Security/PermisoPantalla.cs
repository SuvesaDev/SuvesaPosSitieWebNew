using SuvesaPosSitioAplicacion.Class;

namespace SuvesaPosSitioAplicacion.Security;

/// <summary>Lo que un rol puede hacer sobre una pantalla concreta.</summary>
public sealed record PermisoPantalla(
    string Menu,
    string Pantalla,
    bool Ver,
    bool Crear,
    bool Modificar,
    bool Borrar)
{
    public bool Permite(AccionPantalla accion) => accion switch
    {
        AccionPantalla.Ver => Ver,
        AccionPantalla.Crear => Crear,
        AccionPantalla.Modificar => Modificar,
        AccionPantalla.Borrar => Borrar,
        _ => false
    };

    /// <summary>
    /// Serializa el permiso para guardarlo en un claim: menu|pantalla|ver|crear|modificar|borrar.
    /// Texto plano y no JSON porque hay uno por pantalla, ~82 por sesion, y el ticket
    /// se serializa entero en cada renovacion.
    /// </summary>
    public string AClaim() =>
        string.Join('|',
            Menu,
            Pantalla,
            Ver ? "1" : "0",
            Crear ? "1" : "0",
            Modificar ? "1" : "0",
            Borrar ? "1" : "0");

    /// <summary>Inversa de <see cref="AClaim"/>. Devuelve nulo si el texto no tiene el formato.</summary>
    public static PermisoPantalla? DesdeClaim(string valor)
    {
        var p = valor.Split('|');
        if (p.Length != 6)
        {
            return null;
        }

        return new PermisoPantalla(
            Menu: p[0],
            Pantalla: p[1],
            Ver: p[2] == "1",
            Crear: p[3] == "1",
            Modificar: p[4] == "1",
            Borrar: p[5] == "1");
    }
}
