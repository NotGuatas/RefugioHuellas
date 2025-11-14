using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Models;

namespace RefugioHuellas.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var context = sp.GetRequiredService<ApplicationDbContext>();
            var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

            // 1) Migraciones
            await context.Database.MigrateAsync();

            // 2) Roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 3) Admin
            const string adminEmail = "admin@huellas.com";
            const string adminPass = "Admin123$";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPass);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // 4) Traits de personalidad (solo si no hay)
            if (!await context.PersonalityTraits.AnyAsync())
            {
                context.PersonalityTraits.AddRange(
                    new PersonalityTrait { Key = "housingType", Name = "Tipo de vivienda", Prompt = "¿Dónde vives actualmente?", Active = true },
                    new PersonalityTrait { Key = "space", Name = "Espacio disponible", Prompt = "¿Cuánto espacio tiene el perro?", Active = true },
                    new PersonalityTrait { Key = "noiseTolerance", Name = "Ruido", Prompt = "¿Qué tan tolerante eres al ruido?", Active = true },
                    new PersonalityTrait { Key = "activityLevel", Name = "Actividad", Prompt = "¿Qué tan activo eres físicamente?", Active = true },
                    new PersonalityTrait { Key = "experiencePets", Name = "Experiencia", Prompt = "¿Tienes experiencia previa con perros?", Active = true }
                );
                await context.SaveChangesAsync();
            }

            // 5) Perros de ejemplo (solo si está vacío)
            if (!await context.Dogs.AnyAsync())
            {
                context.Dogs.AddRange(
                    new Dog
                    {
                        Name = "Lucas",
                        Description = "Perrito encontrado por el sector de La Merced.",
                        HealthStatus = "Con todas sus vacunas y en buen estado.",
                        Sterilized = true,
                        IntakeDate = DateTime.UtcNow.AddDays(-5),
                        Breed = "Mestizo pequeño",
                        Size = "Pequeño",
                        EnergyLevel = 4,
                        IdealEnvironment = "Departamento",
                        PhotoUrl = "/uploads/lucas.jpg"   // puedes actualizar luego si quieres
                    },
                    new Dog
                    {
                        Name = "Rocky",
                        Description = "Muy juguetón, rescatado del Batán Bajo.",
                        HealthStatus = "Buena salud.",
                        Sterilized = true,
                        IntakeDate = DateTime.UtcNow.AddDays(-3),
                        Breed = "Mestizo mediano",
                        Size = "Mediano",
                        EnergyLevel = 5,
                        IdealEnvironment = "Casa con patio",
                        PhotoUrl = "/uploads/rocky.jpg"
                    }
                    // agrega más si quieres
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
