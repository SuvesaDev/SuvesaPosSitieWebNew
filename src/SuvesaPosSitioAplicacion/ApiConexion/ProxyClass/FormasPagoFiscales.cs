using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
public sealed class FormasPagoFiscales : ProxyBase, IFormasPagoFiscales { private readonly IFormasPagosApiCliente _api; public FormasPagoFiscales(IFormasPagosApiCliente api,IContextoSesion sesion,ILogger<FormasPagoFiscales> log):base(sesion,log)=>_api=api; public Task<ResponseGeneric<ICollection<FormasPagoDTO>>> Obtener()=>Ejecutar(async()=>{var r=await _api.ObtenerFormasDePagoSinClienteAsync();return EnvelopeApi.A(r.Status,r.CurrentException,r.ValidationErrors,r.Responses);},"consultar las formas de pago"); public Task<ResponseGeneric<FormasPagoDTO>> Crear(FormasPagoDTO f)=>Ejecutar(async()=>{var r=await _api.CreateAsync(f);return EnvelopeApi.A(r.Status,r.CurrentException,r.ValidationErrors,r.Responses);},"crear la forma de pago"); public Task<ResponseGeneric<FormasPagoDTO>> Actualizar(FormasPagoDTO f)=>Ejecutar(async()=>{var r=await _api.UpdateAsync(f);return EnvelopeApi.A(r.Status,r.CurrentException,r.ValidationErrors,r.Responses);},"actualizar la forma de pago"); }
