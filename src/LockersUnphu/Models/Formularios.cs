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

public sealed class EstudianteForm
{
    [Required(ErrorMessage = "Escribe la matrícula.")]
    [RegularExpression(@"[A-Za-z0-9-]{1,24}", ErrorMessage = "Usa hasta 24 letras, números o guiones.")]
    [Display(Name = "Matrícula")]
    public string Matricula { get; set; } = "";

    [Required(ErrorMessage = "Escribe el nombre.")]
    [StringLength(120, ErrorMessage = "Usa hasta 120 caracteres.")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Escribe una contraseña inicial.")]
    [StringLength(128, MinimumLength = 12, ErrorMessage = "Usa entre 12 y 128 caracteres.")]
    [DataType(DataType.Password), Display(Name = "Contraseña inicial")]
    public string Clave { get; set; } = "";
}

public sealed class EquipoForm
{
    [Required(ErrorMessage = "Escribe el nombre de usuario.")]
    [RegularExpression(@"[A-Za-z0-9._-]{1,64}", ErrorMessage = "Usa hasta 64 letras, números, puntos, guiones o guiones bajos.")]
    public string Usuario { get; set; } = "";

    [Required(ErrorMessage = "Escribe una contraseña inicial.")]
    [StringLength(128, MinimumLength = 12, ErrorMessage = "Usa entre 12 y 128 caracteres.")]
    [DataType(DataType.Password), Display(Name = "Contraseña inicial")]
    public string Clave { get; set; } = "";

    [Display(Name = "Rol")]
    public string Rol { get; set; } = "";
}
