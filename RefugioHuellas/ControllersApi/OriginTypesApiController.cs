using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;

namespace RefugioHuellas.Controllers.Api
{
    [ApiController]
    [Route("api/origin-types")]
    public class OriginTypesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public OriginTypesApiController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.OriginTypes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();

            return Ok(items);
        }
    }
}
