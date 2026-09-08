using SuvesaPosSitioAplicacion.Class;

namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Lo que un rol puede hacer sobre una funcion concreta (rediseno de seguridad V2).
/// La llave es el <see cref="FuncionCodigo"/> (estable), no el rotulo.
/// </summary>
public sealed record PermisoFuncion(
    string ModuloCodigo,
    string FuncionCodigo,
    IReadOnlySet<string> Acciones)
{
    public bool Permite(AccionPantalla accion)
        => Acciones.Contains(accion.ToString().ToUpperInvariant());

    /// <summary>
    /// Serializa para guardarlo en un claim: <c>moduloCodigo|funcionCodigo|VER,CREAR,...</c>.
    /// Texto plano (no JSON) porque hay uno por funcion, ~90 por sesion, y el ticket se
    /// serializa entero en cada renovacion.
    /// </summary>
    public string AClaim()
        => string.Join('|', ModuloCodigo, FuncionCodigo, string.Join(',', Acciones));

    /// <summary>Inversa de <see cref="AClaim"/>. Nulo si el texto no tiene el formato.</summary>
    public static PermisoFuncion? DesdeClaim(string valor)
    {
        var p = valor.Split('|');
        if (p.Length != 3 || string.IsNullOrWhiteSpace(p[1]))
        {
            return null;
        }

        var acciones = p[2]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(a => a.ToUpperInvariant())
            .ToHashSet();

        return new PermisoFuncion(p[0], p[1], acciones);
    }
}
