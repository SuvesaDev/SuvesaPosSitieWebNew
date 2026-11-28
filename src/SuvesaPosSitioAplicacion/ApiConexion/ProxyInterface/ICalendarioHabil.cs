using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Calendario hábil (lunes a viernes menos feriados y cierres) de la proyección de cierre y las metas.</summary>
public interface ICalendarioHabil
{
    Task<ResponseGeneric<CalendarioHabilAnioDTO>> ObtenerAnio(int anio);
    Task<ResponseGeneric<DiaNoHabilDTO>> GuardarDia(DiaNoHabilDTO dia);
    /// <summary>Un día manual se borra; un feriado se desactiva y puede reactivarse.</summary>
    Task<ResponseGeneric<bool>> QuitarDia(DateTime fecha);
    /// <summary>Propone los feriados de Costa Rica del año sin pisar lo ya registrado; devuelve los nuevos.</summary>
    Task<ResponseGeneric<IReadOnlyList<DiaNoHabilDTO>>> GenerarFeriados(int anio);
}
