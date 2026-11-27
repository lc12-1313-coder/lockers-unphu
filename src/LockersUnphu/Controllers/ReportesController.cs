using LockersUnphu.Data;
using LockersUnphu.Domain;
using LockersUnphu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LockersUnphu.Controllers;

[Authorize(Roles = Roles.Equipo)]
[Route("reportes")]
public sealed class ReportesController(LockersDb db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var total = await db.Asignaciones.CountAsync(ct);
        var promedio = await db.Asignaciones.Where(a => a.FechaHoraFin != null)
            .AverageAsync(a => (double?)EF.Functions.DateDiffSecond(a.FechaHoraInicio, a.FechaHoraFin), ct);
        var recientes = await db.Asignaciones.AsNoTracking().OrderByDescending(a => a.FechaHoraInicio)
            .ThenByDescending(a => a.Id).Take(50)
            .Select(a => new PrestamoFila(a.Id, a.Locker.Numero, a.Estudiante.Matricula, a.Estudiante.Nombre,
                a.FechaHoraInicio, a.FechaHoraFin))
            .ToListAsync(ct);

        return View(new ReportePantalla(total, promedio, recientes));
    }
}
