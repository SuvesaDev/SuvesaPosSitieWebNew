using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Entidades bancarias. Catalogo simple: listar, crear, editar, activar y desactivar.</summary>
public interface IBancos
{
    Task<ResponseGeneric<ICollection<EntidadesBancariasDTO>>> Obtener();

    Task<ResponseGeneric<EntidadesBancariasDTO>> ObtenerPorId(int id);

    Task<ResponseGeneric<EntidadesBancariasDTO>> Crear(EntidadesBancariasDTO banco);

    Task<ResponseGeneric<EntidadesBancariasDTO>> Editar(EntidadesBancariasDTO banco);

    Task<ResponseGeneric<EntidadesBancariasDTO>> Activar(int id);

    Task<ResponseGeneric<EntidadesBancariasDTO>> Inactivar(int id);

    // --- Cuentas bancarias del banco (necesarias para depósitos y recibos CxP) ---
    Task<ResponseGeneric<ICollection<CuentaBancariaDTO>>> Cuentas(int idBanco, int idEmpresa = 0);

    Task<ResponseGeneric<CuentaBancariaDTO>> CrearCuenta(CuentaBancariaDTO cuenta);

    Task<ResponseGeneric<CuentaBancariaDTO>> EditarCuenta(CuentaBancariaDTO cuenta);

    Task<ResponseGeneric<CuentaBancariaDTO>> InactivarCuenta(int idCuenta);
}
