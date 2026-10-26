namespace SuvesaPosSitioAplicacion.DTOs.Caja;

/// <summary>
/// Lo que el sistema tiene registrado en la apertura, por moneda. Es solo una referencia
/// para el cajero: las diferencias se ven en el cierre, no en el arqueo.
/// </summary>
public sealed class ArqueoReferenciaSistemaWebDTO
{
    public long NumApertura { get; init; }
    public List<ArqueoReferenciaMonedaWebDTO> Monedas { get; init; } = new();

    private static readonly ArqueoReferenciaMonedaWebDTO Vacia = new();

    public ArqueoReferenciaMonedaWebDTO Colones => Monedas.FirstOrDefault(m => m.CodMoneda == 1) ?? Vacia;

    /// <summary>Todo lo que no es colones se trata como dólares, igual que el resto del arqueo.</summary>
    public ArqueoReferenciaMonedaWebDTO Dolares => new()
    {
        CodMoneda = 2,
        EfectivoNeto = Monedas.Where(m => m.CodMoneda != 1).Sum(m => m.EfectivoNeto),
        Tarjetas = Monedas.Where(m => m.CodMoneda != 1).Sum(m => m.Tarjetas),
        ChequesConfirmados = Monedas.Where(m => m.CodMoneda != 1).Sum(m => m.ChequesConfirmados),
        ChequesPendientes = Monedas.Where(m => m.CodMoneda != 1).Sum(m => m.ChequesPendientes),
        PreDepositos = Monedas.Where(m => m.CodMoneda != 1).Sum(m => m.PreDepositos),
        PagosDepositoConfirmados = Monedas.Where(m => m.CodMoneda != 1).Sum(m => m.PagosDepositoConfirmados),
        DepositosPendientes = Monedas.Where(m => m.CodMoneda != 1).Sum(m => m.DepositosPendientes),
    };
}

public sealed class ArqueoReferenciaMonedaWebDTO
{
    public int CodMoneda { get; init; }
    public decimal EfectivoNeto { get; init; }
    public decimal Tarjetas { get; init; }
    public decimal ChequesConfirmados { get; init; }
    public decimal ChequesPendientes { get; init; }
    public decimal PreDepositos { get; init; }
    public decimal PagosDepositoConfirmados { get; init; }
    public decimal DepositosPendientes { get; init; }

    /// <summary>Referencia del renglón «Depósitos»: pre-depósitos enviados más pagos de depósito confirmados.</summary>
    public decimal DepositosConfirmados => PreDepositos + PagosDepositoConfirmados;
}
