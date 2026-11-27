using LockersUnphu.Data;
using LockersUnphu.Domain;
using LockersUnphu.Models;
using LockersUnphu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LockersUnphu.Controllers;

[Authorize(Roles = Roles.PersonalBiblioteca)]
[Route("mostrador")]
public sealed class MostradorController(LockersDb db, Prestamos prestamos) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? matricula, CancellationToken ct)
    {
        matricula = Normalizar(matricula);
        var estudiante = string.IsNullOrEmpty(matricula) ? null : await db.Estudiantes.AsNoTracking()
            .SingleOrDefaultAsync(e => e.Matricula == matricula, ct);
        var activos = await db.Asignaciones.AsNoTracking().Where(a => a.FechaHoraFin == null)
            .OrderBy(a => a.FechaHoraInicio)
            .Select(a => new PrestamoFila(a.Id, a.Locker.Numero, a.Estudiante.Matricula, a.Estudiante.Nombre,
                a.FechaHoraInicio, a.FechaHoraFin))
            .ToListAsync(ct);
        var disponibles = estudiante is null || activos.Any(a => a.Matricula == matricula)
            ? []
            : await db.Lockers.AsNoTracking().Where(l => l.Estado == EstadoLocker.Disponible).OrderBy(l => l.Numero)
                .Select(l => new LockerFila(l.Id, l.Numero, l.Estado, null)).ToListAsync(ct);

        return View(new MostradorPantalla(matricula, estudiante, disponibles, activos));
    }

    [HttpPost("entregar")]
    public async Task<IActionResult> Entregar(string? matricula, int lockerId, CancellationToken ct)
    {
        matricula = Normalizar(matricula);
        if (string.IsNullOrEmpty(matricula) || matricula.Length > 24)
        {
            TempData["Error"] = "Indica una matrícula válida.";
            return RedirectToAction(nameof(Index));
        }

        var error = await prestamos.EntregarAsync(matricula, lockerId, ct);
        TempData[error is null ? "Confirmacion" : "Error"] = error ?? "Entrega registrada. El locker está ocupado.";
        return RedirectToAction(nameof(Index), new { matricula });
    }

    [HttpPost("devolver")]
    public async Task<IActionResult> Devolver(int id, string? matricula, CancellationToken ct)
    {
        var error = await prestamos.DevolverAsync(id, ct);
        TempData[error is null ? "Confirmacion" : "Error"] = error ?? "Devolución registrada. El locker está disponible.";
        return RedirectToAction(nameof(Index), new { matricula = Normalizar(matricula) });
    }

    private static string? Normalizar(string? matricula) => matricula?.Trim().ToUpperInvariant();
}
