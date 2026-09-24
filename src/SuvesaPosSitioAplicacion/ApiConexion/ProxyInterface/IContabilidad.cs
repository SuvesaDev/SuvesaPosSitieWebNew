using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Contabilidad General — W1 (feature flag, activación) + W2 (catálogo de
/// cuentas, dimensiones y plantillas versionadas). El resto de <c>api/contabilidad/*</c>
/// se agrega proxy por proxy en fases posteriores, no todo de una vez.</summary>
public interface IContabilidad
{
    Task<ResponseGeneric<EstadoActivacionContabilidadDTO>> EstadoActivacion(long idEmpresa, int? idEmisor);
    Task<ResponseGeneric<bool>> ActivarEmpresa(long idEmpresa);
    Task<ResponseGeneric<bool>> DesactivarEmpresa(long idEmpresa);
    Task<ResponseGeneric<long>> ActivarEmisor(ActivarEmisorDTO comando);
    Task<ResponseGeneric<bool>> DesactivarEmisor(int idEmisor);
    Task<ResponseGeneric<bool>> ConfirmarClave(string contrasena);

    // ---- W2: catálogo de cuentas, dimensiones y plantillas ----
    Task<ResponseGeneric<ICollection<CuentaContableDTO>>> ListarCuentas(long idLibroContable);
    Task<ResponseGeneric<CuentaContableDTO>> CrearCuenta(CrearCuentaContableDTO comando);
    Task<ResponseGeneric<CuentaContableDTO>> EditarCuenta(long idCuenta, EditarCuentaContableDTO comando);

    Task<ResponseGeneric<ICollection<DimensionContableDTO>>> ListarDimensiones(long idLibroContable);
    Task<ResponseGeneric<long>> CrearDimension(CrearDimensionContableDTO comando);
    Task<ResponseGeneric<ICollection<ValorDimensionContableDTO>>> ListarValoresDimension(long idDimension);
    Task<ResponseGeneric<long>> CrearValorDimension(CrearValorDimensionDTO comando);

    Task<ResponseGeneric<ICollection<PlantillaContableDTO>>> ListarPlantillas(long idLibroContable);
    Task<ResponseGeneric<long>> CrearPlantilla(CrearPlantillaContableDTO comando);
    Task<ResponseGeneric<ICollection<VersionPlantillaDTO>>> ListarVersiones(long idPlantilla);
    Task<ResponseGeneric<VersionPlantillaDetalleDTO>> ObtenerVersion(long idVersion);
    Task<ResponseGeneric<VersionPlantillaDTO>> CrearVersion(long idPlantilla, CrearVersionPlantillaDTO comando);
    Task<ResponseGeneric<VersionPlantillaDTO>> ActivarVersion(long idVersion);
    Task<ResponseGeneric<ResultadoSimulacionDTO>> Simular(long idVersion, string payloadJson);
}
