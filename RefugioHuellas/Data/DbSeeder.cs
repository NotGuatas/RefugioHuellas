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

            // 3) Cuenta admin
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

            // 4) Rasgos de compatibilidad (12 preguntas) — SEED INTELIGENTE
            var traits = new List<PersonalityTrait>
            {
                new() { Key = "housingType",     Name = "Tipo de vivienda",            Prompt = "¿Vives en departamento?",                        Weight = 3, Active = true },
                new() { Key = "space",           Name = "Espacio disponible",          Prompt = "¿Tienes patio o jardín amplio?",                 Weight = 3, Active = true },
                new() { Key = "time",            Name = "Tiempo libre",                Prompt = "¿Tienes tiempo para paseos diarios?",            Weight = 4, Active = true },
                new() { Key = "activityLevel",   Name = "Nivel de actividad",          Prompt = "¿Haces actividad física con frecuencia?",        Weight = 4, Active = true },
                new() { Key = "noiseTolerance",  Name = "Tolerancia al ruido",         Prompt = "¿Toleras ladridos frecuentes?",                  Weight = 2, Active = true },
                new() { Key = "kidsAtHome",      Name = "Niños en casa",               Prompt = "¿Hay niños pequeños en casa?",                   Weight = 3, Active = true },
                new() { Key = "otherPets",       Name = "Otras mascotas",              Prompt = "¿Tienes otras mascotas actualmente?",            Weight = 3, Active = true },
                new() { Key = "allergy",         Name = "Alergias",                    Prompt = "¿Alguien en casa tiene alergia a perros?",       Weight = 2, Active = true },
                new() { Key = "trainingWilling", Name = "Entrenamiento",               Prompt = "¿Estás dispuesto a entrenar con constancia?",    Weight = 4, Active = true },
                new() { Key = "travelOften",     Name = "Viajes frecuentes",           Prompt = "¿Viajas o te ausentas de casa con frecuencia?",  Weight = 2, Active = true },
                new() { Key = "budgetMonthly",   Name = "Presupuesto mensual",         Prompt = "¿Cuentas con presupuesto mensual para el perro?", Weight = 3, Active = true },
                new() { Key = "fence",           Name = "Cerca/seguridad",             Prompt = "¿Tu patio tiene cerca o medidas de seguridad?",  Weight = 3, Active = true },
            };

            foreach (var trait in traits)
            {
                // Si existe, actualizar
                var existing = await context.PersonalityTraits
                    .FirstOrDefaultAsync(t => t.Key == trait.Key);

                if (existing == null)
                {
                    // Nuevo
                    context.PersonalityTraits.Add(trait);
                }
                else
                {
                    // Actualizar valores si hubo cambios
                    existing.Name = trait.Name;
                    existing.Prompt = trait.Prompt;
                    existing.Weight = trait.Weight;
                    existing.Active = true;
                }
            }

            await context.SaveChangesAsync();

            // 5) Tipos de origen del perro
            if (!await context.OriginTypes.AnyAsync())
            {
                context.OriginTypes.AddRange(
                    new OriginType { Name = "Calle" },
                    new OriginType { Name = "Rescate policial" },
                    new OriginType { Name = "Rescate vecinal" },
                    new OriginType { Name = "Abandono" },
                    new OriginType { Name = "Entregado por familia" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
