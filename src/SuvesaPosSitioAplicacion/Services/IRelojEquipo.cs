namespace SuvesaPosSitioAplicacion.Services;

/// <summary>Reloj del equipo que usa la pantalla, no el reloj del servidor Blazor.</summary>
public interface IRelojEquipo
{
    Task<DateTime> AhoraAsync(CancellationToken cancellationToken = default);
    Task<DateTime> HoyAsync(CancellationToken cancellationToken = default);

    /// <summary>Guarda la hora de pared del navegador para la cabecera de las llamadas al API.</summary>
    Task PublicarMarcaAsync(CancellationToken cancellationToken = default);
}
