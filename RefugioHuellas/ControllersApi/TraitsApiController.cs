using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;


namespace RefugioHuellas.ControllersApi
{
    [ApiController]
    [Route("api/traits")]
    public class TraitsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public TraitsApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /api/traits
        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            var traits = await _db.PersonalityTraits
                .Where(t => t.Active)
                .OrderBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Key,
                    t.Name,
                    t.Weight,
                    t.Prompt
                })
                .ToListAsync();

            return Ok(traits);
        }
    }
}