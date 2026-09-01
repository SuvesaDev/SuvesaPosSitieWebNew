using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IFamilias" />
///
/// NOTA: GetFamiliasAsync devuelve ObjectResponseGeneric.Responses tipado como
/// `object`, no como ICollection&lt;FamiliaDTO&gt;. El swagger declara la respuesta
/// como generica en lugar de tipada; verificado contra devapi.pos2650.com que el
/// contenido real SI es un arreglo de FamiliaDTO. Se deserializa aqui a mano.
public sealed class Familias : ProxyBase, IFamilias
{
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);

    private readonly IFamiliasApiCliente _api;
    private readonly ISubFamiliasApiCliente? _subFamilias;

    public Familias(IFamiliasApiCliente api, ISubFamiliasApiCliente subFamilias, IContextoSesion sesion, ILogger<Familias> log)
        : base(sesion, log)
    {
        _api = api;
        _subFamilias = subFamilias;
    }

    // Conserva la firma usada por las pruebas de deserialización y por cualquier
    // consumidor que solo necesite Familias. Las operaciones de sub-familias
    // requieren el constructor completo registrado por DI.
    public Familias(IFamiliasApiCliente api, IContextoSesion sesion, ILogger<Familias> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<FamiliaDTO>>> Obtener()
        => Ejecutar(async () =>
        {
            var r = await _api.GetFamiliasAsync();

            if (r.Status != ResponseStatus._0)
            {
                return EnvelopeApi.A<ICollection<FamiliaDTO>>(
                    r.Status, r.CurrentException, r.ValidationErrors, null);
            }

            // r.Responses es un JsonElement (object sin tipar): se reinterpreta.
            var lista = ADatos(r.Responses);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, lista);
        }, "consultar las familias");

    public Task<ResponseGeneric<FamiliaDTO>> Crear(FamiliaDTO familia)
        => Ejecutar(async () =>
        {
            var r = await _api.CreateFamiliaAsync(familia);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la familia");

    public Task<ResponseGeneric<FamiliaDTO>> Editar(FamiliaDTO familia)
        => Ejecutar(async () =>
        {
            var r = await _api.EditFamiliaAsync(familia);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar la familia");

    public Task<ResponseGeneric<bool>> Eliminar(int codigo)
        => Ejecutar(async () =>
        {
            var r = await _api.DeleteFamiliaAsync(codigo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "eliminar la familia");

    public Task<ResponseGeneric<ICollection<SubFamiliaDTO>>> ObtenerSubFamilias(int codigoFamilia)
        => Ejecutar(async () =>
        {
            var r = await SubFamilias().GetSubFamiliasToFamiliasAsync(codigoFamilia);
            if (r.Status != ResponseStatus._0)
            {
                return EnvelopeApi.A<ICollection<SubFamiliaDTO>>(r.Status, r.CurrentException, r.ValidationErrors, null);
            }
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, ASubFamilias(r.Responses));
        }, "consultar las sub-familias");

    public Task<ResponseGeneric<SubFamiliaDTO>> CrearSubFamilia(SubFamiliaDTO subFamilia)
        => Ejecutar(async () =>
        {
            var r = await SubFamilias().PostCreateSubFamiliaAsync(subFamilia);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la sub-familia");

    public Task<ResponseGeneric<SubFamiliaDTO>> EditarSubFamilia(SubFamiliaDTO subFamilia)
        => Ejecutar(async () =>
        {
            var r = await SubFamilias().PutSubFamiliaAsync(subFamilia);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar la sub-familia");

    public Task<ResponseGeneric<bool>> EliminarSubFamilia(int codigo)
        => Ejecutar(async () =>
        {
            var r = await SubFamilias().DeleteSubFamiliaAsync(codigo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "eliminar la sub-familia");

    private static ICollection<FamiliaDTO> ADatos(object? responses)
    {
        if (responses is null)
        {
            return new List<FamiliaDTO>();
        }

        // System.Text.Json entrega un JsonElement cuando el tipo declarado es object.
        var json = JsonSerializer.Serialize(responses, Opciones);
        return JsonSerializer.Deserialize<List<FamiliaDTO>>(json, Opciones)
               ?? new List<FamiliaDTO>();
    }

    private static ICollection<SubFamiliaDTO> ASubFamilias(object? responses)
    {
        if (responses is null) return new List<SubFamiliaDTO>();
        var json = JsonSerializer.Serialize(responses, Opciones);
        return JsonSerializer.Deserialize<List<SubFamiliaDTO>>(json, Opciones)
               ?? new List<SubFamiliaDTO>();
    }

    private ISubFamiliasApiCliente SubFamilias() => _subFamilias
        ?? throw new InvalidOperationException("El cliente de sub-familias no fue configurado.");
}
