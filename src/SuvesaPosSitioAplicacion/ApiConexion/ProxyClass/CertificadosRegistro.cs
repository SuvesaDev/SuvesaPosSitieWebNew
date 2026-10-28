using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Certificados;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICertificadosRegistro" />
public sealed class CertificadosRegistro : ProxyBase, ICertificadosRegistro
{
    private readonly HttpClient _api;

    public CertificadosRegistro(IHttpClientFactory factory, IContextoSesion sesion, ILogger<CertificadosRegistro> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    private static string Ruta(string dueno, long id)
        => $"api/certificados-registro/{(dueno == "proveedor" ? "proveedor" : "articulo")}/{id}";

    public Task<ResponseGeneric<List<CertificadoRegistroWebDTO>>> Listar(string dueno, long id, bool historial)
        => Ejecutar(async () => await LecturaEnvelope.Leer<List<CertificadoRegistroWebDTO>>(
            await _api.GetAsync($"{Ruta(dueno, id)}?historial={historial.ToString().ToLowerInvariant()}")), "consultar los certificados de registro");

    public Task<ResponseGeneric<CertificadoRegistroWebDTO>> Guardar(string dueno, long id, CertificadoRegistroWebDTO certificado)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CertificadoRegistroWebDTO>(
            await _api.PostAsJsonAsync(Ruta(dueno, id), certificado, LecturaEnvelope.Json)), "guardar el certificado de registro");

    public Task<ResponseGeneric<CertificadoArchivoWebDTO>> Archivo(string dueno, long id, long idCertificado)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CertificadoArchivoWebDTO>(
            await _api.GetAsync($"{Ruta(dueno, id)}/{idCertificado}/archivo")), "descargar el certificado");

    public Task<ResponseGeneric<bool>> DarDeBaja(string dueno, long id, long idCertificado)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PostAsync($"{Ruta(dueno, id)}/{idCertificado}/baja", null)), "dar de baja el certificado");
}
