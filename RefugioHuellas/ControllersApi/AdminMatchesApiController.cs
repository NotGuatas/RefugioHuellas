using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;

namespace RefugioHuellas.Controllers.Api
{
    [ApiController]
    [Route("api/admin/matches")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminMatchesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private const int WINDOW_DAYS_DEFAULT = 7;

        public AdminMatchesApiController(ApplicationDbContext db) => _db = db;

        private int ResolveWindowDays(int? days)
            => (days.HasValue && days.Value > 0 && days.Value <= 60) ? days.Value : WINDOW_DAYS_DEFAULT;

        [HttpGet("best")]
        public async Task<IActionResult> Best()
        {
            var dogs = await _db.Dogs.AsNoTracking()
                .OrderByDescending(d => d.IntakeDate)
                .ToListAsync();

            var result = new List<object>();

            foreach (var dog in dogs)
            {
                var topApp = await _db.AdoptionApplications.AsNoTracking()
                    .Where(a => a.DogId == dog.Id)
                    .OrderByDescending(a => a.CompatibilityScore)
                    .ThenBy(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                if (topApp != null)
                {
                    var userEmail = await _db.Users
                        .Where(u => u.Id == topApp.UserId)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();

                    result.Add(new
                    {
                        dogId = dog.Id,
                        dogName = dog.Name,
                        dogPhotoUrl = dog.PhotoUrl,
                        dogIntakeDate = dog.IntakeDate,
                        applicationId = topApp.Id,
                        userEmail,
                        topApp.CompatibilityScore,
                        topApp.Status,
                        topApp.CreatedAt
                    });
                }
                else
                {
                    result.Add(new
                    {
                        dogId = dog.Id,
                        dogName = dog.Name,
                        dogPhotoUrl = dog.PhotoUrl,
                        dogIntakeDate = dog.IntakeDate,
                        applicationId = (int?)null,
                        userEmail = (string?)null,
                        CompatibilityScore = 0,
                        Status = "Sin solicitudes",
                        CreatedAt = dog.IntakeDate
                    });
                }
            }

            return Ok(result);
        }

        [HttpGet("top")]
        public async Task<IActionResult> Top([FromQuery] int? days)
        {
            int window = ResolveWindowDays(days);
            var since = DateTime.UtcNow.AddDays(-window);

            var recentDogs = await _db.Dogs.AsNoTracking()
                .Where(d => d.IntakeDate >= since)
                .OrderByDescending(d => d.IntakeDate)
                .ToListAsync();

            var result = new List<object>();

            foreach (var dog in recentDogs)
            {
                var start = dog.IntakeDate;
                var end = dog.IntakeDate.AddDays(window);

                var apps = await _db.AdoptionApplications.AsNoTracking()
                    .Where(a => a.DogId == dog.Id && a.CreatedAt >= start && a.CreatedAt < end)
                    .OrderByDescending(a => a.CompatibilityScore)
                    .ThenBy(a => a.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                var top3 = new List<object>();
                foreach (var a in apps)
                {
                    var userEmail = await _db.Users
                        .Where(u => u.Id == a.UserId)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();

                    top3.Add(new
                    {
                        applicationId = a.Id,
                        userEmail,
                        a.CompatibilityScore,
                        a.Status,
                        a.CreatedAt
                    });
                }

                result.Add(new
                {
                    dogId = dog.Id,
                    dogName = dog.Name,
                    dogPhotoUrl = dog.PhotoUrl,
                    dogIntakeDate = dog.IntakeDate,
                    windowDays = window,
                    topCandidates = top3
                });
            }

            return Ok(new { windowDays = window, dogs = result });
        }

        public record ApproveRequest(int ApplicationId);

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] ApproveRequest req)
        {
            var app = await _db.AdoptionApplications
                .Include(a => a.Dog)
                .FirstOrDefaultAsync(a => a.Id == req.ApplicationId);

            if (app == null) return NotFound(new { message = "Solicitud no encontrada." });

            app.Status = "Aprobada";
            await _db.SaveChangesAsync();

            return Ok(new { message = "Solicitud aprobada.", dogName = app.Dog?.Name });
        }
    }
}
