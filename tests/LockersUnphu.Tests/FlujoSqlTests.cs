using System.Net;
using LockersUnphu.Controllers;
using LockersUnphu.Models;
using LockersUnphu.Data;
using LockersUnphu.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LockersUnphu.Tests;

public sealed class FlujoSqlTests
{
    [SqlServerFact]
    [Trait("Categoria", "SqlServer")]
    public async Task Registra_entrega_y_devolucion_y_las_conserva_al_reiniciar()
    {
        var conexion = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("LOCKERS_TEST_SQL"))
        {
            InitialCatalog = $"LockersPruebas_{Guid.NewGuid():N}"
        }.ConnectionString;
        var clave = $"DemoA1{Guid.NewGuid():N}";

        await using var app = CrearApp(conexion, clave);
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<LockersDb>();
            await db.Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<DatosIniciales>().CargarAsync();
            var lockers = await db.Lockers.AsNoTracking().ToDictionaryAsync(l => l.Numero, l => l.Id);

            var reporteInicial = Assert.IsType<ViewResult>(await new ReportesController(db).Index(default));
            var resumenInicial = Assert.IsType<ReportePantalla>(reporteInicial.Model);
            Assert.Equal(1, resumenInicial.Total);
            Assert.Equal(2700d, resumenInicial.PromedioSegundos);

            using var personal = app.CrearCliente();
            await IngresarAsync(personal, "biblioteca.demo", clave);
            var alta = await AplicacionPruebas.TokenAsync(personal, "/estudiantes/nuevo");
            using var registro = await AplicacionPruebas.EnviarAsync(personal, "/estudiantes/nuevo", alta,
                ("Matricula", "PRUEBA-1"), ("Nombre", "Persona Ficticia"), ("Clave", clave));
            Assert.Equal(HttpStatusCode.Redirect, registro.StatusCode);
            using var duplicado = await AplicacionPruebas.EnviarAsync(personal, "/estudiantes/nuevo", alta,
                ("Matricula", "PRUEBA-1"), ("Nombre", "Persona Duplicada"), ("Clave", clave));
            Assert.Equal(HttpStatusCode.OK, duplicado.StatusCode);
            Assert.Contains("ya está registrada", WebUtility.HtmlDecode(await duplicado.Content.ReadAsStringAsync()));

            var token = await AplicacionPruebas.TokenAsync(personal, "/mostrador?matricula=PRUEBA-1");
            await EntregarAsync(personal, token, "PRUEBA-1", lockers[2], "Entrega registrada");
            await EntregarAsync(personal, token, "PRUEBA-1", lockers[3], "ya posee una asignación activa");
            await EntregarAsync(personal, token, "DEMO-1002", lockers[2], "no está disponible");
            await EntregarAsync(personal, token, "DEMO-1002", lockers[8], "no está disponible");

            await EntregarAsync(personal, token, "NO-EXISTE", lockers[3], "No se encontró un estudiante");
            await EntregarAsync(personal, token, "DEMO-1003", 0, "no está disponible");

            var asignacion = await db.Asignaciones.AsNoTracking().Include(a => a.Locker)
                .SingleAsync(a => a.FechaHoraFin == null);
            Assert.Equal(EstadoLocker.Ocupado, asignacion.Locker.Estado);
            Assert.NotEqual(default, asignacion.FechaHoraInicio);
            using var estudiante = app.CrearCliente();
            await IngresarAsync(estudiante, "PRUEBA-1", clave);
            var inventario = WebUtility.HtmlDecode(await estudiante.GetStringAsync("/lockers"));
            Assert.Contains("Mi préstamo · Locker 02", inventario);
            Assert.DoesNotContain("Estudiante Demo Uno", inventario);

            foreach (var ruta in new[] { "/mostrador", "/reportes" })
            {
                using var consultaDenegada = await estudiante.GetAsync(ruta);
                Assert.Equal(HttpStatusCode.Redirect, consultaDenegada.StatusCode);
                Assert.StartsWith("/cuenta/sin-acceso", consultaDenegada.Headers.Location?.PathAndQuery);
            }
            var tokenEstudiante = await AplicacionPruebas.TokenAsync(estudiante, "/cuenta/clave");
            foreach (var ruta in new[] { "/mostrador/entregar", "/mostrador/devolver" })
            {
                using var accionDenegada = await AplicacionPruebas.EnviarAsync(estudiante, ruta, tokenEstudiante,
                    ("matricula", "PRUEBA-1"), ("lockerId", lockers[3].ToString()), ("id", asignacion.Id.ToString()));
                Assert.Equal(HttpStatusCode.Redirect, accionDenegada.StatusCode);
                Assert.StartsWith("/cuenta/sin-acceso", accionDenegada.Headers.Location?.PathAndQuery);
            }

            using var admin = app.CrearCliente();
            await IngresarAsync(admin, "admin.demo", clave);
            Assert.Contains("Agregar locker", await admin.GetStringAsync("/lockers"));
            Assert.Contains("Duración promedio", WebUtility.HtmlDecode(await admin.GetStringAsync("/reportes")));
            using var denegado = await admin.GetAsync("/mostrador");
            Assert.StartsWith("/cuenta/sin-acceso", denegado.Headers.Location?.PathAndQuery);

            var tokenAdmin = await AplicacionPruebas.TokenAsync(admin, "/cuenta/clave");
            foreach (var ruta in new[] { "/mostrador/entregar", "/mostrador/devolver" })
            {
                using var accionDenegada = await AplicacionPruebas.EnviarAsync(admin, ruta, tokenAdmin,
                    ("matricula", "PRUEBA-1"), ("lockerId", lockers[3].ToString()), ("id", asignacion.Id.ToString()));
                Assert.Equal(HttpStatusCode.Redirect, accionDenegada.StatusCode);
                Assert.StartsWith("/cuenta/sin-acceso", accionDenegada.Headers.Location?.PathAndQuery);
            }
            using var sinToken = await personal.PostAsync("/mostrador/entregar", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.BadRequest, sinToken.StatusCode);

            await DevolverAsync(personal, token, asignacion.Id, "Devolución registrada");
            await DevolverAsync(personal, token, asignacion.Id, "No existe una asignación activa");

            await app.DisposeAsync();
            await using var reinicio = CrearApp(conexion, clave);
            await using var nuevoScope = reinicio.Services.CreateAsyncScope();
            var nuevoDb = nuevoScope.ServiceProvider.GetRequiredService<LockersDb>();
            var guardada = await nuevoDb.Asignaciones.AsNoTracking().Include(a => a.Locker).SingleAsync(a => a.Id == asignacion.Id);
            Assert.NotNull(guardada.FechaHoraFin);
            Assert.True(guardada.FechaHoraFin >= guardada.FechaHoraInicio);
            Assert.Equal(EstadoLocker.Disponible, guardada.Locker.Estado);

            using var otroPersonal = reinicio.CrearCliente();
            await IngresarAsync(otroPersonal, "biblioteca.demo", clave);
            Assert.Contains("PRUEBA-1", await otroPersonal.GetStringAsync("/reportes"));

            using var segundaSesion = reinicio.CrearCliente();
            await IngresarAsync(segundaSesion, "biblioteca.demo", clave);
            var tokenUno = await AplicacionPruebas.TokenAsync(otroPersonal, "/mostrador?matricula=DEMO-1002");
            var tokenDos = await AplicacionPruebas.TokenAsync(segundaSesion, "/mostrador?matricula=DEMO-1002");
            var resultados = await Task.WhenAll(
                PublicarEntregaAsync(otroPersonal, tokenUno, "DEMO-1002", lockers[3]),
                PublicarEntregaAsync(segundaSesion, tokenDos, "DEMO-1002", lockers[4]));
            Assert.Single(resultados, pagina => pagina.Contains("Entrega registrada"));
            Assert.Single(resultados, pagina => pagina.Contains("ya posee una asignación activa"));
            Assert.Equal(1, await nuevoDb.Asignaciones.CountAsync(a => a.FechaHoraFin == null));
            Assert.Equal(1, await nuevoDb.Lockers.CountAsync(l => l.Estado == EstadoLocker.Ocupado));

            var mismoLocker = await Task.WhenAll(
                PublicarEntregaAsync(otroPersonal, tokenUno, "PRUEBA-1", lockers[5]),
                PublicarEntregaAsync(segundaSesion, tokenDos, "DEMO-1001", lockers[5]));
            Assert.Single(mismoLocker, pagina => pagina.Contains("Entrega registrada"));
            Assert.Single(mismoLocker, pagina => pagina.Contains("no está disponible"));
            Assert.Equal(1, await nuevoDb.Asignaciones.CountAsync(a => a.LockerId == lockers[5] && a.FechaHoraFin == null));
            Assert.Equal(2, await nuevoDb.Lockers.CountAsync(l => l.Estado == EstadoLocker.Ocupado));

        }
        finally
        {
            await using var limpieza = new LockersDb(new DbContextOptionsBuilder<LockersDb>().UseSqlServer(conexion).Options);
            if (await limpieza.Database.CanConnectAsync())
                await limpieza.Database.EnsureDeletedAsync();
        }
    }

    private static AplicacionPruebas CrearApp(string conexion, string clave) => new()
    {
        Configuracion =
        {
            ["ConnectionStrings:Lockers"] = conexion,
            ["Inicializacion:Administrador"] = "admin.demo",
            ["Inicializacion:ClaveAdministrador"] = clave,
            ["Inicializacion:ClaveDemo"] = clave,
            ["Inicializacion:Demo"] = "true"
        }
    };

    private static async Task IngresarAsync(HttpClient cliente, string usuario, string clave)
    {
        var token = await AplicacionPruebas.TokenAsync(cliente, "/cuenta/ingresar");
        using var respuesta = await AplicacionPruebas.EnviarAsync(cliente, "/cuenta/ingresar", token,
            ("Usuario", usuario), ("Clave", clave));
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/", respuesta.Headers.Location?.OriginalString);
    }

    private static async Task EntregarAsync(HttpClient cliente, string token, string matricula, int lockerId, string esperado) =>
        Assert.Contains(esperado, await PublicarEntregaAsync(cliente, token, matricula, lockerId));

    private static async Task<string> PublicarEntregaAsync(HttpClient cliente, string token, string matricula, int lockerId)
    {
        using var respuesta = await AplicacionPruebas.EnviarAsync(cliente, "/mostrador/entregar", token,
            ("matricula", matricula), ("lockerId", lockerId.ToString()));
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        return WebUtility.HtmlDecode(await cliente.GetStringAsync(respuesta.Headers.Location));
    }

    private static async Task DevolverAsync(HttpClient cliente, string token, int id, string esperado)
    {
        using var respuesta = await AplicacionPruebas.EnviarAsync(cliente, "/mostrador/devolver", token, ("id", id.ToString()));
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
