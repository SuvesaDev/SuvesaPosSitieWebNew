using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Arma el membrete (emisor, sucursal, logo) del centro de la sesión para los
/// reportes tabulares del sitio. Misma regla que el API: emisor único con serie
/// de facturación en la sucursal; si hay varios, el primero con nombre.
/// </summary>
public sealed class MembreteReporte
{
    private readonly IContextoSesion _sesion;
    private readonly ISeriesFacturacionFiscales _series;
    private readonly IEmisoresFiscales _emisores;

    public MembreteReporte(
        IContextoSesion sesion,
        ISeriesFacturacionFiscales series,
        IEmisoresFiscales emisores)
    {
        _sesion = sesion;
        _series = series;
        _emisores = emisores;
    }

    public async Task<EncabezadoEmpresaPdf?> ResolverAsync()
    {
        await _sesion.CargarAsync();
        var idSucursal = _sesion.IdSucursal;
        if (idSucursal <= 0) return null;

        var series = await _series.Obtener();
        if (!series.EsCorrecta || series.Responses is null) return null;

        var deSucursal = series.Responses.Where(s => s.IdSucursal == idSucursal).ToList();
        var idsEmisor = deSucursal.Select(s => s.IdEmisor).Where(id => id > 0).Distinct().ToList();
        if (idsEmisor.Count == 0) return null;

        // Paridad con MembreteCongelado.EmisorUnicoDeSucursal: si hay uno solo, ese;
        // si hay varios, el de la primera serie con nombre (no inventar elección).
        var idEmisor = idsEmisor.Count == 1
            ? idsEmisor[0]
            : deSucursal.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.EmisorNombre))?.IdEmisor
              ?? idsEmisor[0];

        var serie = deSucursal.FirstOrDefault(s => s.IdEmisor == idEmisor);
        var nombreEmisor = serie?.EmisorNombre;
        var identificacion = serie?.EmisorIdentificacion;
        var nombreSucursal = !string.IsNullOrWhiteSpace(serie?.SucursalNombre)
            ? serie!.SucursalNombre
            : _sesion.NombreSucursal;

        string? telefono = null;
        string? correo = null;
        string? direccion = null;

        var emisores = await _emisores.Obtener();
        if (emisores.EsCorrecta && emisores.Responses is not null)
        {
            var emisor = emisores.Responses.FirstOrDefault(e => e.Id == idEmisor);
            if (emisor is not null)
            {
                if (string.IsNullOrWhiteSpace(nombreEmisor)) nombreEmisor = emisor.Nombre;
                if (string.IsNullOrWhiteSpace(identificacion)) identificacion = emisor.Identificacion;
                telefono = emisor.Telefono;
                correo = emisor.Correo;
                direccion = emisor.OtrasSeñas;
            }
        }

        if (string.IsNullOrWhiteSpace(nombreEmisor)) return null;

        byte[]? logo = null;
        var logoR = await _emisores.DescargarLogo(idEmisor);
        if (logoR.EsCorrecta && logoR.Responses is { Contenido.Length: > 0 } archivo)
            logo = archivo.Contenido;

        return new EncabezadoEmpresaPdf(
            NombreEmisor: nombreEmisor!,
            Identificacion: identificacion,
            NombreSucursal: nombreSucursal,
            Telefono: telefono,
            Correo: correo,
            Direccion: direccion,
            Logo: logo);
    }
}
