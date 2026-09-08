namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// Extiende el partial generado por NSwag (SeePosDtos.cs) — este archivo no se
// regenera, así que sobrevive a ./tools/actualizar-contratos.sh.
public partial class ConfiguracionBonificacion
{
    /// <summary>
    /// Unidades totales a facturar para completar el grupo: las que se pagan
    /// (<see cref="CantidadVenta"/>) más las que se regalan (<see cref="CantidadBonificable"/>).
    /// "Compra 10 lleva 1" factura 11 unidades (10 pagadas + 1 gratis), no 10.
    /// </summary>
    public int CantidadTotalGrupo => CantidadVenta + CantidadBonificable;
}
