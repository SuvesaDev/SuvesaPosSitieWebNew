namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Decide la acción de la pantalla de Facturación a partir de la serie elegida
/// (PLAN_TIQUETE_RUTAS_FACTURACION_WEB.md §3). Función pura y testeable; la validación
/// financiera definitiva vive en el API. No deduce Tiquete de CodigoFe 04 ni del título
/// de la serie: usa el marcador EsTiquete del tipo y la naturaleza de la serie.
/// </summary>
public static class PoliticaRutaFacturacion
{
    public enum Ruta
    {
        /// <summary>No tiquete + contado: se guarda una preventa; el cobro ocurre en Cobrar.</summary>
        GuardarPreventaContado,
        /// <summary>No tiquete + crédito: se confirma la factura con plazo y saldo CxC.</summary>
        ConfirmarCredito,
        /// <summary>Tiquete + electrónico: modal de pagos 100% → venta/cobro → emisión + impresión.</summary>
        CobrarTiqueteElectronico,
        /// <summary>Tiquete + interno: modal de pagos 100% → venta/cobro → impresión interna.</summary>
        CobrarTiqueteInterno,
        /// <summary>Combinación no permitida o serie mal configurada.</summary>
        ConfiguracionInvalida,
    }

    /// <summary>Lo que la pantalla necesita saber de la serie seleccionada.</summary>
    public readonly record struct EntradaSerie(
        bool EsTiquete,
        bool EsCredito,
        bool RequiereDocumentoElectronico,
        bool EmisionV44Habilitada,
        string? CodigoFe);

    public readonly record struct Resultado(Ruta Ruta, string? Motivo)
    {
        public bool EsValida => Ruta != Ruta.ConfiguracionInvalida;
    }

    public static Resultado Resolver(EntradaSerie s)
    {
        // Una serie que exige documento electrónico pero no tiene la emisión V4.4
        // habilitada NO es interna: está incompleta y se bloquea (decisión A0 / W2).
        if (s.RequiereDocumentoElectronico && !s.EmisionV44Habilitada)
            return new(Ruta.ConfiguracionInvalida,
                "La serie requiere documento electrónico pero la emisión V4.4 no está habilitada.");

        var esInterna = !s.RequiereDocumentoElectronico;

        if (s.EsTiquete)
        {
            if (s.EsCredito)
                return new(Ruta.ConfiguracionInvalida, "Un tiquete no puede ser a crédito (se exige el 100% al confirmar).");
            if (!esInterna && s.CodigoFe != "04")
                return new(Ruta.ConfiguracionInvalida, "Un tiquete electrónico debe usar el código de comprobante 04.");
            return new(esInterna ? Ruta.CobrarTiqueteInterno : Ruta.CobrarTiqueteElectronico, null);
        }

        // No tiquete.
        if (!esInterna && s.CodigoFe != "01")
            return new(Ruta.ConfiguracionInvalida, "Una factura electrónica de venta debe usar el código de comprobante 01.");

        return s.EsCredito
            ? new(Ruta.ConfirmarCredito, null)
            : new(Ruta.GuardarPreventaContado, null);
    }

    /// <summary>Etiqueta del botón primario para cada ruta.</summary>
    public static string TextoAccion(Ruta ruta) => ruta switch
    {
        Ruta.GuardarPreventaContado => "Guardar preventa",
        Ruta.ConfirmarCredito => "Facturar a crédito",
        Ruta.CobrarTiqueteElectronico => "Cobrar",
        Ruta.CobrarTiqueteInterno => "Cobrar",
        _ => "Configuración inválida",
    };
}
