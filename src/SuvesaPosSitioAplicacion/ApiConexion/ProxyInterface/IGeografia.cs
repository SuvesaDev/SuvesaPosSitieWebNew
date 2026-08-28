using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Catálogos geográficos dependientes usados por los formularios.</summary>
public interface IGeografia
{
    Task<ResponseGeneric<ICollection<ProvinciaDTO>>> Provincias();

    Task<ResponseGeneric<ICollection<CantonDTO>>> Cantones(int provincia);

    Task<ResponseGeneric<ICollection<DistritoDTO>>> Distritos(int canton);
}
