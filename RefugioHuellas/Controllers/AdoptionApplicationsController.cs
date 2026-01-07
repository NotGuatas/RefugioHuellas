using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Models;
using RefugioHuellas.Models.ViewModels;
using RefugioHuellas.Services.Compatibility;
using System.Text.RegularExpressions;


namespace RefugioHuellas.Controllers
{
    [Authorize]
    public class AdoptionApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICompatibilityService _compat;

        // ventana por defecto
        private const int WINDOW_DAYS_DEFAULT = 7;

        public AdoptionApplicationsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ICompatibilityService compat)
        {
            _context = context;
            _userManager = userManager;
            _compat = compat;
        }

        private int ResolveWindowDays(int? days)
            => (days.HasValue && days.Value > 0 && days.Value <= 60) ? days.Value : WINDOW_DAYS_DEFAULT;

        private static bool IsWindowOpen(DateTime intakeUtc, int windowDays)
            => true;

        // Crear solicitud 
        [HttpGet]
        public async Task<IActionResult> Create(int dogId)
        {
            var userId = _userManager.GetUserId(User)!;

            // Verificar perro
            var dog = await _context.Dogs.FindAsync(dogId);
            if (dog == null)
            {
                TempData["Error"] = "El perro seleccionado no existe.";
                return RedirectToAction("Index", "Dogs");
            }

            // Si el usuario ya aplicó para este perro, no permitir repetir
            var existingApp = await _context.AdoptionApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId && a.DogId == dogId);

            if (existingApp != null)
            {
                TempData["Info"] = $"Ya enviaste una solicitud para {dog.Name}. " +
                                   $"Compatibilidad: {existingApp.CompatibilityScore}%. " +
                                   $"Estado: {existingApp.Status}.";
                return RedirectToAction("Details", "Dogs", new { id = dogId });
            }

            // Mostramos la vista de Create (nombre, apellido y teléfono)
            ViewBag.DogId = dogId;
            return View();
        }
        

        // (POST valida nombre, apellido y teléfono 
        //  y luego envía al formulario de compatibilidad)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int dogId, string firstName, string lastName, string phone)
        {
            var userId = _userManager.GetUserId(User)!;

            var dog = await _context.Dogs.FindAsync(dogId);
            if (dog == null)
            {
                TempData["Error"] = "El perro seleccionado no existe.";
                return RedirectToAction("Index", "Dogs");
            }

            var existingApp = await _context.AdoptionApplications
                .FirstOrDefaultAsync(a => a.UserId == userId && a.DogId == dogId);
            if (existingApp != null)
            {
                TempData["Info"] = $"Ya enviaste una solicitud para {dog.Name}. " +
                                   $"Compatibilidad: {existingApp.CompatibilityScore}%. " +
                                   $"Estado: {existingApp.Status}.";
                return RedirectToAction("Details", "Dogs", new { id = dogId });
            }

            // Nombre obligatorio
            if (string.IsNullOrWhiteSpace(firstName))
            {
                ModelState.AddModelError("firstName", "El nombre es obligatorio.");
            }

            // Apellido obligatorio
            if (string.IsNullOrWhiteSpace(lastName))
            {
                ModelState.AddModelError("lastName", "El apellido es obligatorio.");
            }

            // Teléfono (dato sensible)
            // Regla: debe comenzar con 09 y tener exactamente 10 dígitos.
            if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, @"^09\d{8}$"))
            {
                ModelState.AddModelError("phone", "El teléfono debe comenzar con 09 y tener 10 dígitos (ej: 0991234567).");
            }

            if (!ModelState.IsValid)
            {
                // Volvemos al formulario mostrando los errores
                ViewBag.DogId = dogId;
                return View();
            }

            // ¿El usuario ya tiene perfil de compatibilidad guardado?
            var hasProfile = await _context.UserTraitResponses
                .AnyAsync(r => r.UserId == userId);

            if (hasProfile)
            {
                // Se Calcula el porcentaje usando el perfil guardado.
                var score = await _compat.CalculateFromUserProfileAsync(dog, userId);

                var app = new AdoptionApplication
                {
                    DogId = dogId,
                    UserId = userId,
                    Phone = phone,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pendiente",
                    CompatibilityScore = score
                };

                _context.AdoptionApplications.Add(app);
                await _context.SaveChangesAsync();

                TempData["AdoptOk"] = $"Tu solicitud fue enviada. Compatibilidad: {score}%";
                return RedirectToAction("Details", "Dogs", new { id = dogId });
            }

            //  Primera vez: guarda datos básicos en TempData y manda al formulario de compatibilidad
            TempData["Phone"] = phone;
            TempData["FirstName"] = firstName;
            TempData["LastName"] = lastName;

            return RedirectToAction("Test", "Compatibility", new { dogId });
        }



        // ===== Mis solicitudes =====
        [HttpGet]
        public async Task<IActionResult> My()
        {
            var userId = _userManager.GetUserId(User)!;

            var apps = await _context.AdoptionApplications
                .Include(a => a.Dog)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(apps);
        }

        // ===== Panel admin (sin aprobar aquí) =====
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var apps = await _context.AdoptionApplications
                .Include(a => a.Dog)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AdminAdoptionVm
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserEmail = _context.Users.Where(u => u.Id == a.UserId).Select(u => u.Email).FirstOrDefault(),
                    DogName = a.Dog != null ? a.Dog.Name : "(eliminado)",
                    CompatibilityScore = a.CompatibilityScore,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return View(apps);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDetails(int id)
        {
            var app = await _context.AdoptionApplications
                .Include(a => a.Dog)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound();

            var vm = new AdminAdoptionDetailVm
            {
                Id = app.Id,
                UserEmail = _context.Users.Where(u => u.Id == app.UserId).Select(u => u.Email).FirstOrDefault(),
                DogName = app.Dog?.Name ?? "(eliminado)",
                CompatibilityScore = app.CompatibilityScore,
                Status = app.Status,
                CreatedAt = app.CreatedAt,
                Answers = await _context.AdoptionApplicationAnswers
                    .Include(x => x.Trait)
                    .Where(x => x.AdoptionApplicationId == app.Id)
                    .Select(x => new ValueTuple<string, int>(x.Trait!.Prompt ?? x.Trait!.Name, x.Value))
                    .ToListAsync()
            };

            return View(vm);
        }

        // ===== Top 3 (recientes) =====
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TopMatches(int? days)
        {
            int window = ResolveWindowDays(days);
            var since = DateTime.UtcNow.AddDays(-window);

            var recentDogs = await _context.Dogs
                .Where(d => d.IntakeDate >= since)
                .OrderByDescending(d => d.IntakeDate)
                .ToListAsync();

            var result = new List<TopMatchVm>();

            foreach (var dog in recentDogs)
            {
                var start = dog.IntakeDate;
                var end = dog.IntakeDate.AddDays(window);

                var apps = await _context.AdoptionApplications
                    .Where(a => a.DogId == dog.Id && a.CreatedAt >= start && a.CreatedAt < end)
                    .OrderByDescending(a => a.CompatibilityScore)
                    .ThenBy(a => a.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                var top3 = apps.Select(a => new TopMatchCandidateVm
                {
                    ApplicationId = a.Id,
                    UserEmail = _context.Users.Where(u => u.Id == a.UserId).Select(u => u.Email).FirstOrDefault(),
                    CompatibilityScore = a.CompatibilityScore,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                }).ToList();

                result.Add(new TopMatchVm
                {
                    DogId = dog.Id,
                    DogName = dog.Name,
                    IntakeDate = dog.IntakeDate,
                    TopCandidates = top3
                });
            }

            ViewBag.WindowDays = window;
            return View(result);
        }

        public async Task<IActionResult> MyBestMatches()
        {
            var userId = _userManager.GetUserId(User)!;

            // Verificar si el usuario tiene perfil de compatibilidad
            var hasProfile = await _context.UserTraitResponses
                .AnyAsync(r => r.UserId == userId);

            var window = ResolveWindowDays(null);
            ViewBag.WindowDays = window;
            ViewBag.NeedsProfile = !hasProfile;

            // Si NO tiene perfil, devolvemos la vista con lista vacía
            if (!hasProfile)
            {
                return View(new List<MyBestMatchVm>());
            }

            // Si SÍ tiene perfil, calculamos los mejores matches
            var dogs = await _context.Dogs
                .OrderByDescending(d => d.IntakeDate)
                .ToListAsync();

            var scores = await _compat.CalculateBestMatchesForUserAsync(userId);

            var items = dogs
                .Where(d => IsWindowOpen(d.IntakeDate, window))
                .Select(d => new MyBestMatchVm
                {
                    DogId = d.Id,
                    DogName = d.Name,
                    PhotoUrl = d.PhotoUrl,
                    Size = d.Size,
                    EnergyLevel = d.EnergyLevel,
                    IntakeDate = d.IntakeDate,
                    CompatibilityScore = scores.TryGetValue(d.Id, out var s) ? s : 50
                })
                .OrderByDescending(x => x.CompatibilityScore)
                .ThenBy(x => x.IntakeDate)
                .Take(3)
                .ToList();

            return View(items);
        }



        //  Mejor candidato (ganador por perro) - sin esperar ventana
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BestMatches(int? days)
        {
            // Ignoramos "days" a propósito: ya no usamos ventana de días
            var dogs = await _context.Dogs
                .OrderByDescending(d => d.IntakeDate)
                .ToListAsync();

            var result = new List<BestMatchVm>();

            foreach (var dog in dogs)
            {
                // Tomamos SIEMPRE la mejor solicitud para ese perro, sin filtrar por fechas
                var topApp = await _context.AdoptionApplications
                    .Where(a => a.DogId == dog.Id)
                    .OrderByDescending(a => a.CompatibilityScore)
                    .ThenBy(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                if (topApp != null)
                {
                    var userEmail = await _context.Users
                        .Where(u => u.Id == topApp.UserId)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();

                    result.Add(new BestMatchVm
                    {
                        DogId = dog.Id,
                        DogName = dog.Name,
                        IntakeDate = dog.IntakeDate,
                        ApplicationId = topApp.Id,
                        UserEmail = userEmail,
                        CompatibilityScore = topApp.CompatibilityScore,
                        Status = topApp.Status,
                        CreatedAt = topApp.CreatedAt
                    });
                }
                else
                {
                    result.Add(new BestMatchVm
                    {
                        DogId = dog.Id,
                        DogName = dog.Name,
                        IntakeDate = dog.IntakeDate,
                        ApplicationId = null,
                        UserEmail = null,
                        CompatibilityScore = 0,
                        Status = "Sin solicitudes",
                        CreatedAt = dog.IntakeDate
                    });
                }
            }

            return View(result);
        }


        // ===== Aprobar ganador desde BestMatches =====
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ApproveFromTop(int applicationId)
        {
            var app = await _context.AdoptionApplications
                .Include(a => a.Dog)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null)
            {
                TempData["Error"] = "La solicitud no existe.";
                return RedirectToAction("BestMatches");
            }

            app.Status = "Aprobada";
            await _context.SaveChangesAsync();

            TempData["Ok"] = $"Solicitud aprobada para {app.Dog?.Name}.";
            return RedirectToAction("BestMatches");
        }
    }
}
