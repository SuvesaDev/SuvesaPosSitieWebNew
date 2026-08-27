using SuvesaPosSitioAplicacion.Models;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Donde persisten las pestanas entre recargas del navegador.
///
/// Es una interfaz y no una llamada directa a ProtectedLocalStorage para que la
/// logica del espacio de trabajo se pueda probar sin navegador, y para poder
/// cambiar el almacen sin tocarla.
/// </summary>
public interface IAlmacenEspacioTrabajo
{
    Task<EstadoEspacioGuardado?> LeerAsync();

    Task GuardarAsync(EstadoEspacioGuardado estado);
}

/// <summary>Lo que se guarda entre recargas.</summary>
public sealed record EstadoEspacioGuardado(
    List<PestanaTrabajo> Pestanas,
    string? IdActual,
    int UltimaVenta);
