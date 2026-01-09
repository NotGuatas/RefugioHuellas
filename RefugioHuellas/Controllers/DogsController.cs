using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using RefugioHuellas.Data.Repositories;
using RefugioHuellas.Services.Compatibility;
using RefugioHuellas.Services.Storage;

namespace RefugioHuellas.Controllers
{
    [Authorize]
    public class DogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDogRepository _dogs;
        private readonly IUserTraitResponseRepository _responses;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICompatibilityService _compat;
        private readonly IPhotoStorage _photos;

        public DogsController(
            ApplicationDbContext context,
            IDogRepository dogs,
            IUserTraitResponseRepository responses,
            UserManager<IdentityUser> userManager,
            ICompatibilityService compat,
            IPhotoStorage photos)
        {
            _context = context;
            _dogs = dogs;
            _responses = responses;
            _userManager = userManager;
            _compat = compat;
            _photos = photos;
        }

        // ----------- PÚBLICO -----------
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var dogs = await _dogs.GetAllAsync(includeOriginType: true);

            return View(dogs);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dog = await _dogs.GetByIdAsync(id.Value, includeOriginType: true);

            if (dog == null) return NotFound();

            // Compatibilidad personalizada para el usuario logueado
            int? compatScore = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var hasProfile = await _responses.HasProfileAsync(userId);

                    if (hasProfile)
                    {
                        compatScore = await _compat.CalculateFromUserProfileAsync(dog, userId);
                    }
                }
            }

            ViewBag.CompatScore = compatScore;

            return View(dog);
        }

        // ----------- SOLO ADMIN -----------
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["OriginTypeId"] = new SelectList(_context.OriginTypes, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,Description,Breed,Size,EnergyLevel,IdealEnvironment,OriginTypeId,PhotoFile,HealthStatus,Sterilized,PhotoUrl")]
            Dog dog)

        {
            // Requerimos imagen al crear
            if (dog.PhotoFile == null || dog.PhotoFile.Length == 0)
                ModelState.AddModelError("PhotoFile", "Debes subir una foto del perro.");

            if (!ModelState.IsValid)
            {
                ViewData["OriginTypeId"] = new SelectList(_context.OriginTypes, "Id", "Name", dog.OriginTypeId);
                return View(dog);
            }

            await HandleUploadAsync(dog); // establece dog.PhotoUrl


            dog.IntakeDate = DateTime.UtcNow;


            _context.Add(dog);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Perro creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dog = await _context.Dogs.FindAsync(id);
            if (dog == null) return NotFound();

            ViewData["OriginTypeId"] = new SelectList(_context.OriginTypes, "Id", "Name", dog.OriginTypeId);
            return View(dog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,Description,Breed,Size,EnergyLevel,IdealEnvironment,OriginTypeId,PhotoFile,HealthStatus,Sterilized")]
    Dog dog)
        {
            if (id != dog.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["OriginTypeId"] = new SelectList(_context.OriginTypes, "Id", "Name", dog.OriginTypeId);
                return View(dog);
            }

            var existing = await _context.Dogs.FirstOrDefaultAsync(d => d.Id == id);
            if (existing == null) return NotFound();

            try
            {
                // Actualiza campos editables
                existing.Name = dog.Name;
                existing.Description = dog.Description;
                existing.Breed = dog.Breed;
                existing.Size = dog.Size;
                existing.EnergyLevel = dog.EnergyLevel;
                existing.IdealEnvironment = dog.IdealEnvironment;
                existing.OriginTypeId = dog.OriginTypeId;
                existing.HealthStatus = dog.HealthStatus;
                existing.Sterilized = dog.Sterilized;

                // Si viene nueva foto, reemplaza PhotoUrl
                if (dog.PhotoFile != null && dog.PhotoFile.Length > 0)
                {
                    var url = await _photos.SaveAsync(dog.PhotoFile);
                    if (!string.IsNullOrEmpty(url))
                        existing.PhotoUrl = url;
                }

                // IMPORTANTÍSIMO: NO toques IntakeDate (se queda el valor original en BD)
                await _context.SaveChangesAsync();

                TempData["Ok"] = "Perro actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DogExists(dog.Id)) return NotFound();
                throw;
            }
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var dog = await _context.Dogs
                .Include(d => d.OriginType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dog == null) return NotFound();

            return View(dog);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dog = await _context.Dogs.FindAsync(id);
            if (dog == null) return NotFound();

            _context.Dogs.Remove(dog);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Perro eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool DogExists(int id)
            => _context.Dogs.Any(e => e.Id == id);

        // SRP: Manejo de subida de imagen queda delegado a un servicio.
        private async Task HandleUploadAsync(Dog dog)
        {
            var url = await _photos.SaveAsync(dog.PhotoFile);
            if (!string.IsNullOrEmpty(url))
                dog.PhotoUrl = url;
        }
    }
}
