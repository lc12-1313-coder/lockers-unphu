using LockersUnphu.Domain;
using LockersUnphu.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LockersUnphu.Data;

public sealed class DatosIniciales(
    LockersDb db,
    RoleManager<IdentityRole> roles,
    UserManager<ApplicationUser> usuarios,
    Cuentas cuentas,
    IConfiguration config,
    IWebHostEnvironment entorno,
    TimeProvider reloj)
{
    public async Task CargarAsync(CancellationToken ct = default)
    {
        foreach (var rol in new[] { Roles.Administrador, Roles.PersonalBiblioteca, Roles.Estudiante })
        {
            if (!await roles.RoleExistsAsync(rol))
            {
                var resultado = await roles.CreateAsync(new IdentityRole(rol));
                if (!resultado.Succeeded)
                    throw new InvalidOperationException(Cuentas.Mensaje(resultado));
            }
        }

        if ((await usuarios.GetUsersInRoleAsync(Roles.Administrador)).Count == 0)
        {
            var usuario = Requerir("Inicializacion:Administrador");
            var clave = Requerir("Inicializacion:ClaveAdministrador");
            await CrearAsync(usuario, clave, Roles.Administrador, null, ct);
        }

        if (!entorno.IsDevelopment() || !config.GetValue<bool>("Inicializacion:Demo"))
            return;

        var claveDemo = Requerir("Inicializacion:ClaveDemo");
        await CrearDemoAsync("biblioteca.demo", claveDemo, Roles.PersonalBiblioteca, null, ct);

        var estudiantes = new[] { ("DEMO-1001", "Estudiante Demo Uno"), ("DEMO-1002", "Estudiante Demo Dos"), ("DEMO-1003", "Estudiante Demo Tres") };
        foreach (var (matricula, nombre) in estudiantes)
            await CrearDemoAsync(matricula, claveDemo, Roles.Estudiante, nombre, ct);

        var numerosExistentes = await db.Lockers.Select(l => l.Numero).ToListAsync(ct);
        for (var numero = 1; numero <= 12; numero++)
        {
            if (!numerosExistentes.Contains(numero))
                db.Lockers.Add(new Locker { Numero = numero, Estado = numero == 8 ? EstadoLocker.FueraDeServicio : EstadoLocker.Disponible });
        }
        await db.SaveChangesAsync(ct);

        if (!await db.Asignaciones.AnyAsync(ct))
        {
            var fecha = reloj.GetUtcNow().AddHours(-2);
            db.Asignaciones.Add(new Asignacion
            {
                EstudianteId = await db.Estudiantes.Where(e => e.Matricula == "DEMO-1001").Select(e => e.Id).SingleAsync(ct),
                LockerId = await db.Lockers.Where(l => l.Numero == 1).Select(l => l.Id).SingleAsync(ct),
                FechaHoraInicio = fecha,
                FechaHoraFin = fecha.AddMinutes(45)
            });
            await db.SaveChangesAsync(ct);
        }
    }

    private async Task CrearDemoAsync(string usuario, string clave, string rol, string? nombre, CancellationToken ct)
    {
        if (await usuarios.FindByNameAsync(usuario) is null)
            await CrearAsync(usuario, clave, rol, nombre, ct);
    }

    private async Task CrearAsync(string usuario, string clave, string rol, string? nombre, CancellationToken ct)
    {
        var error = await cuentas.CrearAsync(usuario, clave, rol, nombre, ct);
        if (error is not null)
            throw new InvalidOperationException(error);
    }

    private string Requerir(string clave) => config[clave] is { Length: > 0 } valor
        ? valor
        : throw new InvalidOperationException($"Configura {clave} para inicializar la base.");
}
