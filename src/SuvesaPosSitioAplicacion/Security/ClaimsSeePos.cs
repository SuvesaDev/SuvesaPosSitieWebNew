namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Tipos de claim propios. Van dentro del ticket de autenticacion, que se guarda
/// en servidor (ver <see cref="AlmacenTickets"/>): el navegador solo recibe la llave.
/// </summary>
public static class ClaimsSeePos
{
    public const string Token = "seepos:token";
    public const string Expiracion = "seepos:expiracion";
    public const string Administrador = "seepos:administrador";
    public const string IdSucursal = "seepos:idSucursal";
    public const string NombreSucursal = "seepos:nombreSucursal";
    public const string IdRol = "seepos:idRol";
    public const string NombreRol = "seepos:nombreRol";
    public const string CostaPets = "seepos:costaPets";
    public const string AgenteCostaPets = "seepos:agenteCostaPets";
    public const string AceptaConsignacion = "seepos:aceptaConsignacion";

    /// <summary>Un claim por pantalla. Valor: "menu|pantalla|ver|crear|modificar|borrar".</summary>
    public const string Permiso = "seepos:permiso";
}
