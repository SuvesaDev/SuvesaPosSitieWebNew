namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Comprueba si la SPA React esta realmente disponible.
///
/// Sin esto, cuando no lo esta el usuario ve un iframe **en blanco y sin ninguna
/// explicacion**: la pantalla parece rota y no hay forma de saber por que.
/// </summary>
public interface ISondaLegado
{
    Task<bool> EstaDisponibleAsync(CancellationToken token = default);
}
