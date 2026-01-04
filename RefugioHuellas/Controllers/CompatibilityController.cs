using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Models;
using RefugioHuellas.Models.ViewModels;
using RefugioHuellas.Services;

namespace RefugioHuellas.Controllers
{
    [Authorize]
    public class CompatibilityController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CompatibilityService _compat;



        public CompatibilityController(ApplicationDbContext db, UserManager<IdentityUser> userManager, CompatibilityService compat)
        {
            _db = db;
            _userManager = userManager;
            _compat = compat;
        }


        // GET: /Compatibility/Test
        [HttpGet]
        public async Task<IActionResult> Test(int dogId)
        {
            var userId = _userManager.GetUserId(User)!;
            var dog = await _db.Dogs.FindAsync(dogId);
            if (dog == null) return NotFound();
        

            //  Si ya existe solicitud: NO permitir nuevo formulario
            var existing = await _db.AdoptionApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId && a.DogId == dogId);

            if (existing != null)
            {
                TempData["Info"] = $"Ya enviaste una solicitud para {dog.Name}. " +
                                   $"Compatibilidad: {existing.CompatibilityScore}%. " +
                                   $"Estado: {existing.Status}. Te informaremos por correo.";
                return RedirectToAction("Details", "Dogs", new { id = dogId });
            }

            // Armar formulario 
            var traits = await _db.PersonalityTraits
                                  .Where(t => t.Active)
                                  .OrderBy(t => t.Id)
                                  .ToListAsync();

            var vm = new CompatibilityFormVm
            {
                DogId = dog.Id,
                DogName = dog.Name,
                Answers = traits.Select(t => new CompatibilityAnswerVm
                {
                    TraitId = t.Id,
                    Key = t.Key,
                    Prompt = t.Prompt ?? t.Name,
                    Value = t.Key is "housingType" or "space" or "noiseTolerance" ? 5 : 3
                }).ToList()
            };

            return View(vm);
        }

        // POST: /Compatibility/Test
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Test(CompatibilityFormVm vm)
        {
            var userId = _userManager.GetUserId(User)!;
            var dog = await _db.Dogs.FindAsync(vm.DogId);
            if (dog == null) return NotFound();

            // Doble verificación anti-duplicado en POST
            var existing = await _db.AdoptionApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId && a.DogId == vm.DogId);

            if (existing != null)
            {
                TempData["Info"] = $"Ya enviaste una solicitud para {dog.Name}. " +
                                   $"Compatibilidad: {existing.CompatibilityScore}%. " +
                                   $"Estado: {existing.Status}.";
                return RedirectToAction("Details", "Dogs", new { id = vm.DogId });
            }

            // Calcular score en base a las respuestas del formulario
            int score = await _compat.CalculateFromAnswersAsync(dog, vm.Answers);

            // Actualizar / crear perfil del usuario con estas respuestas
            await UpsertUserProfileAsync(userId, vm.Answers);

            // Recuperar teléfono desde TempData (primer paso de Create)
            var phone = TempData["Phone"] as string ?? string.Empty;

            // Crear solicitud de adopción (todavía sin respuestas asociadas)
            var app = new AdoptionApplication
            {
                DogId = vm.DogId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pendiente",
                CompatibilityScore = score,
                Phone = phone
            };
            _db.AdoptionApplications.Add(app);

            // Guardamos aquí para que app.Id ya exista en la BD
            await _db.SaveChangesAsync();

            // Ahora sí, guardamos snapshot de las respuestas vinculadas a esta solicitud
            foreach (var a in vm.Answers)
            {
                _db.AdoptionApplicationAnswers.Add(new AdoptionApplicationAnswer
                {
                    AdoptionApplicationId = app.Id,  // ahora app.Id ya es válido
                    TraitId = a.TraitId,
                    Value = Math.Clamp(a.Value, 1, 5)
                });
            }

            // Guardamos las respuestas
            await _db.SaveChangesAsync();

            TempData["AdoptOk"] = $"Tu solicitud fue enviada. Compatibilidad: {score}%";
            return RedirectToAction("Details", "Dogs", new { id = vm.DogId });
        }



        // Guarda o actualiza el perfil de compatibilidad del usuario
        private async Task UpsertUserProfileAsync(string userId, IEnumerable<CompatibilityAnswerVm> answers)
        {
            var traitIds = answers.Select(a => a.TraitId).ToList();

            var existing = await _db.UserTraitResponses
                .Where(r => r.UserId == userId && traitIds.Contains(r.TraitId))
                .ToListAsync();

            foreach (var a in answers)
            {
                var value = Math.Clamp(a.Value, 1, 5);

                var current = existing.FirstOrDefault(r => r.TraitId == a.TraitId);
                if (current == null)
                {
                    _db.UserTraitResponses.Add(new UserTraitResponse
                    {
                        UserId = userId,
                        TraitId = a.TraitId,
                        Value = value,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    current.Value = value;
                    current.CreatedAt = DateTime.UtcNow;
                }
            }
        }

    }
}
