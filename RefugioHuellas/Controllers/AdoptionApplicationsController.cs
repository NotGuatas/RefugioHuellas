using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RefugioHuellas.Data;
using RefugioHuellas.Models;

namespace RefugioHuellas.Controllers
{
    [Authorize] // solo logueados pueden llegar aquí
    public class AdoptionApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdoptionApplicationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /AdoptionApplications/Create?dogId=5
        public IActionResult Create(int dogId)
        {
            // Puedes validar que el dog existe si deseas
            ViewBag.DogId = dogId;
            return View();
        }

        // POST: /AdoptionApplications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int dogId, string? notes) // notes opcional si luego quieres
        {
            var userId = _userManager.GetUserId(User)!;

            var app = new AdoptionApplication
            {
                DogId = dogId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pendiente"
            };

            _context.AdoptionApplications.Add(app);
            await _context.SaveChangesAsync();

            TempData["AdoptOk"] = "Tu solicitud fue enviada. Te contactaremos pronto.";
            return RedirectToAction("Details", "Dogs", new { id = dogId });
        }
    }
}
