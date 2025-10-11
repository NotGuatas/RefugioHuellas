using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Models;

namespace RefugioHuellas.Controllers
{
    [Authorize]
    public class DogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ----------- PÚBLICO -----------
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var dogs = await _context.Dogs.ToListAsync();
            return View(dogs);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dog = await _context.Dogs.FirstOrDefaultAsync(m => m.Id == id);
            if (dog == null) return NotFound();

            return View(dog);
        }

        // ----------- SOLO ADMIN -----------
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,PhotoFile,HealthStatus,Sterilized,IntakeDate")] Dog dog)
        {
            // Requerimos imagen al crear
            if (dog.PhotoFile == null || dog.PhotoFile.Length == 0)
                ModelState.AddModelError("PhotoFile", "Selecciona una imagen.");

            if (!ModelState.IsValid) return View(dog);

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

            return View(dog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,PhotoFile,HealthStatus,Sterilized,IntakeDate")] Dog dog)
        {
            if (id != dog.Id) return NotFound();
            if (!ModelState.IsValid) return View(dog);

            var existing = await _context.Dogs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (existing == null) return NotFound();

            // Si suben nueva imagen, se reemplaza. Si no, se conserva la actual.
            dog.PhotoUrl = existing.PhotoUrl;
            await HandleUploadAsync(dog);

            existing.Name = dog.Name;
            existing.Description = dog.Description;
            existing.HealthStatus = dog.HealthStatus;
            existing.Sterilized = dog.Sterilized;
            existing.IntakeDate = dog.IntakeDate;
            existing.PhotoUrl = dog.PhotoUrl;

            try
            {
                _context.Update(existing);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Perro actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Dogs.Any(e => e.Id == dog.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var dog = await _context.Dogs.FirstOrDefaultAsync(m => m.Id == id);
            if (dog == null) return NotFound();

            return View(dog);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dog = await _context.Dogs.FindAsync(id);
            if (dog != null) _context.Dogs.Remove(dog);

            await _context.SaveChangesAsync();
            TempData["Ok"] = "Perro eliminado.";
            return RedirectToAction(nameof(Index));
        }

        // ----------- Helper: subir imagen -----------
        private async Task HandleUploadAsync(Dog dog)
        {
            if (dog.PhotoFile is { Length: > 0 })
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(dog.PhotoFile.FileName).ToLowerInvariant();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("PhotoFile", "Formato de imagen no permitido.");
                    return;
                }

                if (dog.PhotoFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    ModelState.AddModelError("PhotoFile", "La imagen no debe superar 5 MB.");
                    return;
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
