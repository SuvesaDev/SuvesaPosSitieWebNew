namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// Credenciales del usuario de pruebas. Se leen del entorno, nunca del codigo:
/// asi no acaban en el repositorio ni en el historial.
///
///   export SEEPOS_USUARIO=...
///   export SEEPOS_PASSWORD=...
///   export SEEPOS_API=https://devapi.pos2650.com   # opcional
///
/// Sin ellas, las pruebas que las necesitan se omiten en lugar de fallar.
/// </summary>
public static class CredencialesPrueba
{
    public static string? Usuario => Environment.GetEnvironmentVariable("SEEPOS_USUARIO");

    public static string? Password => Environment.GetEnvironmentVariable("SEEPOS_PASSWORD");

    public static string Api =>
        Environment.GetEnvironmentVariable("SEEPOS_API") ?? "https://devapi.pos2650.com";

    public static bool Hay =>
        !string.IsNullOrWhiteSpace(Usuario) && !string.IsNullOrWhiteSpace(Password);

    public const string Motivo =
        "Requiere SEEPOS_USUARIO y SEEPOS_PASSWORD en el entorno.";
}

/// <summary>Hecho que solo corre si hay credenciales configuradas.</summary>
public sealed class HechoConCredencialesAttribute : FactAttribute
{
    public HechoConCredencialesAttribute()
    {
        if (!CredencialesPrueba.Hay)
        {
            Skip = CredencialesPrueba.Motivo;
        }
    }
}
