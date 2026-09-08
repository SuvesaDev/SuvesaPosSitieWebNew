using Microsoft.AspNetCore.Http;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>Orquesta el inicio y cierre de sesion. Solo se puede llamar desde render estatico.</summary>
public interface IServicioAutenticacion
{
    /// <summary>Valida credenciales contra el API y establece la sesion, todavia sin sucursal.</summary>
    Task<Response> IngresarAsync(HttpContext contexto, string usuario, string password);

    /// <summary>Anade la sucursal elegida a la sesion ya iniciada.</summary>
    Task<Response> EstablecerSucursalAsync(HttpContext contexto, SucursalDTO sucursal);

    Task SalirAsync(HttpContext contexto);
}
