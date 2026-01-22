using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;

namespace RefugioHuellas.Controllers.Api
{
    [ApiController]
    [Route("api/admin/adoptions")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminAdoptionsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AdminAdoptionsApiController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var apps = await _db.AdoptionApplications
                .AsNoTracking()
                .Include(a => a.Dog)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    userEmail = _db.Users.Where(u => u.Id == a.UserId).Select(u => u.Email).FirstOrDefault(),
                    dogId = a.DogId,
                    dogName = a.Dog != null ? a.Dog.Name : "(eliminado)",
                    a.CompatibilityScore,
                    a.Status,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(apps);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var app = await _db.AdoptionApplications
                .AsNoTracking()
                .Include(a => a.Dog)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound(new { message = "Solicitud no encontrada." });

            var answers = await _db.AdoptionApplicationAnswers
                .AsNoTracking()
                .Include(x => x.Trait)
                .Where(x => x.AdoptionApplicationId == id)
                .Select(x => new
                {
                    trait = x.Trait!.Prompt ?? x.Trait!.Name,
                    x.Value
                })
                .ToListAsync();

            var userEmail = await _db.Users.Where(u => u.Id == app.UserId).Select(u => u.Email).FirstOrDefaultAsync();

            return Ok(new
            {
                app.Id,
                app.UserId,
                userEmail,
                dogId = app.DogId,
                dogName = app.Dog?.Name ?? "(eliminado)",
                app.Phone,
                app.CompatibilityScore,
                app.Status,
                app.CreatedAt,
                answers
            });
        }

        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var app = await _db.AdoptionApplications
                .Include(a => a.Dog)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound(new { message = "Solicitud no encontrada." });

            app.Status = "Aprobada";
            await _db.SaveChangesAsync();

            return Ok(new { message = "Solicitud aprobada.", dogName = app.Dog?.Name });
        }
    }
}
