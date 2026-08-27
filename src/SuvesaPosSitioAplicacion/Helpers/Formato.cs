using System.Globalization;

namespace SuvesaPosSitioAplicacion.Helpers;

/// <summary>
/// Como se muestran importes y cantidades. Punto unico: si manana cambia el numero
/// de decimales o el separador, cambia aqui y no en 78 pantallas.
///
/// TAMBIEN ES EL BORDE ENTRE float Y decimal.
/// El API entrega el dinero en <c>float</c> y <c>double</c> —asi lo declara su propio
/// swagger— y ninguna de las dos sirve para operar con dinero. Estos metodos aceptan
/// lo que llega y lo pasan a <see cref="decimal"/> en el momento de convertirlo a texto.
///
/// Regla: **formatear un float esta bien; sumar dos floats no**. Cuando una pantalla
/// tenga que operar, convierta con <see cref="AImporte"/> primero y opere en decimal.
/// </summary>
public static class Formato
{
    private static readonly CultureInfo Cultura = new("es-CR");

    /// <summary>Importe con dos decimales y separador de miles.</summary>
    public static string Importe(decimal valor) => valor.ToString("N2", Cultura);

    public static string Importe(float valor) => Importe(AImporte(valor));

    public static string Importe(double valor) => Importe(AImporte(valor));

    public static string Importe(decimal? valor) => valor.HasValue ? Importe(valor.Value) : "—";

    /// <summary>Cantidad. Sin decimales si es entera, con dos si no.</summary>
    public static string Cantidad(decimal valor)
        => valor == decimal.Truncate(valor)
            ? valor.ToString("N0", Cultura)
            : valor.ToString("N2", Cultura);

    public static string Cantidad(float valor) => Cantidad(AImporte(valor));

    public static string Cantidad(double valor) => Cantidad(AImporte(valor));

    /// <summary>
    /// Pasa a decimal un valor que llego del API en coma flotante.
    ///
    /// La conversion es directa a proposito. Pasar antes por <c>double</c> destapa
    /// el ruido binario del float —2090.91f vista como double es 2090.909912109375—
    /// mientras que el cast directo a decimal respeta los ~7 digitos significativos
    /// que el float realmente guarda, y devuelve 2090,91.
    ///
    /// Tampoco se redondea: redondear despues de haber destapado el ruido lo
    /// conserva en lugar de quitarlo. El cast ya hace lo correcto.
    ///
    /// Esto NO recupera precision que el API ya perdio, y con importes de mas de
    /// siete digitos el valor llega degradado de origen. Solo evita amplificarla.
    /// </summary>
    public static decimal AImporte(float valor)
    {
        if (float.IsNaN(valor) || float.IsInfinity(valor))
        {
            return 0m;
        }

        return (decimal)valor;
    }

    /// <inheritdoc cref="AImporte(float)" />
    public static decimal AImporte(double valor)
    {
        if (double.IsNaN(valor) || double.IsInfinity(valor))
        {
            return 0m;
        }

        // Fuera del rango de decimal no hay conversion posible; en importes reales
        // no deberia ocurrir nunca, pero no puede tumbar una pantalla.
        if (valor > (double)decimal.MaxValue || valor < (double)decimal.MinValue)
        {
            return 0m;
        }

        return (decimal)valor;
    }
}
