using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Models;



namespace RefugioHuellas.ControllersApi
{
    [ApiController]
    [Route("api/user-traits")]
    [Authorize] // requiere JWT
    public class UserTraitsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public UserTraitsApiController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /api/user-traits/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var items = await _db.UserTraitResponses
                .Where(x => x.UserId == user.Id)
                .Select(x => new { x.TraitId, x.Value })
                .ToListAsync();

            return Ok(items);
        }

        public class SaveRequest
        {
            public List<Item> Items { get; set; } = new();
            public class Item
            {
                public int TraitId { get; set; }
                public int Value { get; set; } // 1..5
            }
        }

        // POST: /api/user-traits/me
        [HttpPost("me")]
        public async Task<IActionResult> SaveMine([FromBody] SaveRequest req)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Validación básica
            foreach (var it in req.Items)
            {
                if (it.Value < 1 || it.Value > 5)
                    return BadRequest("Value must be between 1 and 5.");
            }

            var traitIds = req.Items.Select(i => i.TraitId).Distinct().ToList();

            // Asegurar que existan y estén activos
            var activeTraitIds = await _db.PersonalityTraits
                .Where(t => t.Active && traitIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            if (activeTraitIds.Count != traitIds.Count)
                return BadRequest("Some traits are invalid or inactive.");

            // Cargar existentes del usuario
            var existing = await _db.UserTraitResponses
                .Where(x => x.UserId == user.Id && traitIds.Contains(x.TraitId))
                .ToListAsync();

            foreach (var it in req.Items)
            {
                var row = existing.FirstOrDefault(e => e.TraitId == it.TraitId);
                if (row == null)
                {
                    _db.UserTraitResponses.Add(new UserTraitResponse
                    {
                        UserId = user.Id,
                        TraitId = it.TraitId,
                        Value = it.Value
                    });
                }
                else
                {
                    row.Value = it.Value;
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { saved = req.Items.Count });
        }
    }
}