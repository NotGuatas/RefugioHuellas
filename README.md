# RefugioHuellas 🐶
Aplicación web MVC para un refugio de perros con autenticación y roles (Admin/User),
CRUD de perros (solo Admin), solicitudes de adopción, e imágenes subidas a wwwroot/uploads.

## Requisitos
- .NET 8 SDK
- Git

## Puesta en marcha
Bash  
git clone https://github.com/NotGuatas/RefugioHuellas.git
cd RefugioHuellas
dotnet ef database update
dotnet run

Admin seed:

Email: admin@huellas.com

Password:
Perfiles de usuario con ASP.NET Core Identity.

Imágenes se guardan en wwwroot/uploads (carpeta ignorada en git por defecto).
