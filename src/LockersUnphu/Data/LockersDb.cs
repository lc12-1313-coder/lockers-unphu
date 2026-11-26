using LockersUnphu.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LockersUnphu.Data;

public sealed class LockersDb(DbContextOptions<LockersDb> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
    public DbSet<Locker> Lockers => Set<Locker>();
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var estudiantes = builder.Entity<Estudiante>();
        estudiantes.ToTable("Estudiantes");
        estudiantes.Property(e => e.Matricula).HasMaxLength(24);
        estudiantes.Property(e => e.Nombre).HasMaxLength(120);
        estudiantes.HasIndex(e => e.Matricula).IsUnique();
        estudiantes.HasIndex(e => e.ApplicationUserId).IsUnique();
        estudiantes.HasOne<ApplicationUser>().WithOne().HasForeignKey<Estudiante>(e => e.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        var lockers = builder.Entity<Locker>();
        lockers.ToTable("Lockers", table =>
        {
            table.HasCheckConstraint("CK_Lockers_Numero", "[Numero] BETWEEN 1 AND 999");
            table.HasCheckConstraint("CK_Lockers_Estado", "[Estado] IN (0, 1, 2)");
        });
        lockers.HasIndex(l => l.Numero).IsUnique();
        lockers.Property(l => l.Version).IsRowVersion();

        var asignaciones = builder.Entity<Asignacion>();
        asignaciones.ToTable("Asignaciones", table =>
            table.HasCheckConstraint("CK_Asignaciones_Fechas", "[FechaHoraFin] >= [FechaHoraInicio]"));
        asignaciones.HasOne(a => a.Estudiante).WithMany().OnDelete(DeleteBehavior.Restrict);
        asignaciones.HasOne(a => a.Locker).WithMany().OnDelete(DeleteBehavior.Restrict);
        asignaciones.HasIndex(a => a.EstudianteId).IsUnique().HasFilter("[FechaHoraFin] IS NULL");
        asignaciones.HasIndex(a => a.LockerId).IsUnique().HasFilter("[FechaHoraFin] IS NULL");
    }
}
