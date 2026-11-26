using System.Net;
using LockersUnphu.Data;
using LockersUnphu.Domain;
using LockersUnphu.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LockersUnphu.Tests;

public sealed class BaseDatosTests
{
    [Fact]
    public async Task Inicio_exige_sesion_y_formularios_exigen_token()
    {
        using var app = new AplicacionPruebas();
        using var cliente = app.CrearCliente();
        using var inicio = await cliente.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, inicio.StatusCode);
        Assert.StartsWith("/cuenta/ingresar", inicio.Headers.Location?.PathAndQuery);

        await AplicacionPruebas.TokenAsync(cliente, "/cuenta/ingresar");
        using var sinToken = await cliente.PostAsync("/cuenta/ingresar", new FormUrlEncodedContent([]));
        Assert.Equal(HttpStatusCode.BadRequest, sinToken.StatusCode);
    }

    [SqlServerFact]
    [Trait("Categoria", "SqlServer")]
    public async Task Migracion_seed_identidad_e_indices_preservan_las_reglas()
    {
        var conexion = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("LOCKERS_TEST_SQL"))
        {
            InitialCatalog = $"LockersBasePruebas_{Guid.NewGuid():N}"
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

        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<LockersDb>();
            await db.Database.MigrateAsync();
            var seed = scope.ServiceProvider.GetRequiredService<DatosIniciales>();
            await seed.CargarAsync();
            await seed.CargarAsync();
            Assert.Equal(3, await db.Roles.CountAsync());
            Assert.Equal(5, await db.Users.CountAsync());
            Assert.Equal(3, await db.Estudiantes.CountAsync());
            Assert.Equal(12, await db.Lockers.CountAsync());
            Assert.Equal(1, await db.Asignaciones.CountAsync());

            var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var (nombre, rol) in new[]
            {
                ("admin.demo", Roles.Administrador),
                ("biblioteca.demo", Roles.PersonalBiblioteca),
                ("DEMO-1002", Roles.Estudiante)
            })
            {
                var usuario = await usuarios.FindByNameAsync(nombre);
                Assert.NotNull(usuario);
                Assert.NotEqual(clave, usuario.PasswordHash);
                Assert.True(await usuarios.CheckPasswordAsync(usuario, clave));
                Assert.True(await usuarios.IsInRoleAsync(usuario, rol));

                using var cliente = app.CrearCliente();
                var token = await AplicacionPruebas.TokenAsync(cliente, "/cuenta/ingresar");
                using var respuesta = await AplicacionPruebas.EnviarAsync(cliente, "/cuenta/ingresar", token,
                    ("Usuario", nombre), ("Clave", clave));
                Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
                var inicio = WebUtility.HtmlDecode(await cliente.GetStringAsync("/"));
                Assert.Contains(nombre, inicio);
                Assert.Contains(Roles.Nombre(rol), inicio);

                var tokenClave = await AplicacionPruebas.TokenAsync(cliente, "/cuenta/clave");
                using var cambio = await AplicacionPruebas.EnviarAsync(cliente, "/cuenta/clave", tokenClave,
                    ("Actual", clave), ("Nueva", clave + "Nueva1"));
                Assert.Equal(HttpStatusCode.Redirect, cambio.StatusCode);

                var tokenSalida = await AplicacionPruebas.TokenAsync(cliente, "/cuenta/clave");
                using var salida = await AplicacionPruebas.EnviarAsync(cliente, "/cuenta/salir", tokenSalida);
                Assert.Equal(HttpStatusCode.Redirect, salida.StatusCode);
                using var cerrado = await cliente.GetAsync("/");
                Assert.Equal(HttpStatusCode.Redirect, cerrado.StatusCode);

                using var nuevaSesion = app.CrearCliente();
                var nuevoToken = await AplicacionPruebas.TokenAsync(nuevaSesion, "/cuenta/ingresar");
                using var reingreso = await AplicacionPruebas.EnviarAsync(nuevaSesion, "/cuenta/ingresar", nuevoToken,
                    ("Usuario", nombre), ("Clave", clave + "Nueva1"));
                Assert.Equal(HttpStatusCode.Redirect, reingreso.StatusCode);
            }

            var cuentas = scope.ServiceProvider.GetRequiredService<Cuentas>();
            var duplicado = await cuentas.CrearAsync("DEMO-1001", clave, Roles.Estudiante, "Duplicado", default);
            Assert.Equal("Esa matrícula ya está registrada.", duplicado);

            var estudiante1 = await db.Estudiantes.AsNoTracking().OrderBy(e => e.Id).FirstAsync();
            var estudiante2 = await db.Estudiantes.AsNoTracking().OrderBy(e => e.Id).Skip(1).FirstAsync();
            var locker1 = await db.Lockers.AsNoTracking().SingleAsync(l => l.Numero == 1);
            var locker2 = await db.Lockers.AsNoTracking().SingleAsync(l => l.Numero == 2);
            db.Asignaciones.Add(new Asignacion
            {
                EstudianteId = estudiante1.Id, LockerId = locker1.Id, FechaHoraInicio = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            await RechazarDuplicadoAsync(db, estudiante1.Id, locker2.Id);
            await RechazarDuplicadoAsync(db, estudiante2.Id, locker1.Id);
            Assert.Equal(1, await db.Asignaciones.CountAsync(a => a.FechaHoraFin == null));

            await using var otraConexion = new LockersDb(new DbContextOptionsBuilder<LockersDb>().UseSqlServer(conexion).Options);
            Assert.Equal(12, await otraConexion.Lockers.CountAsync());
            Assert.Equal(5, await otraConexion.Users.CountAsync());
        }
        finally
        {
            await using var limpieza = new LockersDb(new DbContextOptionsBuilder<LockersDb>().UseSqlServer(conexion).Options);
            await limpieza.Database.EnsureDeletedAsync();
        }
    }

    private static async Task RechazarDuplicadoAsync(LockersDb db, int estudianteId, int lockerId)
    {
        db.Asignaciones.Add(new Asignacion
        {
            EstudianteId = estudianteId, LockerId = lockerId, FechaHoraInicio = DateTimeOffset.UtcNow
        });
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.True(error.InnerException is SqlException { Number: 2601 or 2627 });
        db.ChangeTracker.Clear();
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
