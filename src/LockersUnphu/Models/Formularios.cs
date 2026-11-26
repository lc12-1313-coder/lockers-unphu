using System.ComponentModel.DataAnnotations;

namespace LockersUnphu.Models;

public sealed class IngresoForm
{
    [Required(ErrorMessage = "Escribe tu usuario o matrícula.")]
    [StringLength(64)]
    [Display(Name = "Usuario o matrícula")]
    public string Usuario { get; set; } = "";

    [Required(ErrorMessage = "Escribe tu contraseña.")]
    [StringLength(128)]
    [DataType(DataType.Password), Display(Name = "Contraseña")]
    public string Clave { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public sealed class ClaveForm
{
    [Required(ErrorMessage = "Escribe tu contraseña actual.")]
    [StringLength(128)]
    [DataType(DataType.Password), Display(Name = "Contraseña actual")]
    public string Actual { get; set; } = "";

    [Required(ErrorMessage = "Escribe tu nueva contraseña.")]
    [StringLength(128, MinimumLength = 12, ErrorMessage = "Usa entre 12 y 128 caracteres.")]
    [DataType(DataType.Password), Display(Name = "Nueva contraseña")]
    public string Nueva { get; set; } = "";
}

