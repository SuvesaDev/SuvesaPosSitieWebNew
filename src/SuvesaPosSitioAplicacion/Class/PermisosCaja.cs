using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Class;

/// <summary>
/// Quién ve las cajas de todos los cajeros. Es la misma regla que aplica el API: el administrador, quien
/// tenga el permiso «Consulta de Arqueos» o quien pueda cerrar cajas. El resto ve solo lo de sus cajas,
/// identificado por su clave interna.
/// </summary>
public static class PermisosCaja
{
    public static bool VeTodasLasCajas(IContextoSesion sesion)
        => sesion.EsSuperAdministrador
           || sesion.PuedeVer("CAJA.CONSULTA_DE_ARQUEOS")
           || sesion.Puede("CAJA.CONCILIACION_DE_CAJA", AccionPantalla.Crear);
}
