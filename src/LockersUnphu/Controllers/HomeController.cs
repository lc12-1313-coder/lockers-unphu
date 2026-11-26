using System.Diagnostics;
using LockersUnphu.Models;
using Microsoft.AspNetCore.Mvc;

namespace LockersUnphu.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();

    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

