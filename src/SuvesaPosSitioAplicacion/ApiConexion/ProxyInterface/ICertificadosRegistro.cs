using SuvesaPosSitioAplicacion.DTOs.Certificados;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Certificados de registro de artículos y proveedores (laboratorios), con PDF, vencimiento y aviso.</summary>
public interface ICertificadosRegistro
{
    /// <param name="dueno">«articulo» o «proveedor».</param>
    Task<ResponseGeneric<List<CertificadoRegistroWebDTO>>> Listar(string dueno, long id, bool historial);

    Task<ResponseGeneric<CertificadoRegistroWebDTO>> Guardar(string dueno, long id, CertificadoRegistroWebDTO certificado);

    Task<ResponseGeneric<CertificadoArchivoWebDTO>> Archivo(string dueno, long id, long idCertificado);

    Task<ResponseGeneric<bool>> DarDeBaja(string dueno, long id, long idCertificado);
}
