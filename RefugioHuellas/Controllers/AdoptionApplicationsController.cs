using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Models;
using RefugioHuellas.Models.ViewModels;
using RefugioHuellas.Services;
using System.Text.RegularExpressions;


namespace RefugioHuellas.Controllers
{
    [Authorize]
    public class AdoptionApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CompatibilityService _compat;

        // Cambia este valor si quieres otra ventana por defecto
        private const int WINDOW_DAYS_DEFAULT = 7;

        public AdoptionApplicationsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            CompatibilityService compat)
        {
            _context = context;
            _userManager = userManager;
            _compat = compat;
        }

        // ===== Helpers de ventana =====
        private int ResolveWindowDays(int? days)
            => (days.HasValue && days.Value > 0 && days.Value <= 60) ? days.Value : WINDOW_DAYS_DEFAULT;

        private static bool IsWindowOpen(DateTime intakeUtc, int windowDays)
            => DateTime.UtcNow < intakeUtc.AddDays(windowDays);

        // ===== Crear solicitud (bloquea si la ventana está cerrada) =====
        // Evitar duplicados y bloquear si la ventana ya cerró
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

            // Bloqueo por ventana cerrada (usa la misma regla que en Admin: 7 días por defecto)
            int window = ResolveWindowDays(null); // por defecto 7
            var closeAtUtc = dog.IntakeDate.AddDays(window);
            if (DateTime.UtcNow >= closeAtUtc)
            {
                TempData["Error"] = $"La ventana de adopción de {dog.Name} cerró el {closeAtUtc.ToLocalTime():g}.";
                return RedirectToAction("Details", "Dogs", new { id = dogId });
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

        // (Este POST ahora valida nombre, apellido y teléfono en BACK-END
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
                TempData["Info"] = "Ya has enviado una solicitud para este perro.";
                return RedirectToAction("Details", "Dogs", new { id = dogId });
            }

            // BLOQUEO: ventana cerrada
            if (!IsWindowOpen(dog.IntakeDate, WINDOW_DAYS_DEFAULT))
            {
                TempData["Error"] = "La ventana de postulaciones para este perro ya está cerrada.";
                return RedirectToAction("Details", "Dogs", new { id = dogId });
            }

            // ✅ Validación en BACK-END

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

            // Seguimos con el flujo normal: mandar al formulario de compatibilidad.
            TempData["Error"] = $"Gracias {firstName}, ahora completa el formulario de compatibilidad para este perrito.";
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

        // ===== Mejor candidato (ganador por perro dentro de ventana) =====
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BestMatches(int? days)
        {
            int window = ResolveWindowDays(days);

            var dogs = await _context.Dogs
                .OrderByDescending(d => d.IntakeDate)
                .ToListAsync();

            var result = new List<BestMatchVm>();

            foreach (var dog in dogs)
            {
                var start = dog.IntakeDate;
                var end = dog.IntakeDate.AddDays(window);

                var topApp = await _context.AdoptionApplications
                    .Where(a => a.DogId == dog.Id && a.CreatedAt >= start && a.CreatedAt < end)
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

            ViewBag.WindowDays = window;
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
