using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using RefugioHuellas.Services;

namespace RefugioHuellas.Controllers
{
    [Authorize]
    public class DogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CompatibilityService _compat;

        public DogsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            CompatibilityService compat)
        {
            _context = context;
            _userManager = userManager;
            _compat = compat;
        }

        // ----------- PÚBLICO -----------
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var dogs = await _context.Dogs
                .Include(d => d.OriginType)
                .OrderByDescending(d => d.IntakeDate)
                .ToListAsync();

            return View(dogs);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dog = await _context.Dogs
                .Include(d => d.OriginType)     // 🔹 cargar origen
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dog == null) return NotFound();

            // Compatibilidad personalizada para el usuario logueado
            int? compatScore = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var hasProfile = await _context.UserTraitResponses
                        .AnyAsync(r => r.UserId == userId);

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
            [Bind("Id,Name,Description,Breed,Size,EnergyLevel,IdealEnvironment,OriginTypeId,PhotoFile,HealthStatus,Sterilized,IntakeDate")]
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
            [Bind("Id,Name,Description,Breed,Size,EnergyLevel,IdealEnvironment,OriginTypeId,PhotoFile,HealthStatus,Sterilized,IntakeDate,PhotoUrl")]
            Dog dog)
        {
            if (id != dog.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["OriginTypeId"] = new SelectList(_context.OriginTypes, "Id", "Name", dog.OriginTypeId);
                return View(dog);
            }

            try
            {
                // Si viene nueva foto, la reemplazamos
                await HandleUploadAsync(dog);

                _context.Update(dog);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "Perro actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DogExists(dog.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
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

        // Manejo de subida de imagen
        private async Task HandleUploadAsync(Dog dog)
        {
            if (dog.PhotoFile != null && dog.PhotoFile.Length > 0)
            {
                var ext = Path.GetExtension(dog.PhotoFile.FileName);
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowed.Contains(ext.ToLower()))
                {
                    throw new InvalidOperationException("Formato de imagen no permitido.");
                }

                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dog.PhotoFile.CopyToAsync(stream);

                dog.PhotoUrl = $"/uploads/{fileName}";
            }
        }
    }
}
