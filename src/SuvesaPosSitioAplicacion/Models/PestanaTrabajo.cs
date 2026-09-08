namespace SuvesaPosSitioAplicacion.Models;

/// <summary>
/// Una pestana abierta del espacio de trabajo.
///
/// El sistema actual identifica las pestanas por su nombre y las busca con
/// <c>name.includes(...)</c>, lo que hace que cerrar "Clientes" arrastre tambien a
/// "Clientes Frecuentes" (de ahi los casos especiales que tiene el reducer).
/// Aqui cada pestana lleva un <see cref="Id"/> estable y las operaciones van por id.
/// Mismo comportamiento visible, sin la fragilidad.
/// </summary>
public sealed record PestanaTrabajo
{
    public required string Id { get; init; }

    /// <summary>Lo que ve el usuario. Para las ventas incluye el numero: "Venta # 2".</summary>
    public required string Titulo { get; init; }

    public required string Ruta { get; init; }

    /// <summary>Las ventas son las unicas que admiten varias pestanas a la vez.</summary>
    public bool EsVenta { get; init; }

    /// <summary>Numero de venta, para el titulo y la ruta. Cero si no es una venta.</summary>
    public int Numero { get; init; }
}
