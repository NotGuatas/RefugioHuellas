# RefugioHuellas 🐾

RefugioHuellas es una aplicación web ASP.NET Core MVC (.NET 9) para la gestión de adopciones de perros en un refugio.
Incluye autenticación con Identity, control de roles (Admin / Usuario), evaluación de compatibilidad entre adoptante y perro, y administración completa de los registros.

Características principales
Autenticación y roles

Sistema de login y registro con ASP.NET Core Identity.

Roles:

Admin: Puede gestionar perros, revisar solicitudes y aprobar adopciones.

Usuario: Puede ver perros disponibles, llenar formularios de compatibilidad y enviar solicitudes.

#  Gestión de perros (CRUD completo)

Crear, editar, eliminar y listar perros (solo Admin).

Atributos principales:

Nombre, descripción, foto, salud, fecha de ingreso, esterilización.

Nueva información añadida:

# Raza 

Tamaño (Pequeño, Mediano, Grande)

Nivel de energía (1–5)

Entorno ideal (Departamento, Casa con patio, etc.)

Subida de imágenes al directorio wwwroot/uploads.

#  Solicitudes de adopción

Los usuarios pueden enviar una solicitud de adopción personalizada por perro.

Cada solicitud se evalúa con un formulario de compatibilidad dinámico.

#  Sistema de compatibilidad inteligente

Evaluación basada en rasgos de personalidad del adoptante y características del perro.

Se calcula un porcentaje de compatibilidad (0–100%) con un modelo de ponderaciones.

Evita duplicados: un usuario no puede solicitar el mismo perro más de una vez.

#  Panel administrativo avanzado

El Administrador tiene acceso a tres vistas clave:

Solicitudes de adopción:
Lista completa de solicitudes con compatibilidad, estado y fecha.

Mejores coincidencias:
Muestra los tres candidatos más compatibles por perro dentro de una ventana de tiempo.

Mejores candidatos:
Resume el mejor candidato por cada perro, con control de ventana temporal:

“Provisional” → ventana aún abierta (recibiendo solicitudes).

“Cerrada” → ventana finalizada; permite aprobar al mejor candidato.

# Ventana de adopción

Cada perro tiene una ventana temporal de evaluación (por defecto 7 días) desde su ingreso.

Durante la ventana:

Se pueden recibir solicitudes nuevas.

El sistema actualiza automáticamente los puntajes.

Una vez cerrada:

Ya no se aceptan nuevas solicitudes.

Solo queda disponible el mejor candidato para aprobación.

#  Flujo de adopción

El usuario inicia sesión.

Selecciona un perro disponible.

Llena el formulario de compatibilidad.

El sistema guarda la solicitud con un puntaje de compatibilidad.

Al finalizar la ventana, el admin aprueba al mejor candidato desde “Mejores Candidatos”.

# Estructura del proyecto

RefugioHuellas/
│
├── Controllers/
│   ├── DogsController.cs
│   ├── AdoptionApplicationsController.cs
│   └── CompatibilityController.cs
│
├── Models/
│   ├── Dog.cs
│   ├── AdoptionApplication.cs
│   ├── PersonalityTrait.cs
│   ├── AdoptionApplicationAnswer.cs
│   └── ViewModels/
│       ├── CompatibilityFormVm.cs
│       ├── AdminAdoptionVm.cs
│       ├── TopMatchVm.cs
│       └── BestMatchVm.cs
│
├── Services/
│   └── CompatibilityService.cs
│
├── Views/
│   ├── Dogs/
│   ├── AdoptionApplications/
│   └── Compatibility/
│
└── wwwroot/uploads/  ← imágenes subidas


# Requisitos

.NET 9 SDK

Git

SQLite (DB Browse for SQLite)

# Como correr el proyecto 

# Clonar el repositorio
git clone https://github.com/NotGuatas/RefugioHuellas.git
cd RefugioHuellas

# Restaurar dependencias
dotnet restore

# Crear la base de datos
dotnet ef database update

# Ejecutar la aplicación
dotnet run


# Credenciales iniciales (seed)

Email: admin@huellas.com  
Password: Admin123$


# Notas adicionales
Las imágenes subidas se guardan en wwwroot/uploads/
(esta carpeta está ignorada por Git para evitar subir archivos grandes).

El sistema usa Entity Framework Core con migraciones automáticas.

Al cerrarse la ventana de adopción, las solicitudes quedan bloqueadas para ese perro.

Compatible con futuras integraciones en React (API lista para consumir).

# Próximas mejoras (futuro)

Conexión con frontend React para versión moderna.

Agregar más preguntas al formulario de compatibilidad.

Sistema de notificaciones por correo al aprobar adopciones.

