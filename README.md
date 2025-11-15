# RefugioHuellas 🐾

RefugioHuellas es una aplicación web ASP.NET Core MVC (.NET 9) para la gestión de adopciones de perros en un refugio.
Incluye autenticación con Identity, control de roles (Admin / Usuario), evaluación de compatibilidad entre adoptante y perro, y administración completa de los registros.

Características principales
Autenticación y roles

Sistema de login y registro con ASP.NET Core Identity.

Roles:

Admin: Puede gestionar perros, revisar solicitudes y aprobar adopciones.

Usuario: Puede ver perros disponibles, llenar formularios de compatibilidad y enviar solicitudes.

#  Gestión de perros

Crear, editar, eliminar y listar perros (solo Admin).

Atributos principales:

Nombre, descripción,raza, tamaño, Nivel de energía, Entorno ideal, foto, salud, fecha de ingreso, esterilización.

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

# Requisitos

.NET 9 SDK

Git

SQLite (DB Browse for SQLite)

# Como correr el proyecto 

Clonar el repositorio:
git clone https://github.com/NotGuatas/RefugioHuellas.git
cd RefugioHuellas

Restaurar dependencias:
dotnet restore

Crear la base de datos:
dotnet ef database update

Ejecutar la aplicación:
dotnet run


Credenciales iniciales:

Email: admin@huellas.com  
Password: Admin123$


# Link para ver el proyecto deployado
https://refugiohuellas-2.onrender.com

Cuenta Admin para acceder:
mail: admin@huellas.com  
Password: Admin123$


