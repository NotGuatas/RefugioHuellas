using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Data;
using RefugioHuellas.Models;
using RefugioHuellas.Services.Storage;

namespace RefugioHuellas.Controllers.Api
{
    [ApiController]
    [Route("api/admin/dogs")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminDogsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPhotoStorage _photos;

        public AdminDogsApiController(ApplicationDbContext db, IPhotoStorage photos)
        {
            _db = db;
            _photos = photos;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var dogs = await _db.Dogs
                .AsNoTracking()
                .Include(d => d.OriginType)
                .OrderByDescending(d => d.IntakeDate)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Description,
                    d.Breed,
                    d.Size,
                    d.EnergyLevel,
                    d.IdealEnvironment,
                    d.PhotoUrl,
                    d.HealthStatus,
                    d.Sterilized,
                    d.IntakeDate,
                    d.OriginTypeId,
                    originTypeName = d.OriginType != null ? d.OriginType.Name : null
                })
                .ToListAsync();

            return Ok(dogs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var d = await _db.Dogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (d == null) return NotFound(new { message = "Perro no encontrado." });

            return Ok(new
            {
                d.Id,
                d.Name,
                d.Description,
                d.Breed,
                d.Size,
                d.EnergyLevel,
                d.IdealEnvironment,
                d.PhotoUrl,
                d.HealthStatus,
                d.Sterilized,
                d.IntakeDate,
                d.OriginTypeId
            });
        }

        public class DogUpsertForm
        {
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public string Breed { get; set; } = "";
            public string Size { get; set; } = "";
            public int EnergyLevel { get; set; } = 3;
            public string IdealEnvironment { get; set; } = "";
            public int OriginTypeId { get; set; }
            public string? HealthStatus { get; set; }
            public bool Sterilized { get; set; }
            public IFormFile? PhotoFile { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] DogUpsertForm form)
        {
            if (string.IsNullOrWhiteSpace(form.Name))
                return BadRequest(new { message = "Name es obligatorio." });

            if (form.OriginTypeId <= 0)
                return BadRequest(new { message = "OriginTypeId es obligatorio." });

            if (form.PhotoFile == null || form.PhotoFile.Length == 0)
                return BadRequest(new { message = "Debes subir una foto del perro." });

            var photoUrl = await _photos.SaveAsync(form.PhotoFile);

            var dog = new Dog
            {
                Name = form.Name.Trim(),
                Description = form.Description,
                Breed = form.Breed ?? "",
                Size = form.Size ?? "",
                EnergyLevel = form.EnergyLevel,
                IdealEnvironment = form.IdealEnvironment ?? "",
                OriginTypeId = form.OriginTypeId,
                HealthStatus = form.HealthStatus,
                Sterilized = form.Sterilized,
                PhotoUrl = photoUrl,
                IntakeDate = DateTime.UtcNow
            };

            _db.Dogs.Add(dog);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Perro creado.", id = dog.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] DogUpsertForm form)
        {
            var existing = await _db.Dogs.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return NotFound(new { message = "Perro no encontrado." });

            if (string.IsNullOrWhiteSpace(form.Name))
                return BadRequest(new { message = "Name es obligatorio." });

            if (form.OriginTypeId <= 0)
                return BadRequest(new { message = "OriginTypeId es obligatorio." });

            existing.Name = form.Name.Trim();
            existing.Description = form.Description;
            existing.Breed = form.Breed ?? "";
            existing.Size = form.Size ?? "";
            existing.EnergyLevel = form.EnergyLevel;
            existing.IdealEnvironment = form.IdealEnvironment ?? "";
            existing.OriginTypeId = form.OriginTypeId;
            existing.HealthStatus = form.HealthStatus;
            existing.Sterilized = form.Sterilized;

            
            if (form.PhotoFile != null && form.PhotoFile.Length > 0)
            {
                var url = await _photos.SaveAsync(form.PhotoFile);
                if (!string.IsNullOrEmpty(url))
                    existing.PhotoUrl = url;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Perro actualizado." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dog = await _db.Dogs.FirstOrDefaultAsync(x => x.Id == id);
            if (dog == null) return NotFound(new { message = "Perro no encontrado." });

            _db.Dogs.Remove(dog);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Perro eliminado." });
        }
    }
}
