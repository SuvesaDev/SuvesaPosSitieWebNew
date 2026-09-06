using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>
/// Marca "ocupado" el circuito mientras una llamada al API está en vuelo, para que
/// el layout muestre la barra de carga. Resuelve el <see cref="IEstadoOcupado"/> del
/// circuito vía <see cref="AccesorServiciosCircuito"/>; si no hay circuito (p. ej.
/// render estático) simplemente no hace nada.
/// </summary>
public sealed class OcupadoHandler(AccesorServiciosCircuito accesor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage solicitud, CancellationToken ct)
    {
        if (accesor.Servicios?.GetService(typeof(IEstadoOcupado)) is not IEstadoOcupado estado)
            return await base.SendAsync(solicitud, ct);

        using (estado.Rastrear())
            return await base.SendAsync(solicitud, ct);
    }
}
