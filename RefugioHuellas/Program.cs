using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RefugioHuellas.Data;
using RefugioHuellas.Data.Repositories;
using RefugioHuellas.Services.Compatibility;
using RefugioHuellas.Services.Compatibility.Rules;
using RefugioHuellas.Services.Storage;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// ----------------- Servicios -----------------

// Cadena de conexión: debe existir "DefaultConnection" en appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext: con SQL 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        }));


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


// JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]!;
var jwtIssuer = jwtSection["Issuer"]!;
var jwtAudience = jwtSection["Audience"]!;

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });







// ----------------- Inyección de dependencias (SOLID + Patrones) -----------------

// Repository pattern (abstraer EF Core)
builder.Services.AddScoped<IDogRepository, EfDogRepository>();
builder.Services.AddScoped<ITraitRepository, EfTraitRepository>();
builder.Services.AddScoped<IUserTraitResponseRepository, EfUserTraitResponseRepository>();

// Strategy pattern: reglas por rasgo (se agregan sin tocar el servicio principal)
builder.Services.AddScoped<ITraitRule, HousingTypeRule>();
builder.Services.AddScoped<ITraitRule, SpaceRule>();
builder.Services.AddScoped<ITraitRule, TimeRule>();
builder.Services.AddScoped<ITraitRule, NoiseToleranceRule>();
builder.Services.AddScoped<ITraitRule, ActivityLevelRule>();

// Factory Method: resolver regla por clave
builder.Services.AddScoped<ITraitRuleFactory, TraitRuleFactory>();

// Servicio de compatibilidad (DIP: controladores dependen de ICompatibilityService)
builder.Services.AddScoped<ICompatibilityService, CompatibilityService>();

// SRP: almacenamiento de fotos aislado del controlador
builder.Services.AddScoped<IPhotoStorage, LocalPhotoStorage>();

//Activar Cors para React

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

//  Migraciones + SEED (roles, admin, traits, perros demo)
try
{
    await DbSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "DbSeeder failed. App will continue without seeding.");
}


//  Pipeline de ejecución
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowReact");
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Ruta raíz: si hay sesión => /Dogs, si no => login
app.MapGet("/", () => Results.Redirect("/Dogs"));



app.MapControllers();


app.MapFallbackToFile("/app", "app/index.html");
app.MapFallbackToFile("/app/{*path:nonfile}", "app/index.html");

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dogs}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapRazorPages();

app.Run();
