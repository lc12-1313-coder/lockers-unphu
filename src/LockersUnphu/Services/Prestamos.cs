using LockersUnphu.Data;
using LockersUnphu.Domain;
using Microsoft.EntityFrameworkCore;

namespace LockersUnphu.Services;

public sealed class Prestamos(LockersDb db, TimeProvider reloj)
{
    public const string EstudianteOcupado = "El estudiante ya posee una asignación activa.";
    public const string LockerOcupado = "El locker seleccionado no está disponible.";
    public const string SinAsignacion = "No existe una asignación activa para esta devolución.";

    public async Task<string?> EntregarAsync(string matricula, int lockerId, CancellationToken ct)
    {
        var estudiante = await db.Estudiantes.SingleOrDefaultAsync(e => e.Matricula == matricula, ct);
        if (estudiante is null)
            return "No se encontró un estudiante con esa matrícula.";

        if (await db.Asignaciones.AnyAsync(a => a.EstudianteId == estudiante.Id && a.FechaHoraFin == null, ct))
            return EstudianteOcupado;

        var locker = await db.Lockers.FindAsync([lockerId], ct);
        if (locker is null || locker.Estado != EstadoLocker.Disponible)
            return LockerOcupado;

        locker.Estado = EstadoLocker.Ocupado;
        db.Asignaciones.Add(new Asignacion
        {
            EstudianteId = estudiante.Id,
            LockerId = locker.Id,
            FechaHoraInicio = reloj.GetUtcNow()
        });

        try
        {
            await db.SaveChangesAsync(ct);
            return null;
        }
        catch (DbUpdateConcurrencyException)
        {
            return LockerOcupado;
        }
        catch (DbUpdateException exception) when (SqlErrors.IsUniqueViolation(exception))
        {
            return await db.Asignaciones.AsNoTracking()
                .AnyAsync(a => a.EstudianteId == estudiante.Id && a.FechaHoraFin == null, ct)
                ? EstudianteOcupado
                : LockerOcupado;
        }
    }

    public async Task<string?> DevolverAsync(int id, CancellationToken ct)
    {
        var asignacion = await db.Asignaciones.Include(a => a.Locker)
            .SingleOrDefaultAsync(a => a.Id == id && a.FechaHoraFin == null, ct);
        if (asignacion is null)
            return SinAsignacion;

        var ahora = reloj.GetUtcNow();
        asignacion.FechaHoraFin = ahora < asignacion.FechaHoraInicio ? asignacion.FechaHoraInicio : ahora;
        asignacion.Locker.Estado = EstadoLocker.Disponible;

        try
        {
            await db.SaveChangesAsync(ct);
            return null;
        }
        catch (DbUpdateConcurrencyException)
        {
            return SinAsignacion;
        }
    }
}
