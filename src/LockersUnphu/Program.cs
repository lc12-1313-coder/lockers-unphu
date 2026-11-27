using LockersUnphu.Data;
using LockersUnphu.Domain;
using LockersUnphu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var inicializar = args.Contains("--inicializar", StringComparer.Ordinal);
var builder = WebApplication.CreateBuilder(args.Where(arg => arg != "--inicializar").ToArray());
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(
    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddDbContext<LockersDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Lockers")
        ?? throw new InvalidOperationException("Configura ConnectionStrings:Lockers para usar SQL Server.")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 12;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddErrorDescriber<ErroresIdentity>()
    .AddEntityFrameworkStores<LockersDb>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/cuenta/ingresar";
    options.AccessDeniedPath = "/cuenta/sin-acceso";
    options.Cookie.Name = "LockersUnphu.Sesion";
});
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
var proteccion = builder.Services.AddDataProtection().SetApplicationName("LockersUnphu");
if (builder.Configuration["DataProtection:KeysDirectory"] is { Length: > 0 } ruta)
    proteccion.PersistKeysToFileSystem(Directory.CreateDirectory(ruta));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<Cuentas>();
builder.Services.AddScoped<Prestamos>();
builder.Services.AddScoped<DatosIniciales>();

var app = builder.Build();
if (inicializar)
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<DatosIniciales>().CargarAsync();
    Console.WriteLine("Datos iniciales preparados.");
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets().AllowAnonymous();
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();

public partial class Program;
