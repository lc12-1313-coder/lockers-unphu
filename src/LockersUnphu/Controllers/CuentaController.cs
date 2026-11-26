using LockersUnphu.Domain;
using LockersUnphu.Models;
using LockersUnphu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LockersUnphu.Controllers;

[Route("cuenta")]
public sealed class CuentaController(SignInManager<ApplicationUser> sesion, UserManager<ApplicationUser> usuarios) : Controller
{
    [AllowAnonymous, HttpGet("ingresar")]
    public IActionResult Ingresar(string? returnUrl) => User.Identity?.IsAuthenticated == true
        ? RedirectToAction("Index", "Home")
        : View(new IngresoForm { ReturnUrl = returnUrl });

    [AllowAnonymous, HttpPost("ingresar")]
    public async Task<IActionResult> Ingresar(IngresoForm form)
    {
        if (!ModelState.IsValid)
            return View(form);

        var resultado = await sesion.PasswordSignInAsync(form.Usuario.Trim(), form.Clave, false, lockoutOnFailure: true);
        if (resultado.Succeeded)
            return Url.IsLocalUrl(form.ReturnUrl) ? LocalRedirect(form.ReturnUrl!) : RedirectToAction("Index", "Home");

        ModelState.AddModelError("", "No se pudo ingresar. Revisa tus datos o intenta más tarde.");
        return View(form);
    }

    [HttpPost("salir")]
    public async Task<IActionResult> Salir()
    {
        await sesion.SignOutAsync();
        return RedirectToAction(nameof(Ingresar));
    }

    [HttpGet("sin-acceso")]
    public IActionResult SinAcceso()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    [HttpGet("clave")]
    public IActionResult Clave() => View(new ClaveForm());

    [HttpPost("clave")]
    public async Task<IActionResult> Clave(ClaveForm form)
    {
        if (!ModelState.IsValid)
            return View(form);

        var usuario = await usuarios.GetUserAsync(User);
        if (usuario is null)
            return Challenge();

        var resultado = await usuarios.ChangePasswordAsync(usuario, form.Actual, form.Nueva);
        if (!resultado.Succeeded)
        {
            ModelState.AddModelError("", resultado.Errors.Any(e => e.Code == "PasswordMismatch")
                ? "La contraseña actual es incorrecta."
                : Cuentas.Mensaje(resultado));
            return View(form);
        }

        await sesion.RefreshSignInAsync(usuario);
        TempData["Confirmacion"] = "Contraseña actualizada.";
        return RedirectToAction(nameof(Clave));
    }
}

