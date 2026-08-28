using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IClientesConsulta" />
public sealed class ClientesConsulta : ProxyBase, IClientesConsulta
{
    private readonly IClienteApiCliente _api;

    public ClientesConsulta(
        IClienteApiCliente api,
        IContextoSesion sesion,
        ILogger<ClientesConsulta> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public async Task<ResponseGeneric<ICollection<FiltranClienteDTO>>> Buscar(string texto)
    {
        var limpio = texto?.Trim() ?? string.Empty;

        if (limpio.Length < 2)
        {
            return new ResponseGeneric<ICollection<FiltranClienteDTO>>(
                new List<FiltranClienteDTO>());
        }

        // Solo digitos se busca por cedula; si no, por nombre. Mismo criterio que en
        // la consulta de inventario: el usuario no tiene que elegir el modo.
        var porCedula = limpio.All(char.IsDigit);

        var peticion = new BuscarClienteDTO
        {
            Cedula = porCedula ? limpio : null,
            Nombre = porCedula ? null : limpio
        };

        return await Ejecutar(async () =>
        {
            var r = porCedula
                ? await _api.BuscarCedulaAsync(peticion)
                : await _api.BuscarNombreAsync(peticion);

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, $"buscar clientes con {limpio}");
    }
}
