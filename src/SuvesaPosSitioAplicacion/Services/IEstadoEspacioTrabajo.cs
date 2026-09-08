using SuvesaPosSitioAplicacion.Models;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// El espacio de trabajo por pestanas. Es la pieza a medida del sistema: ninguna
/// libreria la trae, y es lo que define como se usa el punto de venta.
///
/// Scope de circuito. Se restaura del navegador al abrir y se guarda en cada cambio.
/// </summary>
public interface IEstadoEspacioTrabajo
{
    IReadOnlyList<PestanaTrabajo> Pestanas { get; }
    PestanaTrabajo? Actual { get; }

    /// <summary>Se dispara cuando cambian las pestanas o la seleccionada.</summary>
    event Action? Cambio;

    /// <summary>
    /// Abre la pantalla. Si ya hay una pestana para esa ruta la selecciona, salvo
    /// que sea una venta: esas siempre abren una nueva.
    /// </summary>
    PestanaTrabajo Abrir(string titulo, string ruta);

    void Seleccionar(string id);

    /// <summary>Cierra la pestana. Si era la activa, pasa a la anterior.</summary>
    void Cerrar(string id);

    void CerrarTodas();

    Task RestaurarAsync();
}
