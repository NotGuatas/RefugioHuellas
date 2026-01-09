using RefugioHuellas.Services.Storage;

public class LocalPhotoStorage : IPhotoStorage
{
    private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public async Task<string?> SaveAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext))
            throw new InvalidOperationException("Formato de imagen no permitido.");

        var root = Environment.GetEnvironmentVariable("UPLOAD_ROOT");
        var uploadsPath = string.IsNullOrWhiteSpace(root)
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")
            : root;

        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{fileName}";
    }
}
