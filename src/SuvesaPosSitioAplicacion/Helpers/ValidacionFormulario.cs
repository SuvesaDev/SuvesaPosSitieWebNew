namespace SuvesaPosSitioAplicacion.Helpers;

/// <summary>Mensajes de campos faltantes para formularios de alta/edición.</summary>
public static class ValidacionFormulario
{
    public static string MensajeCamposFaltantes(IReadOnlyList<string> campos)
    {
        if (campos.Count == 0)
        {
            return "Revise los campos obligatorios marcados con *.";
        }

        if (campos.Count == 1)
        {
            return $"El campo «{campos[0]}» es obligatorio.";
        }

        return "Complete los campos obligatorios:\n• " + string.Join("\n• ", campos);
    }
}
