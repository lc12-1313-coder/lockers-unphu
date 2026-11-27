using System.Net;
using LockersUnphu.Data;
using LockersUnphu.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LockersUnphu.Tests;

public sealed class EquipoTests
{
    [SqlServerFact]
    [Trait("Categoria", "SqlServer")]
    public async Task Administracion_del_equipo_valida_roles_y_revoca_sesiones_anteriores()
    {
        var conexion = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("LOCKERS_TEST_SQL"))
        {
            InitialCatalog = $"LockersEquipoPruebas_{Guid.NewGuid():N}"
        }.ConnectionString;
        var clave = $"DemoA1{Guid.NewGuid():N}";
        await using var app = new AplicacionPruebas
        {
            Configuracion =
            {
                ["ConnectionStrings:Lockers"] = conexion,
                ["Inicializacion:Administrador"] = "admin.demo",
                ["Inicializacion:ClaveAdministrador"] = clave,
                ["Inicializacion:Demo"] = "true",
                ["Inicializacion:ClaveDemo"] = clave
            }
        };
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LockersDb>();
        try
        {
            await db.Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<DatosIniciales>().CargarAsync();
            var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var adminId = (await usuarios.FindByNameAsync("admin.demo"))!.Id;
            var estudianteId = (await usuarios.FindByNameAsync("DEMO-1002"))!.Id;

            using var admin = app.CrearCliente();
            using var personal = app.CrearCliente();
            using var estudiante = app.CrearCliente();
            await IngresarAsync(admin, "admin.demo", clave);
            await IngresarAsync(personal, "biblioteca.demo", clave);
            await IngresarAsync(estudiante, "DEMO-1002", clave);

            foreach (var cliente in new[] { personal, estudiante })
            {
                foreach (var ruta in new[] { "/equipo", "/equipo/nuevo" })
                {
                    using var denegado = await cliente.GetAsync(ruta);
                    Assert.StartsWith("/cuenta/sin-acceso", denegado.Headers.Location?.PathAndQuery);
                }
                var tokenAjeno = await AplicacionPruebas.TokenAsync(cliente, "/cuenta/clave");
                foreach (var ruta in new[] { "/equipo/nuevo", $"/equipo/{adminId}/rol" })
                {
                    using var denegado = await AplicacionPruebas.EnviarAsync(cliente, ruta, tokenAjeno,
                        ("Usuario", "intruso.demo"), ("Clave", clave), ("Rol", Roles.Administrador));
                    Assert.StartsWith("/cuenta/sin-acceso", denegado.Headers.Location?.PathAndQuery);
                }
            }

            var listado = await admin.GetStringAsync("/equipo");
            Assert.Contains("biblioteca.demo", listado);
            Assert.DoesNotContain("DEMO-1002", listado);
            Assert.DoesNotContain(clave, listado);
            using var sinToken = await admin.PostAsync("/equipo/nuevo", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.BadRequest, sinToken.StatusCode);

            var token = await AplicacionPruebas.TokenAsync(admin, "/equipo/nuevo");
            using var rolInvalido = await AplicacionPruebas.EnviarAsync(admin, "/equipo/nuevo", token,
                ("Usuario", "rol.invalido"), ("Clave", clave), ("Rol", Roles.Estudiante));
            Assert.Equal(HttpStatusCode.OK, rolInvalido.StatusCode);
            Assert.Contains("Selecciona un rol válido", WebUtility.HtmlDecode(await rolInvalido.Content.ReadAsStringAsync()));
            Assert.Null(await usuarios.FindByNameAsync("rol.invalido"));

            using var alta = await AplicacionPruebas.EnviarAsync(admin, "/equipo/nuevo", token,
                ("Usuario", "equipo.prueba"), ("Clave", clave), ("Rol", Roles.PersonalBiblioteca));
            Assert.Equal(HttpStatusCode.Redirect, alta.StatusCode);
            Assert.Equal("/equipo", alta.Headers.Location?.OriginalString);
            using var duplicado = await AplicacionPruebas.EnviarAsync(admin, "/equipo/nuevo", token,
                ("Usuario", "equipo.prueba"), ("Clave", clave), ("Rol", Roles.Administrador));
            Assert.Equal(HttpStatusCode.OK, duplicado.StatusCode);
            Assert.Equal(1, await db.Users.CountAsync(u => u.UserName == "equipo.prueba"));

            var cuenta = (await usuarios.FindByNameAsync("equipo.prueba"))!;
            Assert.True(await usuarios.IsInRoleAsync(cuenta, Roles.PersonalBiblioteca));
            using var cuentaCliente = app.CrearCliente();
            await IngresarAsync(cuentaCliente, "equipo.prueba", clave);

            await CambiarRolAsync(admin, cuenta.Id, Roles.Administrador, "Permisos actualizados.");
            using var sesionVieja = await cuentaCliente.GetAsync("/equipo");
            Assert.StartsWith("/cuenta/ingresar", sesionVieja.Headers.Location?.PathAndQuery);
            await IngresarAsync(cuentaCliente, "equipo.prueba", clave);
            Assert.Contains("Crear cuenta", await cuentaCliente.GetStringAsync("/equipo"));

            await CambiarRolAsync(admin, adminId, Roles.PersonalBiblioteca,
                "Tu propio rol debe cambiarlo otro administrador.");
            await CambiarRolAsync(admin, estudianteId, Roles.Administrador,
                "Solo se puede cambiar el rol de una cuenta del equipo.");
            await CambiarRolAsync(admin, cuenta.Id, Roles.Estudiante, "Selecciona un rol válido.");
            await CambiarRolAsync(admin, cuenta.Id, Roles.PersonalBiblioteca, "Permisos actualizados.");
            using var administradorRevocado = await cuentaCliente.GetAsync("/equipo");
            Assert.StartsWith("/cuenta/ingresar", administradorRevocado.Headers.Location?.PathAndQuery);
            await IngresarAsync(cuentaCliente, "equipo.prueba", clave);
            using var personalDenegado = await cuentaCliente.GetAsync("/equipo");
            Assert.StartsWith("/cuenta/sin-acceso", personalDenegado.Headers.Location?.PathAndQuery);

            await using var finalScope = app.Services.CreateAsyncScope();
            var usuariosFinales = finalScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True(await usuariosFinales.IsInRoleAsync((await usuariosFinales.FindByIdAsync(adminId))!, Roles.Administrador));
            Assert.True(await usuariosFinales.IsInRoleAsync((await usuariosFinales.FindByIdAsync(cuenta.Id))!, Roles.PersonalBiblioteca));
            Assert.True(await usuariosFinales.IsInRoleAsync((await usuariosFinales.FindByIdAsync(estudianteId))!, Roles.Estudiante));
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    private static async Task IngresarAsync(HttpClient cliente, string usuario, string clave)
    {
        var token = await AplicacionPruebas.TokenAsync(cliente, "/cuenta/ingresar");
        using var respuesta = await AplicacionPruebas.EnviarAsync(cliente, "/cuenta/ingresar", token,
            ("Usuario", usuario), ("Clave", clave));
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/", respuesta.Headers.Location?.OriginalString);
    }

    private static async Task CambiarRolAsync(HttpClient cliente, string id, string rol, string esperado)
    {
        var token = await AplicacionPruebas.TokenAsync(cliente, "/equipo");
        using var respuesta = await AplicacionPruebas.EnviarAsync(cliente, $"/equipo/{id}/rol", token, ("rol", rol));
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Contains(esperado, WebUtility.HtmlDecode(await cliente.GetStringAsync(respuesta.Headers.Location)));
    }

    private sealed class SqlServerFactAttribute : FactAttribute
    {
        public SqlServerFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LOCKERS_TEST_SQL")))
                Skip = "Configura LOCKERS_TEST_SQL para ejecutar la prueba contra SQL Server.";
        }
    }
}
