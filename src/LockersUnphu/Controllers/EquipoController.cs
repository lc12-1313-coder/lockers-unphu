using LockersUnphu.Data;
using LockersUnphu.Domain;
using LockersUnphu.Models;
using LockersUnphu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LockersUnphu.Controllers;

[Authorize(Roles = Roles.Administrador)]
[Route("equipo")]
public sealed class EquipoController(LockersDb db, UserManager<ApplicationUser> usuarios, Cuentas cuentas) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var equipo = await (from cuenta in db.Users
                            join permiso in db.UserRoles on cuenta.Id equals permiso.UserId
                            join rol in db.Roles on permiso.RoleId equals rol.Id
                            where rol.Name == Roles.Administrador || rol.Name == Roles.PersonalBiblioteca
                            orderby cuenta.UserName
                            select new CuentaFila(cuenta.Id, cuenta.UserName!, rol.Name!)).ToListAsync(ct);

        ViewData["CuentaActual"] = usuarios.GetUserId(User);
        return View(equipo);
    }

    [HttpGet("nuevo")]
    public IActionResult Nuevo() => View(new EquipoForm { Rol = Roles.PersonalBiblioteca });

    [HttpPost("nuevo")]
    public async Task<IActionResult> Nuevo(EquipoForm form, CancellationToken ct)
    {
        if (!EsRolDeEquipo(form.Rol))
            ModelState.AddModelError(nameof(form.Rol), "Selecciona un rol válido.");

        if (!ModelState.IsValid)
            return View(form);

        var error = await cuentas.CrearAsync(form.Usuario, form.Clave, form.Rol, null, ct);
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View(form);
        }

        TempData["Confirmacion"] = "Cuenta del equipo creada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/rol")]
    public async Task<IActionResult> Rol(string id, string rol, CancellationToken ct)
    {
        var error = EsRolDeEquipo(rol)
            ? await cuentas.CambiarRolAsync(id, usuarios.GetUserId(User)!, rol, ct)
            : "Selecciona un rol válido.";

        TempData[error is null ? "Confirmacion" : "Error"] = error ?? "Permisos actualizados.";
        return RedirectToAction(nameof(Index));
    }

    private static bool EsRolDeEquipo(string? rol) => rol is Roles.Administrador or Roles.PersonalBiblioteca;
}
