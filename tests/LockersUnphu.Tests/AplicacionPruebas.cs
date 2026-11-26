using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LockersUnphu.Tests;

public sealed class AplicacionPruebas : WebApplicationFactory<Program>
{
    public Dictionary<string, string?> Configuracion { get; } = [];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(Configuracion));
        builder.ConfigureServices(services => services.AddDataProtection().UseEphemeralDataProtectionProvider());
    }

    public HttpClient CrearCliente() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    public static async Task<string> TokenAsync(HttpClient cliente, string ruta)
    {
        var pagina = await cliente.GetStringAsync(ruta);
        var token = Regex.Match(pagina, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(token.Success, $"Falta el token de formulario en {ruta}.");
        return WebUtility.HtmlDecode(token.Groups[1].Value);
    }

    public static Task<HttpResponseMessage> EnviarAsync(HttpClient cliente, string ruta, string token,
        params (string Nombre, string Valor)[] campos)
    {
        var valores = campos.ToDictionary(campo => campo.Nombre, campo => campo.Valor);
        valores.Add("__RequestVerificationToken", token);
        return cliente.PostAsync(ruta, new FormUrlEncodedContent(valores));
    }
}
