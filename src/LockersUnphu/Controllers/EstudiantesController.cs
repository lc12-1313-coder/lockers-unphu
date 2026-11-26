using LockersUnphu.Data;
using LockersUnphu.Domain;
using LockersUnphu.Models;
using LockersUnphu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LockersUnphu.Controllers;

[Authorize(Roles = Roles.Equipo)]
[Route("estudiantes")]
public sealed class EstudiantesController(Cuentas cuentas, LockersDb db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? matricula, CancellationToken ct)
    {
        matricula = matricula?.Trim().ToUpperInvariant();
        ViewData["Matricula"] = matricula;
        var estudiante = string.IsNullOrEmpty(matricula) ? null
            : await db.Estudiantes.AsNoTracking().SingleOrDefaultAsync(e => e.Matricula == matricula, ct);
        return View(estudiante);
    }

    [HttpGet("nuevo")]
    public IActionResult Nuevo(string? matricula) => View(new EstudianteForm { Matricula = matricula ?? "" });

    [HttpPost("nuevo")]
    public async Task<IActionResult> Nuevo(EstudianteForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(form);

        var matricula = form.Matricula.ToUpperInvariant();
        var error = await cuentas.CrearAsync(matricula, form.Clave, Roles.Estudiante, form.Nombre.Trim(), ct);
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View(form);
        }

        TempData["Confirmacion"] = $"Se creó la cuenta {matricula}. El estudiante ya puede ingresar.";
        return RedirectToAction(nameof(Index), new { matricula });
    }
}
