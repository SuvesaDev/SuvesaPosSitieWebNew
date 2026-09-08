using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Operaciones transaccionales de la facturación de venta.</summary>
public interface IFacturacion
{
    Task<ResponseGeneric<ICollection<TipoFactura>>> Tipos();
    Task<ResponseGeneric<ICollection<EmpresaDTO>>> Empresas();
    Task<ResponseGeneric<Usuario>> ValidarClaveInterna(string contrasena);
    Task<ResponseGeneric<FacturaDTO>> Crear(FacturaDTO factura);

    /// <summary>Catálogo de agentes de venta, para la condición "Agente" del encabezado.</summary>
    Task<ResponseGeneric<ICollection<AgenteVendedorDTO>>> Agentes();
}
