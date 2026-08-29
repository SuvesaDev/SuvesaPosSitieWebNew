using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IFacturacion" />
public sealed class Facturacion : ProxyBase, IFacturacion
{
    private readonly IVentaApiCliente _ventas;
    private readonly ITipoFacturaApiCliente _tipos;
    private readonly ICentrosApiCliente _centros;
    private readonly IUsuarioApiCliente _usuarios;

    public Facturacion(
        IVentaApiCliente ventas,
        ITipoFacturaApiCliente tipos,
        ICentrosApiCliente centros,
        IUsuarioApiCliente usuarios,
        IContextoSesion sesion,
        ILogger<Facturacion> log)
        : base(sesion, log)
    {
        _ventas = ventas;
        _tipos = tipos;
        _centros = centros;
        _usuarios = usuarios;
    }

    public Task<ResponseGeneric<ICollection<TipoFactura>>> Tipos()
        => Ejecutar(async () =>
        {
            var r = await _tipos.ObtenerTipoFacturasAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los tipos de factura");

    public Task<ResponseGeneric<ICollection<EmpresaDTO>>> Empresas()
        => Ejecutar(async () =>
        {
            var r = await _centros.ObtenerEmpresasFacturacionAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las empresas de facturacion");

    public Task<ResponseGeneric<Usuario>> ValidarClaveInterna(string contrasena)
        => Ejecutar(async () =>
        {
            // El contrato vigente del API recibe la clave como parametro de la
            // operacion. Se usa el endpoint que identifica al cajero por su clave,
            // igual que la ventana actual de facturacion.
            var r = await _usuarios.ValidarClaveInternaSinUsuarioAsync(contrasena);
            if (r.Status != ResponseStatus._0)
            {
                // El sitio actual traduce cualquier rechazo de este endpoint a este
                // mensaje de negocio, en vez de mostrar el detalle tecnico del API.
                return new ResponseGeneric<Usuario>("Contraseña incorrecta.");
            }
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "validar la clave interna de facturacion");

    public Task<ResponseGeneric<FacturaDTO>> Crear(FacturaDTO factura)
        => Ejecutar(async () =>
        {
            var r = await _ventas.CrearFactura2Async(factura);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la factura");
}
