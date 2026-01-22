using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Models;
using RefugioHuellas.Services.Compatibility;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace RefugioHuellas.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/adoptions")]

    public class AdoptionsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICompatibilityService _compat;

        public AdoptionsApiController(ApplicationDbContext db, UserManager<IdentityUser> userManager, ICompatibilityService compat)
        {
            _db = db;
            _userManager = userManager;
            _compat = compat;
        }

        public record CreateAdoptionRequest(int DogId, string FirstName, string LastName, string Phone);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdoptionRequest req)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "No autenticado." });

            var dog = await _db.Dogs.FirstOrDefaultAsync(d => d.Id == req.DogId);
            if (dog == null)
                return NotFound(new { message = "El perro seleccionado no existe." });

            var existing = await _db.AdoptionApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId && a.DogId == req.DogId);

            if (existing != null)
                return Conflict(new { message = $"Ya enviaste una solicitud para {dog.Name}." });

            if (string.IsNullOrWhiteSpace(req.FirstName))
                return BadRequest(new { message = "El nombre es obligatorio." });

            if (string.IsNullOrWhiteSpace(req.LastName))
                return BadRequest(new { message = "El apellido es obligatorio." });

            if (string.IsNullOrWhiteSpace(req.Phone) || !Regex.IsMatch(req.Phone, @"^09\d{8}$"))
                return BadRequest(new { message = "El teléfono debe comenzar con 09 y tener 10 dígitos (ej: 0991234567)." });

            var hasProfile = await _db.UserTraitResponses.AnyAsync(r => r.UserId == userId);
            if (!hasProfile)
            {
                return Conflict(new
                {
                    message = "Antes de adoptar, completa tu perfil de compatibilidad.",
                    code = "PROFILE_REQUIRED"
                });
            }

            var score = await _compat.CalculateFromUserProfileAsync(dog, userId);

            var app = new AdoptionApplication
            {
                DogId = req.DogId,
                UserId = userId,
                Phone = req.Phone,
                CreatedAt = DateTime.UtcNow,
                Status = "Pendiente",
                CompatibilityScore = score
            };

            _db.AdoptionApplications.Add(app);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Tu solicitud fue enviada.",
                adoptionId = app.Id,
                compatibilityScore = score,
                status = app.Status
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> My()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "No autenticado." });

            var apps = await _db.AdoptionApplications
                .AsNoTracking()
                .Include(a => a.Dog)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    a.Id,
                    a.DogId,
                    dogName = a.Dog != null ? a.Dog.Name : "(eliminado)",
                    photoUrl = a.Dog != null ? a.Dog.PhotoUrl : null,
                    a.Phone,
                    a.CompatibilityScore,
                    a.Status,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(apps);
        }
    }
}
