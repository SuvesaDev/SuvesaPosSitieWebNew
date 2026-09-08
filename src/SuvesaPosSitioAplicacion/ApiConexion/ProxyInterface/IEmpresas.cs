using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Alta del emisor electronico (datos fiscales, certificado de firma, cuentas
/// bancarias). Igual que Sucursales: el sistema actual SOLO da de alta, no hay
/// edicion para esta pantalla.
/// </summary>
public interface IEmpresas
{
    Task<ResponseGeneric<ICollection<TipoIdentificacionDTO>>> TiposIdentificacion();

    Task<ResponseGeneric<ICollection<ProvinciaDTO>>> Provincias();

    Task<ResponseGeneric<ICollection<CantonDTO>>> Cantones(int idProvincia);

    Task<ResponseGeneric<ICollection<DistritoDTO>>> Distritos(int idCanton);

    Task<ResponseGeneric<ICollection<EntidadesBancariasDTO>>> Bancos();

    Task<ResponseGeneric<ICollection<Moneda>>> Monedas();

    /// <summary>Consulta las actividades economicas registradas en Hacienda para esa cedula.</summary>
    Task<ResponseGeneric<ICollection<ActividadesEmpresaDTO>>> ActividadesHacienda(string identificacion);

    Task<ResponseGeneric<EmpresaDTO>> Crear(EmpresaDTO empresa);
}
