using LockersUnphu.Domain;

namespace LockersUnphu.Models;

public sealed record LockerFila(int Id, int Numero, EstadoLocker Estado, string? Version = null);

public sealed record PrestamoFila(int Id, int Locker, string Matricula, string Estudiante, DateTimeOffset Inicio, DateTimeOffset? Fin);

public sealed record InventarioPantalla(IReadOnlyList<LockerFila> Lockers, PrestamoFila? MiPrestamo);

public sealed record MostradorPantalla(
    string? Matricula,
    Estudiante? Estudiante,
    IReadOnlyList<LockerFila> Disponibles,
    IReadOnlyList<PrestamoFila> Activos);

public static class Formato
{
    private static readonly TimeZoneInfo SantoDomingo = TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");

    public static string Fecha(DateTimeOffset fecha) =>
        TimeZoneInfo.ConvertTime(fecha, SantoDomingo).ToString("dd/MM/yyyy HH:mm");

    public static string Estado(EstadoLocker estado) => estado switch
    {
        EstadoLocker.Disponible => "Disponible",
        EstadoLocker.Ocupado => "Ocupado",
        EstadoLocker.FueraDeServicio => "Fuera de servicio",
        _ => throw new ArgumentOutOfRangeException(nameof(estado))
    };
}
