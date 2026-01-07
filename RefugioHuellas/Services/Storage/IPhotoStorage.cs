using Microsoft.AspNetCore.Http;

namespace RefugioHuellas.Services.Storage
{
    public interface IPhotoStorage
    {
        /// Guarda la foto y devuelve la URL pública (por ejemplo "/uploads/..." ).
        /// Si file es null o vacío, devuelve null.
        Task<string?> SaveAsync(IFormFile? file);
    }
}
