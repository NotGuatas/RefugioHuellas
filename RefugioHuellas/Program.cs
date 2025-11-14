using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Services;

var builder = WebApplication.CreateBuilder(args);

// ===========================================
//  Servicios
// ===========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=refugio.db";

// Base de datos SQLite
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
//  Migraciones iniciales y seeders
// ===========================================
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<ApplicationDbContext>();

    // Aplicar migraciones
    db.Database.Migrate();

    // Seed de preguntas de compatibilidad
    await DataSeeder.SeedCompatibilityAsync(db);

    // Seed de roles + usuario admin (admin@huellas.com / Admin123$)
    await DataSeeder.SeedRolesAndAdminAsync(sp);

    // Si luego creas un método para sembrar perros iniciales,
    // lo llamarías aquí, por ejemplo:
    // await DataSeeder.SeedDogsAsync(db);
}

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

// Static files con el sistema nuevo de .NET 9
app.MapStaticAssets();

// Ruta raíz: si hay sesión => /Dogs, si no => /Login
app.MapGet("/", (HttpContext ctx) =>
{
    var isAuth = ctx.User?.Identity?.IsAuthenticated == true;
    var target = isAuth ? "/Dogs" : "/Identity/Account/Login";
    return Results.Redirect(target);
});

// Ruta por defecto MVC
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dogs}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
