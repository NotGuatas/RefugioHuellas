using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Services.Compatibility;

namespace RefugioHuellas.Controllers
{
    [ApiController]
    [Route("api/adoptionapplications")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdoptionApplicationsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICompatibilityService _compat;

        public AdoptionApplicationsApiController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ICompatibilityService compat)
        {
            _context = context;
            _userManager = userManager;
            _compat = compat;
        }

        [HttpGet("my-best-matches")]
        public async Task<IActionResult> MyBestMatches()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var hasProfile = await _context.UserTraitResponses.AnyAsync(r => r.UserId == userId);
            if (!hasProfile)
                return Ok(new List<object>());

            var dogs = await _context.Dogs
                .OrderByDescending(d => d.IntakeDate)
                .ToListAsync();

            var scores = await _compat.CalculateBestMatchesForUserAsync(userId);

            string SizeLabel(string? size) => size?.Trim().ToLower() switch
            {
                "pequeño" or "pequeno" or "small" => "Pequeño",
                "mediano" or "medium" => "Mediano",
                "grande" or "large" => "Grande",
                _ => size ?? "-"
            };

            var items = dogs
                .Select(d => new
                {
                    dogId = d.Id,
                    dogName = d.Name,
                    photoUrl = d.PhotoUrl,
                    sizeLabel = SizeLabel(d.Size),
                    energy = d.EnergyLevel,
                    score = scores.TryGetValue(d.Id, out var s) ? s : 50
                })
                .OrderByDescending(x => x.score)
                .ThenBy(x => x.dogId)
                .Take(3)
                .ToList();

            return Ok(items);
        }
    }
}
