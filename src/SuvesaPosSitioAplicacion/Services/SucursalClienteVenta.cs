using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Sucursal del cliente a la que se factura o cotiza. Facturación y Proforma la resuelven igual: obligatoria solo cuando
/// el cliente tiene más de una; con una sola se propone sola; sin sucursales no hay nada que elegir.
/// </summary>
public static class SucursalClienteVenta
{
    /// <summary>Hay que elegir (el cliente tiene varias sucursales).</summary>
    public static bool Obligatoria(IReadOnlyCollection<ClienteDatosSucursalDTO> sucursales) => sucursales.Count > 1;

    /// <summary>Se muestra el selector solo cuando hay varias.</summary>
    public static bool MostrarSelector(IReadOnlyCollection<ClienteDatosSucursalDTO> sucursales) => sucursales.Count > 1;

    /// <summary>Valor inicial: el actual si sigue siendo del cliente; la única sucursal si solo tiene una; si no, nada.</summary>
    public static long? Predeterminada(IReadOnlyCollection<ClienteDatosSucursalDTO> sucursales, long? actual)
    {
        if (actual is > 0 && sucursales.Any(s => s.Id == actual)) return actual;
        return sucursales.Count == 1 ? sucursales.First().Id : null;
    }

    /// <summary>Mensaje de error si falta elegirla (o no es del cliente); null si todo está bien.</summary>
    public static string? Validar(IReadOnlyCollection<ClienteDatosSucursalDTO> sucursales, long? elegida)
    {
        if (elegida is > 0 && !sucursales.Any(s => s.Id == elegida))
            return "La sucursal elegida no pertenece al cliente.";
        if (Obligatoria(sucursales) && elegida is not > 0)
            return "El cliente tiene varias sucursales: seleccione la sucursal del cliente.";
        return null;
    }

    /// <summary>Texto del selector: "Sucursal - Nombre comercial" (sin repetir si son iguales).</summary>
    public static string Etiqueta(ClienteDatosSucursalDTO s)
    {
        var nombre = s.Sucursal?.Trim();
        var comercial = s.NombreComercial?.Trim();
        if (string.IsNullOrWhiteSpace(nombre)) return string.IsNullOrWhiteSpace(comercial) ? $"#{s.Id}" : comercial!;
        if (string.IsNullOrWhiteSpace(comercial) || string.Equals(nombre, comercial, StringComparison.OrdinalIgnoreCase)) return nombre;
        return $"{nombre} - {comercial}";
    }
}
