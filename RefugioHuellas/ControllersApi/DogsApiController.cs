using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Services.Compatibility;

namespace RefugioHuellas.Controllers.Api
{
    [ApiController]
    [Route("api/dogs")]
    public class DogsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICompatibilityService _compat;
        public DogsApiController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            ICompatibilityService compat)   // 👈 ESTA ES LA CLAVE
        {
            _db = db;
            _userManager = userManager;
            _compat = compat;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var dogs = await _db.Dogs
                .OrderByDescending(d => d.IntakeDate)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.PhotoUrl,
                    d.Size,
                    d.EnergyLevel,
                    d.IdealEnvironment
                })
                .ToListAsync();

            return Ok(dogs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var dog = await _db.Dogs.FirstOrDefaultAsync(d => d.Id == id);
            if (dog == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            int? score = null;

            if (!string.IsNullOrEmpty(userId))
            {
                var hasProfile = await _db.UserTraitResponses.AnyAsync(r => r.UserId == userId);
                if (hasProfile)
                    score = await _compat.CalculateFromUserProfileAsync(dog, userId);
            }

            return Ok(new
            {
                dog.Id,
                dog.Name,
                dog.PhotoUrl,
                dog.Size,
                dog.EnergyLevel,
                dog.IdealEnvironment,
                CompatibilityScore = score
            });
        }
    }
}
