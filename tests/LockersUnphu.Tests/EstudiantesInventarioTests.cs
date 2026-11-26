using System.Net;
using LockersUnphu.Data;
using LockersUnphu.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LockersUnphu.Tests;

public sealed class EstudiantesInventarioTests
{
    [SqlServerFact]
    [Trait("Categoria", "SqlServer")]
    public async Task Registro_inventario_y_mantenimiento_respetan_permisos_y_privacidad()
    {
        var conexion = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("LOCKERS_TEST_SQL"))
        {
            InitialCatalog = $"LockersInterfazPruebas_{Guid.NewGuid():N}"
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
            await scope.ServiceProvider.GetRequiredService<DatosIniciales>().CargarAsync();

            using var personal = app.CrearCliente();
            using var admin = app.CrearCliente();
            using var estudiante = app.CrearCliente();
            await IngresarAsync(personal, "biblioteca.demo", clave);
            await IngresarAsync(admin, "admin.demo", clave);
            await IngresarAsync(estudiante, "DEMO-1002", clave);

            await RegistrarAsync(personal, "prueba-1", "Estudiante de Prueba", clave);
            var busqueda = await personal.GetStringAsync("/estudiantes?matricula=prueba-1");
            Assert.Contains("Estudiante de Prueba", busqueda);
            Assert.Contains("PRUEBA-1", busqueda);
            await RegistrarAsync(admin, "PRUEBA-2", "Otra Persona", clave);

            var tokenAlta = await AplicacionPruebas.TokenAsync(personal, "/estudiantes/nuevo");
            using var duplicado = await AplicacionPruebas.EnviarAsync(personal, "/estudiantes/nuevo", tokenAlta,
                ("Matricula", "PRUEBA-1"), ("Nombre", "Duplicado"), ("Clave", clave));
            Assert.Equal(HttpStatusCode.OK, duplicado.StatusCode);
            Assert.Contains("ya está registrada", WebUtility.HtmlDecode(await duplicado.Content.ReadAsStringAsync()));

            foreach (var ruta in new[] { "/estudiantes", "/estudiantes/nuevo" })
            {
                using var denegado = await estudiante.GetAsync(ruta);
                Assert.StartsWith("/cuenta/sin-acceso", denegado.Headers.Location?.PathAndQuery);
            }
            await RechazarPostAsync(estudiante, "/estudiantes/nuevo");
            await RechazarPostAsync(estudiante, "/lockers/nuevo");
            await RechazarPostAsync(personal, "/lockers/nuevo");

            var averiado = await db.Lockers.AsNoTracking().SingleAsync(l => l.Numero == 8);
            await RechazarPostAsync(estudiante, $"/lockers/{averiado.Id}/servicio");
            await RechazarPostAsync(personal, $"/lockers/{averiado.Id}/servicio");

            var tokenAdmin = await AplicacionPruebas.TokenAsync(admin, "/lockers");
            var invalido = await PostYLeerAsync(admin, "/lockers/nuevo", tokenAdmin, ("numero", "0"));
            Assert.Contains("del 1 al 999", invalido);
            await PostYLeerAsync(admin, "/lockers/nuevo", tokenAdmin, ("numero", "13"));
            var repetido = await PostYLeerAsync(admin, "/lockers/nuevo", tokenAdmin, ("numero", "13"));
            Assert.Contains("Ese número de locker ya existe", repetido);
            Assert.Equal(13, await db.Lockers.CountAsync());

            var versionAnterior = Convert.ToBase64String(averiado.Version);
            await PostYLeerAsync(admin, $"/lockers/{averiado.Id}/servicio", tokenAdmin,
                ("habilitar", "true"), ("version", versionAnterior));
            var habilitado = await db.Lockers.AsNoTracking().SingleAsync(l => l.Id == averiado.Id);
            Assert.Equal(EstadoLocker.Disponible, habilitado.Estado);
            var obsoleto = await PostYLeerAsync(admin, $"/lockers/{averiado.Id}/servicio", tokenAdmin,
                ("habilitar", "false"), ("version", versionAnterior));
            Assert.Contains("El estado cambió", obsoleto);
            Assert.Equal(EstadoLocker.Disponible, (await db.Lockers.AsNoTracking().SingleAsync(l => l.Id == averiado.Id)).Estado);
            await PostYLeerAsync(admin, $"/lockers/{averiado.Id}/servicio", tokenAdmin,
                ("habilitar", "false"), ("version", Convert.ToBase64String(habilitado.Version)));
            Assert.Equal(EstadoLocker.FueraDeServicio, (await db.Lockers.AsNoTracking().SingleAsync(l => l.Id == averiado.Id)).Estado);

            // Preparar préstamos activos permite comprobar la privacidad antes de incorporar el mostrador.
            foreach (var numero in new[] { 1, 2 })
            {
                var locker = await db.Lockers.SingleAsync(l => l.Numero == numero);
                locker.Estado = EstadoLocker.Ocupado;
                db.Asignaciones.Add(new Asignacion
                {
                    LockerId = locker.Id,
                    EstudianteId = await db.Estudiantes.Where(e => e.Matricula == $"DEMO-100{numero}").Select(e => e.Id).SingleAsync(),
                    FechaHoraInicio = DateTimeOffset.UtcNow
                });
            }
            await db.SaveChangesAsync();
            var ocupado = await db.Lockers.AsNoTracking().SingleAsync(l => l.Numero == 1);
            var rechazo = await PostYLeerAsync(admin, $"/lockers/{ocupado.Id}/servicio", tokenAdmin,
                ("habilitar", "false"), ("version", Convert.ToBase64String(ocupado.Version)));
            Assert.Contains("Registra la devolución", rechazo);
            Assert.Equal(EstadoLocker.Ocupado, (await db.Lockers.AsNoTracking().SingleAsync(l => l.Id == ocupado.Id)).Estado);

            var vistaEstudiante = WebUtility.HtmlDecode(await estudiante.GetStringAsync("/lockers"));
            Assert.Contains("Mi préstamo · Locker 02", vistaEstudiante);
            Assert.DoesNotContain("Estudiante Demo Uno", vistaEstudiante);
            Assert.DoesNotContain("DEMO-1001", vistaEstudiante);
            Assert.DoesNotContain("Agregar locker", vistaEstudiante);
            Assert.DoesNotContain("Estudiantes</a>", vistaEstudiante);
            var vistaAdmin = await admin.GetStringAsync("/lockers");
            Assert.Contains("Agregar locker", vistaAdmin);
            Assert.DoesNotContain("Mi préstamo", vistaAdmin);
        }
        finally
        {
            await using var limpieza = new LockersDb(new DbContextOptionsBuilder<LockersDb>().UseSqlServer(conexion).Options);
            await limpieza.Database.EnsureDeletedAsync();
        }
    }

    private static async Task IngresarAsync(HttpClient cliente, string usuario, string clave)
    {
        var token = await AplicacionPruebas.TokenAsync(cliente, "/cuenta/ingresar");
        using var respuesta = await AplicacionPruebas.EnviarAsync(cliente, "/cuenta/ingresar", token,
            ("Usuario", usuario), ("Clave", clave));
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
    }

    private static async Task RegistrarAsync(HttpClient cliente, string matricula, string nombre, string clave)
    {
        var token = await AplicacionPruebas.TokenAsync(cliente, "/estudiantes/nuevo");
        using var respuesta = await AplicacionPruebas.EnviarAsync(cliente, "/estudiantes/nuevo", token,
            ("Matricula", matricula), ("Nombre", nombre), ("Clave", clave));
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.StartsWith("/estudiantes?", respuesta.Headers.Location?.OriginalString);
    }

    private static async Task RechazarPostAsync(HttpClient cliente, string ruta)
    {
        var token = await AplicacionPruebas.TokenAsync(cliente, "/cuenta/clave");
        using var respuesta = await AplicacionPruebas.EnviarAsync(cliente, ruta, token);
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.StartsWith("/cuenta/sin-acceso", respuesta.Headers.Location?.PathAndQuery);
    }

    private static async Task<string> PostYLeerAsync(HttpClient cliente, string ruta, string token,
        params (string Nombre, string Valor)[] campos)
    {
        using var respuesta = await AplicacionPruebas.EnviarAsync(cliente, ruta, token, campos);
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        return WebUtility.HtmlDecode(await cliente.GetStringAsync(respuesta.Headers.Location));
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
