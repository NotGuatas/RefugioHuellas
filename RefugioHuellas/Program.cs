using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using RefugioHuellas.Data;
using RefugioHuellas.Data.Repositories;
using RefugioHuellas.Services;
using RefugioHuellas.Services.Compatibility;
using RefugioHuellas.Services.Compatibility.Rules;
using RefugioHuellas.Services.Storage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ----------------- Servicios -----------------

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        }));

builder.Services.AddControllersWithViews();

// Identity Core: solo para que UserManager<IdentityUser> esté disponible en DI
// y para mantener la tabla AspNetUsers. NO registra login por cookie.
builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Autenticación: cookie local + OIDC via Keycloak
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["Keycloak:Authority"];
    options.ClientId = builder.Configuration["Keycloak:ClientId"];
    options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.RequireHttpsMetadata = true;
    options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    // sub → NameIdentifier (para que UserManager.GetUserId(User) funcione)
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "preferred_username",
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = ctx =>
        {
            // Keycloak pone los roles en el access_token, no en el id_token.
            // Lo leemos directamente del token endpoint response.
            var accessToken = ctx.TokenEndpointResponse?.AccessToken;
            if (accessToken == null) return Task.CompletedTask;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);
            var resourceClaim = jwt.Claims.FirstOrDefault(c => c.Type == "resource_access");
            if (resourceClaim == null) return Task.CompletedTask;

            var identity = (ClaimsIdentity)ctx.Principal!.Identity!;
            try
            {
                var doc = JsonDocument.Parse(resourceClaim.Value);
                if (doc.RootElement.TryGetProperty("refugiohuellas", out var client) &&
                    client.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        var name = role.GetString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            // Capitalizar para que coincida con [Authorize(Roles = "Admin")]
                            var normalized = char.ToUpper(name[0]) + name[1..];
                            identity.AddClaim(new Claim(ClaimTypes.Role, normalized));
                        }
                    }
                }
            }
            catch { /* claim malformado, ignorar */ }

            return Task.CompletedTask;
        },

        OnRedirectToIdentityProviderForSignOut = ctx =>
        {
            // Redirigir al logout de Keycloak y volver al inicio
            var keycloakLogout = $"{builder.Configuration["Keycloak:Authority"]}/protocol/openid-connect/logout";
            ctx.ProtocolMessage.IssuerAddress = keycloakLogout;
            return Task.CompletedTask;
        }
    };
});

// ----------------- Inyección de dependencias (SOLID + Patrones) -----------------

builder.Services.AddScoped<IDogRepository, EfDogRepository>();
builder.Services.AddScoped<ITraitRepository, EfTraitRepository>();
builder.Services.AddScoped<IUserTraitResponseRepository, EfUserTraitResponseRepository>();

builder.Services.AddScoped<ITraitRule, HousingTypeRule>();
builder.Services.AddScoped<ITraitRule, SpaceRule>();
builder.Services.AddScoped<ITraitRule, TimeRule>();
builder.Services.AddScoped<ITraitRule, NoiseToleranceRule>();
builder.Services.AddScoped<ITraitRule, ActivityLevelRule>();

builder.Services.AddScoped<ITraitRuleFactory, TraitRuleFactory>();
builder.Services.AddScoped<ICompatibilityService, CompatibilityService>();
builder.Services.AddScoped<IPhotoStorage, LocalPhotoStorage>();
builder.Services.AddHttpClient<EscaladaECService>();

var dpKeysPath = "/var/data/dpkeys";
Directory.CreateDirectory(dpKeysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dpKeysPath));

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

if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("RUN_SEEDER") == "true")
{
    try
    {
        await DbSeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "DbSeeder failed. App will continue without seeding.");
    }
}

var uploadRoot = Environment.GetEnvironmentVariable("UPLOAD_ROOT");
if (!string.IsNullOrWhiteSpace(uploadRoot))
{
    Directory.CreateDirectory(uploadRoot);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadRoot),
        RequestPath = "/uploads"
    });
}

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

app.MapGet("/", () => Results.Redirect("/Dogs"));

app.MapControllers();

app.MapFallbackToFile("/app", "app/index.html");
app.MapFallbackToFile("/app/{*path:nonfile}", "app/index.html");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dogs}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
