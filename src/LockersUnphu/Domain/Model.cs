using Microsoft.AspNetCore.Identity;

namespace LockersUnphu.Domain;

public sealed class ApplicationUser : IdentityUser;

public static class Roles
{
    public const string Estudiante = nameof(Estudiante);
    public const string PersonalBiblioteca = nameof(PersonalBiblioteca);
    public const string Administrador = nameof(Administrador);
    public const string Equipo = PersonalBiblioteca + "," + Administrador;

    public static string Nombre(string rol) => rol == PersonalBiblioteca ? "Personal de biblioteca" : rol;
}

public enum EstadoLocker
{
    Disponible = 0,
    Ocupado = 1,
    FueraDeServicio = 2
}

public sealed class Estudiante
{
    public int Id { get; set; }
    public required string Matricula { get; set; }
    public required string Nombre { get; set; }
    public required string ApplicationUserId { get; set; }
}

public sealed class Locker
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public EstadoLocker Estado { get; set; }
    public byte[] Version { get; set; } = [];
}

public sealed class Asignacion
{
    public int Id { get; set; }
    public int EstudianteId { get; set; }
    public Estudiante Estudiante { get; set; } = null!;
    public int LockerId { get; set; }
    public Locker Locker { get; set; } = null!;
    public DateTimeOffset FechaHoraInicio { get; set; }
    public DateTimeOffset? FechaHoraFin { get; set; }
}
