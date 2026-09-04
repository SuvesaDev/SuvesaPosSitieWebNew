namespace SuvesaPosSitioAplicacion.DTOs.Cobros;

/// <summary>
/// Forma de pago con sus propiedades semánticas (SANEAMIENTO Fase 8.1).
/// Espejo de <c>ApiSuvesaPos.DTOs.FormasPagoDTO</c> — los nombres de propiedad
/// serializan a las claves que el API espera (<c>codigo</c>, <c>descripcion</c>, …).
/// </summary>
public class FormaPagoConfigWebDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public bool Efectivo { get; set; }
    public bool Tarjeta { get; set; }
    public bool Deposito { get; set; }
    public bool Cheque { get; set; }

    public bool? Activa { get; set; }
    public int? Orden { get; set; }
    public bool? PermiteVuelto { get; set; }
    public bool? RequiereReferencia { get; set; }
    public bool? AfectaCaja { get; set; }
    public bool? PermiteMonedaExtranjera { get; set; }
    public string? CodigoHacienda { get; set; }
}
