using System.Data;
using LockersUnphu.Data;
using LockersUnphu.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LockersUnphu.Services;

public sealed class Cuentas(LockersDb db, UserManager<ApplicationUser> usuarios)
{
    public async Task<string?> CrearAsync(string usuario, string clave, string rol, string? nombre, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        if (rol == Roles.Estudiante && await db.Estudiantes.AnyAsync(e => e.Matricula == usuario, ct))
            return "Esa matrícula ya está registrada.";

        var cuenta = new ApplicationUser { UserName = usuario };
        try
        {
            var resultado = await usuarios.CreateAsync(cuenta, clave);
            if (!resultado.Succeeded)
                return Mensaje(resultado);

            resultado = await usuarios.AddToRoleAsync(cuenta, rol);
            if (!resultado.Succeeded)
                return Mensaje(resultado);

            if (rol == Roles.Estudiante)
            {
                db.Estudiantes.Add(new Estudiante { Matricula = usuario, Nombre = nombre!, ApplicationUserId = cuenta.Id });
                await db.SaveChangesAsync(ct);
            }

            await tx.CommitAsync(ct);
            return null;
        }
        catch (DbUpdateException exception) when (SqlErrors.IsUniqueViolation(exception))
        {
            return rol == Roles.Estudiante ? "Esa matrícula o cuenta ya está registrada." : "Ese usuario ya existe.";
        }
    }

    public async Task<string?> CambiarRolAsync(string id, string autor, string rol, CancellationToken ct)
    {
        if (id == autor)
            return "Tu propio rol debe cambiarlo otro administrador.";

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var administrador = await usuarios.FindByIdAsync(autor);
            if (administrador is null || !await usuarios.IsInRoleAsync(administrador, Roles.Administrador))
                return "Tu cuenta ya no tiene permiso para cambiar roles.";

            var cuenta = await usuarios.FindByIdAsync(id);
            if (cuenta is null)
                return "No se encontró esa cuenta.";

            var actuales = await usuarios.GetRolesAsync(cuenta);
            if (actuales.Count != 1 || actuales[0] is not (Roles.Administrador or Roles.PersonalBiblioteca))
                return "Solo se puede cambiar el rol de una cuenta del equipo.";

            if (actuales[0] == rol)
                return null;

            var resultado = await usuarios.RemoveFromRolesAsync(cuenta, actuales);
            if (!resultado.Succeeded)
                return Mensaje(resultado);

            resultado = await usuarios.AddToRoleAsync(cuenta, rol);
            if (!resultado.Succeeded)
                return Mensaje(resultado);

            resultado = await usuarios.UpdateSecurityStampAsync(cuenta);
            if (!resultado.Succeeded)
                return Mensaje(resultado);

            await tx.CommitAsync(ct);
            return null;
        }
        catch (SqlException exception) when (exception.Number == 1205)
        {
            return "Otro administrador modificó los permisos al mismo tiempo. Vuelve a intentarlo.";
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 1205 })
        {
            return "Otro administrador modificó los permisos al mismo tiempo. Vuelve a intentarlo.";
        }
    }

    internal static string Mensaje(IdentityResult resultado) =>
        string.Join(" ", resultado.Errors.Select(error => error.Description));
}
