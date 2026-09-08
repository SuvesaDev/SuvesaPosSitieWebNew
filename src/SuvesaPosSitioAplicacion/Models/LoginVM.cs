using System.ComponentModel.DataAnnotations;

namespace SuvesaPosSitioAplicacion.Models;

/// <summary>Datos del formulario de inicio de sesion. Se completa en la semana 2.</summary>
public sealed class LoginVM
{
    [Required(ErrorMessage = "Indique el usuario.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indique la contrasena.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Se elige despues de autenticar, entre las sucursales que devuelve el API.</summary>
    public int IdSucursal { get; set; }
}
