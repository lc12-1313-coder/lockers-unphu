using LockersUnphu.Data;
using LockersUnphu.Domain;
using LockersUnphu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LockersUnphu.Controllers;

[Route("lockers")]
public sealed class InventarioController(LockersDb db, UserManager<ApplicationUser> usuarios) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var esAdmin = User.IsInRole(Roles.Administrador);
        var filas = await db.Lockers.AsNoTracking().OrderBy(l => l.Numero)
            .Select(l => new LockerFila(l.Id, l.Numero, l.Estado, esAdmin ? Convert.ToBase64String(l.Version) : null))
            .ToListAsync(ct);
        PrestamoFila? mio = null;
        if (User.IsInRole(Roles.Estudiante))
        {
            var usuario = usuarios.GetUserId(User);
            mio = await db.Asignaciones.AsNoTracking()
                .Where(a => a.FechaHoraFin == null && a.Estudiante.ApplicationUserId == usuario)
                .Select(a => new PrestamoFila(a.Id, a.Locker.Numero, "", "", a.FechaHoraInicio, a.FechaHoraFin))
                .SingleOrDefaultAsync(ct);
        }

        return View(new InventarioPantalla(filas, mio));
    }

    [Authorize(Roles = Roles.Administrador), HttpPost("nuevo")]
    public async Task<IActionResult> Nuevo(int numero, CancellationToken ct)
    {
        if (numero is < 1 or > 999)
            return Aviso("Indica un número de locker del 1 al 999.");

        db.Lockers.Add(new Locker { Numero = numero, Estado = EstadoLocker.Disponible });
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (SqlErrors.IsUniqueViolation(exception))
        {
            return Aviso("Ese número de locker ya existe.");
        }

        TempData["Confirmacion"] = $"Locker {numero:00} agregado.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrador), HttpPost("{id:int}/servicio")]
    public async Task<IActionResult> Servicio(int id, bool? habilitar, string? version, CancellationToken ct)
    {
        var locker = await db.Lockers.FindAsync([id], ct);
        if (locker is null)
            return Aviso("No se encontró el locker.");

        if (habilitar is null || version != Convert.ToBase64String(locker.Version))
            return Aviso("El estado cambió. Revisa el inventario y vuelve a intentarlo.");

        if (locker.Estado == EstadoLocker.Ocupado)
            return Aviso("Registra la devolución antes de cambiar el estado del locker.");

        locker.Estado = habilitar.Value ? EstadoLocker.Disponible : EstadoLocker.FueraDeServicio;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Aviso("El estado cambió. Revisa el inventario y vuelve a intentarlo.");
        }

        TempData["Confirmacion"] = $"Locker {locker.Numero:00} actualizado.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult Aviso(string mensaje)
    {
        TempData["Error"] = mensaje;
        return RedirectToAction(nameof(Index));
    }
}
