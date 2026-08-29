using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Operaciones de caja: aperturas, arqueos, cierres y depósitos.</summary>
public interface ICajaOperaciones
{
    Task<ResponseGeneric<Usuario>> ValidarClaveInterna(string contrasena);
    Task<ResponseGeneric<ICollection<CajasCantidad>>> CajasDisponibles();
    Task<ResponseGeneric<ICollection<DenominacionMonedum>>> Denominaciones();
    Task<ResponseGeneric<ICollection<User>>> CajerosConCajaAbierta();
    Task<ResponseGeneric<ICollection<AperturaCajaDTO>>> AperturasSinCerrar();
    Task<ResponseGeneric<ICollection<ObtenerAperturaCajaDTO>>> AperturasSinArqueo();
    Task<ResponseGeneric<AperturaCajaDTO>> CrearApertura(AperturaCajaDTO apertura);
    Task<ResponseGeneric<ArqueoCajaDTO>> CrearArqueo(ArqueoCajaDTO arqueo);
    Task<ResponseGeneric<ObtenerDatosCierreCaja>> DatosCierre(long numeroApertura);
    Task<ResponseGeneric<CierreCajaDTO>> CrearCierre(CierreCajaDTO cierre);
    Task<ResponseGeneric<ICollection<EntidadesBancariasDTO>>> Bancos();
    Task<ResponseGeneric<ICollection<EmpresaDTO>>> Empresas();
    Task<ResponseGeneric<ICollection<CuentaBancariaDTO>>> Cuentas(int banco, int empresa);
    Task<ResponseGeneric<ICollection<PreDepositosDTO>>> PreDepositosDeApertura(long apertura);
    Task<ResponseGeneric<ICollection<PreDepositosBuscarDTO>>> BuscarPreDepositos(FiltroBusquedaPreDepositosDTO filtro);
    Task<ResponseGeneric<ICollection<DepositosBuscarDTO>>> BuscarDepositos(FiltroBusquedaDepositosDTO filtro);
    Task<ResponseGeneric<PreDepositosDTO>> CrearPreDeposito(PreDepositosDTO deposito);
    Task<ResponseGeneric<PreDepositosDTO>> EliminarPreDeposito(int id);
    Task<ResponseGeneric<DepositosDTO>> CrearDeposito(DepositosDTO deposito);
}
