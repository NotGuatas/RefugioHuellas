using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Models;

namespace RefugioHuellas.Data
{
    public static class DataSeeder
    {
        //  Crea rol Admin y usuario admin@huellas.com
        public static async Task SeedRolesAndAdminAsync(IServiceProvider sp)
        {
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();

            const string adminRole = "Admin";
            const string adminEmail = "admin@huellas.com";
            const string adminPass = "Admin#12345";

            // Crear rol si no existe
            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole(adminRole));

            // Crear usuario admin si no existe
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, adminPass);
            }

            // Asignar rol Admin si no lo tiene
            if (!await userManager.IsInRoleAsync(admin, adminRole))
                await userManager.AddToRoleAsync(admin, adminRole);
        }

        // Preguntas del test de compatibilidad
        public static async Task SeedCompatibilityAsync(ApplicationDbContext db)
        {
            // catálogo completo de rasgos propuestos
            var traits = new List<PersonalityTrait>
            {
            new() { Key = "housingType",      Name = "Tipo de vivienda",            Prompt = "¿Vives en departamento?",                       Weight = 3, Active = true },
            new() { Key = "space",            Name = "Espacio disponible",          Prompt = "¿Tienes patio o jardín amplio?",                Weight = 3, Active = true },
            new() { Key = "time",             Name = "Tiempo libre",                Prompt = "¿Tienes tiempo para paseos diarios?",           Weight = 4, Active = true },
            new() { Key = "activityLevel",    Name = "Nivel de actividad",          Prompt = "¿Haces actividad física con frecuencia?",       Weight = 4, Active = true },
            new() { Key = "noiseTolerance",   Name = "Tolerancia al ruido",         Prompt = "¿Toleras ladridos frecuentes?",                 Weight = 2, Active = true },
            new() { Key = "kidsAtHome",       Name = "Niños en casa",               Prompt = "¿Hay niños pequeños en casa?",                  Weight = 3, Active = true },
            new() { Key = "otherPets",        Name = "Otras mascotas",              Prompt = "¿Tienes otras mascotas actualmente?",           Weight = 3, Active = true },
            new() { Key = "allergy",          Name = "Alergias",                    Prompt = "¿Alguien en casa tiene alergia a perros?",      Weight = 2, Active = true },
            new() { Key = "trainingWilling",  Name = "Entrenamiento",               Prompt = "¿Estás dispuesto a entrenar con constancia?",   Weight = 4, Active = true },
            new() { Key = "travelOften",      Name = "Viajes frecuentes",           Prompt = "¿Viajas o te ausentas de casa con frecuencia?", Weight = 2, Active = true },
            new() { Key = "budgetMonthly",    Name = "Presupuesto mensual",         Prompt = "¿Cuentas con presupuesto mensual para el perro?",Weight = 3, Active = true },
            new() { Key = "fence",            Name = "Cerca/seguridad",             Prompt = "¿Tu patio tiene cerca o medidas de seguridad?", Weight = 3, Active = true },
            };

            // agregar las que no existan por Key
            foreach (var t in traits)
            {
                var exists = await db.PersonalityTraits.AnyAsync(x => x.Key == t.Key);
                if (!exists) db.PersonalityTraits.Add(t);
            }

            await db.SaveChangesAsync();
        }

    }
}

