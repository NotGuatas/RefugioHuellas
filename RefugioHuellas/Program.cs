using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------- Servicios -----------------

// Cadena de conexión (local y Render usan la misma clave "DefaultConnection")
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=refugio.db";

// DbContext (solo UNA vez)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// MVC
builder.Services.AddControllersWithViews();

// Identity + Roles
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configurar rutas de login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Servicio de compatibilidad
builder.Services.AddScoped<CompatibilityService>();

var app = builder.Build();

// ===========================================
//  Migraciones + SEED (roles, admin, traits, perros demo)
// ===========================================
await DbSeeder.SeedAsync(app.Services);

// ===========================================
//  Pipeline de ejecución
// ===========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Ruta raíz: si hay sesión => /Dogs, si no => login
app.MapGet("/", (HttpContext ctx) =>
{
    var isAuth = ctx.User?.Identity?.IsAuthenticated == true;
    var target = isAuth ? "/Dogs" : "/Identity/Account/Login";
    return Results.Redirect(target);
});

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dogs}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
