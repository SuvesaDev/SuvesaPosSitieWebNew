using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (mismo motivo que FacturaDTO.Condicion.cs).
// Al regenerar, el generado ya trae `idBodega` y este archivo se borra.
//
// Si viene, la Existencia devuelta por la búsqueda es la de esta bodega
// (última fila de Stocks), no el acumulado global del artículo.

public partial class BuscarInventarioDTO
{
    [JsonPropertyName("idBodega")]
    public int? IdBodega { get; set; }
}
